# Load model directly
from transformers import AutoProcessor, AutoModelForZeroShotImageClassification
import torch
from torchvision import transforms
from torchvision.io import read_image
from fastapi import FastAPI, UploadFile
from PIL import Image
from io import BytesIO
import uvicorn
import json
import os
import numpy as np

processor = AutoProcessor.from_pretrained("wkcn/TinyCLIP-ViT-39M-16-Text-19M-YFCC15M")
model = AutoModelForZeroShotImageClassification.from_pretrained("wkcn/TinyCLIP-ViT-39M-16-Text-19M-YFCC15M")
model.eval()


label_map_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "labelMaps.json")
with open(label_map_path) as labelMapJson:
    labelMaps = json.load(labelMapJson)


def get_text_embeddings(labels):
    inputs = processor(text=labels, return_tensors="pt", padding=True)
    text_embeddings = model.get_text_features(**inputs)
    return text_embeddings

def get_image_embeddings(image):
    inputs = processor(images=[image], return_tensors="pt", padding=True)
    image_embeddings = model.get_image_features(**inputs)
    return image_embeddings[0]

# Maps each plant label (value in labelMaps) to its expected element colour.
# Used to boost CLIP scores when the drawing's dominant colour matches.
PLANT_ELEMENT = {
    "fire rose":    "red",
    "flame tulip":  "red",
    "sunflower":    "red",
    "cactus":       "green",
    "vine flower":  "green",
    "grass sprout": "green",
    "water lily":   "blue",
    "coral bloom":  "blue",
    "bubble flower": "blue",
}

def get_dominant_hue(image):
    """Return 'red', 'green', 'blue', or None from the non-background pixels."""
    arr = np.array(image.convert("RGBA"))
    # Mask out near-white and near-transparent pixels (background)
    not_white = ~((arr[:,:,0] > 220) & (arr[:,:,1] > 220) & (arr[:,:,2] > 220))
    not_transparent = arr[:,:,3] > 30
    mask = not_white & not_transparent
    if mask.sum() < 20:
        return None
    r = arr[:,:,0][mask].astype(float).mean()
    g = arr[:,:,1][mask].astype(float).mean()
    b = arr[:,:,2][mask].astype(float).mean()
    # Require a clear winner — the dominant channel must lead by a margin
    margin = 15
    if r > g + margin and r > b + margin:
        return "red"
    elif g > r + margin and g > b + margin:
        return "green"
    elif b > r + margin and b > g + margin:
        return "blue"
    return None

def apply_color_reranking(scores, dominant_hue, boost=0.04, penalty=0.03):
    """Adjust CLIP scores based on whether the plant's element matches the drawing colour."""
    if dominant_hue is None:
        return scores
    adjusted = {}
    for label, score in scores.items():
        expected = PLANT_ELEMENT.get(label)
        if expected == dominant_hue:
            adjusted[label] = score + boost
        elif expected is not None and expected != dominant_hue:
            adjusted[label] = score - penalty
        else:
            adjusted[label] = score
    return adjusted

app = FastAPI()

@app.get("/health")
def health():
    print("Ready abgefragt")
    return {"status": "ok"}

def preprocess_drawing(image):
    """Convert a drawing image so the shape is maximally visible to CLIP.

    Unity sends coloured strokes on a transparent (or dark) background.
    CLIP works best with high-contrast images, so we:
      1. Composite onto a white background (removes transparency).
      2. Convert to greyscale.
      3. Threshold so strokes become solid black on pure white.
    This makes the shape dominate the image instead of background noise.
    """
    # Composite onto white
    white_bg = Image.new("RGBA", image.size, (255, 255, 255, 255))
    if image.mode == "RGBA":
        white_bg.paste(image, mask=image.split()[3])
    else:
        white_bg.paste(image)
    rgb = white_bg.convert("RGB")

    # Greyscale + threshold: anything darker than mid-grey becomes black
    grey = rgb.convert("L")
    bw = grey.point(lambda x: 0 if x < 180 else 255, mode="1")
    return bw.convert("RGB")


@app.post("/predict/{item_id}")
async def predict(file: UploadFile, item_id: str):
    # Entpacke labelMaps für gewünschte labelMap
    labelMap = labelMaps[item_id]
    # Warte auf ein Image
    raw_image = Image.open(BytesIO(await file.read())).convert("RGBA")

    # For move shapes: convert to black-on-white for max shape contrast.
    # For plant/upgrade labels: composite onto white background to remove
    # transparency while keeping the original stroke colours for CLIP.
    if item_id == "move_shapes":
        image = preprocess_drawing(raw_image)
    else:
        # Composite onto white so CLIP sees colours on a clean background
        white_bg = Image.new("RGBA", raw_image.size, (255, 255, 255, 255))
        if raw_image.mode == "RGBA":
            white_bg.paste(raw_image, mask=raw_image.split()[3])
        else:
            white_bg.paste(raw_image)
        image = white_bg.convert("RGB")

    # Ähnlichkeiten berechnen
    scores = {}
    image_emb = get_image_embeddings(image).unsqueeze(dim=0)
    for label, emb in zip(labelMap.keys(), get_text_embeddings(list(labelMap.keys()))):
        score = torch.cosine_similarity(image_emb, emb.unsqueeze(dim=0))
        scores[labelMap[label]] = float(score)

    # For plant detection, re-rank using the drawing's dominant colour so that
    # e.g. a red drawing boosts fire plants and penalises water plants.
    if item_id == "plant_labels":
        dominant_hue = get_dominant_hue(raw_image)
        print(f"Dominant hue detected: {dominant_hue}")
        scores = apply_color_reranking(scores, dominant_hue)

    # Bestes Label bestimmen
    best_label = max(scores, key=scores.get)
    best_score = scores[best_label]

    print("\nErgebnis:")
    print("Bestes Label:", best_label)
    print("Scores pro Label:")
    for k, v in scores.items():
        print(f"  {k}: {v:.4f}")
    return {"label": best_label, "score": best_score, "all_scores": scores}

uvicorn.run(app, host="127.0.0.1", port=8000)


