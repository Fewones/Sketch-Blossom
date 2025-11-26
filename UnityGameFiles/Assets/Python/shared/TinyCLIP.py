# Load model directly
from transformers import AutoProcessor, AutoModelForZeroShotImageClassification
import torch
from torchvision import transforms
from torchvision.io import read_image
from fastapi import FastAPI, UploadFile
from PIL import Image
from io import BytesIO
import uvicorn

processor = AutoProcessor.from_pretrained("wkcn/TinyCLIP-ViT-39M-16-Text-19M-YFCC15M")
model = AutoModelForZeroShotImageClassification.from_pretrained("wkcn/TinyCLIP-ViT-39M-16-Text-19M-YFCC15M")
model.eval()


labels = ["flame tulip", "fire rose", "sunflower", "cactus", "vine flower", "grass sprout", "water lily", "coral bloom", "bubble flower"]

def get_text_embeddings(labels):
    inputs = processor(text=labels, return_tensors="pt", padding=True)
    text_embeddings = model.get_text_features(**inputs)
    return text_embeddings

def get_image_embeddings(image):
    inputs = processor(images=[image], return_tensors="pt", padding=True)
    image_embeddings = model.get_image_features(**inputs)
    return image_embeddings[0]

app = FastAPI()

@app.post("/predict")
async def predict(file: UploadFile):
    # Warte auf ein Image
    image = Image.open(BytesIO(await file.read())).convert("RGB")
    # Ähnlichkeiten berechnen
    scores = {}
    for label, emb in zip(labels, get_text_embeddings(labels)):
        score = torch.cosine_similarity(get_image_embeddings(image).unsqueeze(dim=0), emb.unsqueeze(dim=0))
        scores[label] = float(score)

    # Bestes Label bestimmen
    best_label = max(scores, key=scores.get)
    best_score = scores[best_label]

    print("\nErgebnis:")
    print("Bestes Label:", best_label)
    print("Scores pro Label:")
    for k, v in scores.items():
        print(f"  {k}: {v:.4f}")  
    return {"label": best_label, "score": best_score}

uvicorn.run(app, host="127.0.0.1", port=8000)


