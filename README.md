# FINAL GAME IDEA: Sketch Blossum

Engine: Unity  
Platforms: PC/Mac (Steam), Tablet, Mobile  
Input Methods: Mouse (PC), Touch/Stylus (Tablet/Mobile)  
Genre: Drawing-Based Deck-Building Roguelike Auto-Battler  
Theme: Tamed Growth  
### Team Members
- Michael Dieterle - Project Lead
- Sanja Nikolic - ..
- Stefan - ..
- Marwa - ..

## Gameplay Loop

### World

**1.1**  
- Navigate 2D World → Choose encounter difficulty (1-3 enemies)
- Pre-Battle Deck Selection

**1.2** 
- Draw 5 random cards from your deck (max 10)
- Choose 3 to bring into battle
- Strategy: Cover type weaknesses


### Auto-Battle Combat

**2.1**
- Type triangle (strategy): Water > Fire > Grass > Water (2x or 0.5x Multiplier)
- Stats: Attack, Defense, HP
- Permanent death. Lost plants are gone forever (dead)


### Post-Battle: Tame or Wild Growth

**3.1** For EACH defeated enemy, choose ONE:  
- 🌱 TAME: Draw new plant (limited strokes) → New card
- 🌿 WILD GROWTH: Draw additions on existing card → Stats evolve


**(3.2)** Strategic Deck Building

- Max 10 cards - space is precious
- Invest in strong plants vs diversify deck?
- Risk permanent death vs safe progression


## Repeat → Reach and defeat boss

If not strong enough you start again from the begining

Key Tension: Every drawing choice matters. Every battle risks losing invested cards.

## Detail Gameplay diagram
```
┌─────────────────────────────────────────────────┐
│         START: Empty Deck (0/10 cards)          │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ENCOUNTER 1: Must TAME your first plant        │
│  → Draw plant (limited strokes)                 │
│  → System identifies: Water/Fire/Grass          │
│  → Card created with basic stats                │
│  → Deck: 1/10 cards                             │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  NAVIGATE 2D WORLD                              │
│  → See 3 paths with different difficulties:     │
│     • Easy (1 enemy, low stats)                 │
│     • Medium (2 enemies, medium stats)          │
│     • Hard (3 enemies, high stats)              │
│  → Choose based on deck strength                │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  PRE-BATTLE                                     │
│  → Draw 5 random cards from deck                │
│  → Choose 3 to bring into battle                │
│  → Strategy: Cover type weaknesses              │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  AUTO-BATTLE COMBAT                             │
│  → Your 3 plants vs Enemy plants                │
│  → Type advantage: Water>Fire>Grass>Water       │
│  → Damage = Attack × Type Multiplier - Defense  │
│  → Plants attack until one side has 0 HP        │
│  → Dead plants = PERMANENT LOSS                 │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  POST-BATTLE REWARDS                            │
│  → For EACH defeated enemy, choose ONE:         │
│                                                  │
│  🌱 TAME (Add new card):                        │
│     • Draw new plant (limited strokes)          │
│     • System identifies type                    │
│     • Basic stats assigned                      │
│     • Added to deck (max 10)                    │
│                                                  │
│  🌿 WILD GROWTH (Evolve existing):              │
│     • Choose plant from deck                    │
│     • Draw additions (thorns, leaves, etc.)     │
│     • System analyzes additions                 │
│     • Stats modified:                           │
│       - Thorns → +Attack, -Defense              │
│       - Leaves → +HP, -Attack                   │
│       - Flowers → Balanced boost                │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  STRATEGIC DECISION                             │
│  → Do I need more cards (variety)?              │
│  → Or stronger cards (evolution)?               │
│  → Deck space is limited (10 max)               │
│  → Dead cards = wasted evolution investment     │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
        Repeat until Boss Encounter
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  BOSS FIGHT                                     │
│  → Multiple tough enemies                       │
│  → Win = Game Complete                          │
│  → Lose = Roguelike reset                       │
└─────────────────────────────────────────────────┘

```

## 🛠️ **UNITY TECHNICAL ARCHITECTURE**

### **Core Systems to Build**
```
SketchBloom_Unity/
├── Assets/
│   ├── Scripts/
│   │   ├── Drawing/
│   │   │   ├── DrawingCanvas.cs          ← Cross-platform input
│   │   │   ├── StrokeRecorder.cs         ← Track strokes
│   │   │   ├── PlantAnalyzer.cs          ← Type detection
│   │   │   └── FeatureDetector.cs        ← Evolution analysis
│   │   ├── Cards/
│   │   │   ├── PlantCard.cs              ← Card data structure
│   │   │   ├── CardGenerator.cs          ← Create from drawing
│   │   │   └── CardEvolution.cs          ← Apply wild growth
│   │   ├── Combat/
│   │   │   ├── BattleManager.cs          ← Auto-battle logic
│   │   │   ├── TypeAdvantage.cs          ← Water>Fire>Grass
│   │   │   └── DamageCalculator.cs       ← Attack/Defense
│   │   ├── Deck/
│   │   │   ├── DeckManager.cs            ← Max 10 cards
│   │   │   └── CardSelection.cs          ← Choose 3 for battle
│   │   └── Progression/
│   │       ├── EncounterManager.cs       ← World navigation
│   │       └── RewardScreen.cs           ← Tame vs Wild Growth
│   ├── Scenes/
│   │   ├── MainMenu.scene
│   │   ├── DrawingScene.scene            ← Where drawing happens
│   │   ├── BattleScene.scene             ← Auto-battle visualization
│   │   └── WorldMap.scene                ← 2D navigation
│   └── Prefabs/
│       ├── PlantCard.prefab
│       └── Enemy.prefab
```