## How to Use the TinyCLIP Model

The TinyCLIP Model is used for zero-shot image classification. Players draw freely and the AI classifies drawings against label descriptions (e.g. plant types, upgrade categories). The model runs as a Python FastAPI server alongside Unity.

### Windows
1. Open the project in Unity. Python packages are automatically installed on first launch (you'll see a debug message while installation is in progress). Installation is handled by the `PythonDownloader` script in `UnityGameFiles/Assets/Editor`, which downloads from `https://github.com/Fewones/Sketch-Blossom/releases/tag/sketchblossom-python-win` and extracts to `Sketch-Blossom/UnityGameFiles/Assets/Python`.
2. Run the project. A terminal window will appear showing Python server logs. The game waits for the TinyCLIP server to be ready before proceeding. The `PythonServerManager` script in `Assets/Scripts/Model` handles starting the server by running `TinyCLIP.py` from `Assets/Python/shared`.
3. Draw something and submit. The terminal shows each label with its confidence score. The highest-ranked label is returned to Unity via the `ModelManager` HTTP client.
4. **Plant classification:** After receiving the label and score from the server, `PlantRecognitionSystem.AnalyzeDrawing()` maps the label to a plant type. A score >= 0.2 is valid; >= 0.27 indicates a good result.

### Unix (not tested)
Note: The release assets at https://github.com/Fewones/Sketch-Blossom/releases/tag/sketchblossom-python might also work the same as on Windows.
1. If you haven't already, install Python and run `pip install virtualenv`.
2. In `Sketch-Blossom/UnityGameFiles/Assets/Python` create a virtualenv named `macos-latest` or `ubuntu-latest` (`virtualenv macos-latest` on macOS; `python3 -m venv ubuntu-latest` on Linux).
3. Activate the virtualenv: `source macos-latest/bin/activate` or `source ubuntu-latest/bin/activate`.
4. Run `pip install torch torchvision`.
5. Run `pip install -r ../../../requirements.txt`.
6. Open the project in Unity.
7. Run the project. A terminal may appear with server logs. The game waits for the server before proceeding.
8. Draw something and submit.

## CLIP AI Plant Detection

Plant recognition is powered by **TinyCLIP** (`wkcn/TinyCLIP-ViT-39M-16-Text-19M-YFCC15M`), a zero-shot image classification model. Unlike rule-based systems that check for specific shapes and colors, CLIP compares the player's drawing against natural language descriptions of each plant type, making detection much more true to reality.

**How it works:**
1. Player draws a plant using the **full color palette** (any colors, not limited to RGB primaries)
2. The drawing is sent to the TinyCLIP FastAPI server (running locally on port 8000)
3. The model computes image embeddings and compares them against text embeddings for each plant description
4. Cosine similarity scores determine which plant the drawing most closely resembles
5. The best match and its confidence score are returned to Unity

**Label Maps** (`Assets/Python/shared/labelMaps.json`):
- **plant_labels**: 9 plant descriptions (e.g., "a shining sunflower", "a cactus with many spines")
- **upgrade_labels** (per plant): 3 upgrade stat categories (power/defense/health) + 1 blank

This approach accepts a much wider variety of drawing styles and rewards artistic, detailed drawings rather than requiring rigid shape patterns.
