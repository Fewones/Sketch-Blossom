# DevBlog: March 2026 Update

**Date:** March 11, 2026

It's been a productive sprint for Sketch Blossom! This update covers a batch of improvements spanning our AI pipeline, drawing tools, and overall stability. Here's what changed.

---

## Python Downloader Overhaul

The biggest area of work this cycle was the **PythonDownloader** — the editor tool that bootstraps our embedded Python environment (used to run the TinyCLIP AI server). It was plagued by timeout errors, painfully slow downloads, and silent failures. We tackled it end-to-end:

- **Fixed HTTP timeout errors** — The original downloader was timing out on the ~244 MB Python zip. We increased timeouts and added resilience so the download no longer silently fails on slower connections.
- **Faster downloads** — We replaced `HttpClient` with Unity's native `UnityWebRequest`, bumped buffer sizes, and reduced UI overhead during the download. The result is dramatically faster setup times.
- **Real-time progress feedback** — Added `EditorUtility.DisplayProgressBar` so you can actually see download progress in the Unity Editor, plus 10%-interval logging in the console.
- **Git LFS support with fallback** — The downloader now checks for a local zip via Git LFS first, and only falls back to a GitHub Releases download if the local copy isn't available. This makes setup nearly instant for contributors who clone with LFS.
- **CPU-only PyTorch** — Switched to the CPU-only PyTorch build on Windows, which significantly shrinks the zip size. Since TinyCLIP inference is lightweight, GPU support isn't needed.
- **Fixed pip bootstrap** — Resolved a bug where pip failed to bootstrap correctly in the embedded Python environment, which was blocking dependency installation entirely.
- **CI cleanup** — Removed the unused `build_python.yaml` workflow and stripped out macOS/Ubuntu logic from `build_py.yml` since we're targeting Windows only for the embedded distribution.

## Improved Flower Detection & Transparent Sprites

Our TinyCLIP-based plant recognition had trouble accurately detecting flowers. We made two key fixes:

- **Better label mapping** — Updated `labelMaps.json` with refined labels that give TinyCLIP more signal for distinguishing flower drawings from other plant types.
- **Preprocessing improvements** — Enhanced the image preprocessing pipeline in `TinyCLIP.py` to improve classification accuracy across the board, with flowers seeing the biggest gains.
- **Transparent battle sprites** — Player-drawn sprites now render with transparent backgrounds in battle instead of the previous solid-color fill. This makes drawn plants look much cleaner when displayed on the battle scene.

## New Flood-Fill Tool

Drawing got a major quality-of-life upgrade with the addition of a **flood-fill (paint bucket) tool**:

- Players can now toggle between **draw mode** (freehand brush) and **fill mode** (tap to flood-fill a region with the selected color).
- The fill tool uses a standard flood-fill algorithm on the canvas texture, respecting existing drawn boundaries.
- The capture handler was updated so filled regions are properly included when the drawing is sent to TinyCLIP for classification.
- The fill tool was also wired into the **TameGrowth scene** so players can use it when customizing captured creatures too.

## World Map & Brush Width

Two feature branches were merged in this update:

- **World Map** (PR #63) — The world map exploration system is now integrated into the main branch, giving players a way to navigate between encounters.
- **Brush Width** (PR #78) — Players can now adjust their brush size when drawing, allowing for finer detail or broader strokes.

---

## What's Next

We're continuing to polish the drawing experience and battle flow. Up next on our radar:

- Mid-battle plant switching
- Enemy AI improvements with type-awareness
- Audio and animation polish
- Tutorial system for new players

Thanks for following along — more updates soon!
