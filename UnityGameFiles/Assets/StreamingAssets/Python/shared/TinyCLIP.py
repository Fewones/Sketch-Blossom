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

    # Only preprocess for battle move shapes (black-on-white).
    # Drawing scene (plant_labels, upgrade_labels) needs the original colours.
    if item_id == "move_shapes":
        image = preprocess_drawing(raw_image)
    else:
        image = raw_image.convert("RGB")

    # Ähnlichkeiten berechnen
    scores = {}
    image_emb = get_image_embeddings(image).unsqueeze(dim=0)
    for label, emb in zip(labelMap.keys(), get_text_embeddings(list(labelMap.keys()))):
        score = torch.cosine_similarity(image_emb, emb.unsqueeze(dim=0))
        scores[labelMap[label]] = float(score)

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


