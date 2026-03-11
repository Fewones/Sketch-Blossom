# DevBlog: March 2026 Update

**Date:** March 11, 2026

It's been a productive sprint for Sketch Blossom! This update covers some big changes — our battle scene now uses AI to recognize your drawn moves, enemies come in different difficulty tiers, plant detection got smarter, and you can finally change your brush size. Let's get into it.

---

## Battle Scene Now Uses CLIP for Move Recognition

This is the headline change. The battle scene's move detection has been **completely reworked to use TinyCLIP** — the same AI model we use for plant recognition — instead of relying purely on geometric heuristics.

Previously, the game tried to figure out what you drew using stroke analysis alone: checking if your lines were circular, spiky, curved, etc. It worked, but it struggled with shapes that look similar (circles vs. spirals, squares vs. zigzags). Now, your drawing gets sent to the TinyCLIP server, which compares it against 13 natural-language shape descriptions — things like *"a black zigzag line on white paper, sharp angular back-and-forth turns like a lightning bolt"* or *"a black spiral on white paper, a curved line winding around itself inward in a coil like a snail shell"*.

Here's how it works under the hood:

- When you finish drawing a move, the canvas is captured as a 512x512 texture with a transparent background.
- The image is sent to our TinyCLIP FastAPI server, which preprocesses it into a high-contrast black-on-white image and runs it through the model.
- TinyCLIP returns confidence scores for all 13 shape labels. Since probability is split across many labels, raw scores are normalized (multiplied by 2.5) so that a strong match lands well above the 0.5 confidence threshold.
- The old geometric detection now serves as a **fallback only** — it kicks in when the TinyCLIP server isn't available.

We also added **confusion-aware matching** for shapes that TinyCLIP tends to mix up (circle/spiral, square/triangle, arrow/checkmark). When the primary shape is detected, known confusable shapes get a 65% score boost so the system degrades gracefully instead of picking the wrong move entirely.

The result: move recognition is noticeably more accurate and consistent. Drawing a zigzag actually registers as a zigzag now, even if your lines aren't perfectly sharp.

---

## Three Difficulty Levels

Encounters on the world map now come in **three difficulty tiers**: Easy, Medium, and Hard.

- **Easy** — You face 1 enemy plant.
- **Medium** — You face 2 enemy plants in sequence.
- **Hard** — You face 3 enemy plants back-to-back.

Each difficulty tier is **color-coded on the world map** — green for Easy, orange for Medium, red for Hard — so you can pick your battles at a glance. Before engaging, a **battle preview popup** shows the difficulty label, a star rating, the number of plants you'll face, and each plant's element type.

Higher difficulty also scales enemy stats. A difficulty multiplier (`1.0 + (difficulty - 1) * 0.15`) is applied to HP, Attack, and Defense, so Hard encounters hit harder and take more punishment on top of having more plants to get through.

In multi-plant encounters, defeating one plant triggers a "PLANT DEFEATED!" message showing how many remain, then the next plant loads in with fresh HP and new procedurally generated art — all without leaving the battle scene. Every enemy plant type now has **3 distinct art variations** (27 total procedural artworks), so encounters feel more varied even when you're fighting the same plant species.

---

## Smarter Plant Detection with Better Labels

Our TinyCLIP-based plant recognition got a round of meaningful improvements focused on **label quality** and **color awareness**.

The core issue: TinyCLIP was sometimes detecting the wrong plant type — for example, identifying a red fire rose drawing as a bubble flower. The model is only as good as the text descriptions it compares against, so we rewrote the labels in `labelMaps.json` to be more specific and include explicit color cues:

- *"a red tulip emitting flames"* became *"a red tulip with orange flames and fire"*
- *"a shining sunflower"* became *"a bright yellow sunflower with golden petals radiating light"*
- *"underwater corals"* became *"blue and purple underwater corals branching out"*
- *"a small patch of grass"* became *"a small patch of green grass blades"*

Beyond better labels, we added a **color-aware re-ranking system** on the server side. After TinyCLIP returns its initial confidence scores, the server now extracts the dominant hue from the drawing (red, green, or blue) and boosts or penalizes scores based on whether a plant's element color matches. So if you draw something clearly red, fire-type plants get a confidence boost while water-type plants get pushed down. This dramatically reduces misclassifications across element boundaries.

Images are also now composited onto white backgrounds server-side before classification, giving TinyCLIP consistent contrast regardless of how the drawing was captured.

---

## Adjustable Brush Width

A simple but highly requested feature — you can now **change your brush size** while drawing.

A new slider control has been added to the drawing canvas UI. Dragging it adjusts the line width in real time, and the slider handle itself scales to give you a visual preview of the brush thickness. This works everywhere you draw: the main **Drawing scene**, the **Tame scene** (for customizing captured plants), and the **Wild Growth scene** (for upgrades).

The implementation ties a `Slider` component to the `lineWidth` property on the canvas. The handle resizes dynamically based on the slider value so you always know roughly how thick your strokes will be before you put pen to canvas.

One related fix worth noting: in the battle scene specifically, brush size is kept at 8px rather than the player's custom width. This is intentional — thinner strokes give TinyCLIP cleaner shape outlines to classify, which keeps move recognition accurate.

---

## What's Next

We're continuing to polish the drawing experience and battle flow. Up next on our radar:

- Mid-battle plant switching
- Smarter enemy AI with type-awareness
- Audio and animation polish
- Tutorial system for new players

Thanks for following along — more updates soon!
