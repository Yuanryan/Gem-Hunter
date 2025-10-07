# Gem Hunter Match - 2D Combat Puzzle Game

A strategic match-3 puzzle game with turn-based combat mechanics, featuring gem-based combat systems

## 🎮 How to Play

### 🚀 Play Online

**Play the game directly in your browser**: [https://galagala0.itch.io/gem-match-project](https://galagala0.itch.io/gem-match-project)

### Basic Gameplay

1. **Match Gems**: Swap adjacent gems to create matches of 3 or more
2. **Combat System**: Each match triggers combat against enemies
3. **Turn-Based Combat**: Player attacks first, then enemy attacks
4. **Win Condition**: Defeat the enemy by reducing their health to 0
5. **Lose Condition**: Your health reaches 0

### Gem Types & Effects

#### 🔴 Red Gems - Attack

- **Purpose**: Deal damage to enemies
- **Damage**: Base damage per gem + combo bonuses
- **Combo Bonus**: Extra damage when matching 4+ gems

#### 🔵 Blue Gems - Attack

- **Purpose**: Deal damage to enemies
- **Damage**: Base damage per gem + combo bonuses
- **Combo Bonus**: Extra damage when matching 4+ gems

#### 🟡 Yellow Gems - Attack

- **Purpose**: Deal damage to enemies
- **Damage**: Base damage per gem + combo bonuses
- **Combo Bonus**: Extra damage when matching 4+ gems

#### ⚪ White Gems - Healing

- **Purpose**: Restore player health
- **Healing**: 3 HP per white gem
- **Max Heal**: Cannot exceed maximum health
- **Timing**: Healing occurs after enemy attack

#### 🟢 Green Gems - Shield

- **Purpose**: Block incoming enemy damage
- **Shield**: 5 shield points per green gem
- **Max Shield**: Cannot exceed 50 shield points
- **Duration**: Shield lasts until consumed or turn ends

### Combat Mechanics

#### Player Turn

1. Match gems to build up damage, healing, and shield
2. Damage is calculated: `(Attack Gems × Base Damage) + Combo Bonus`
3. Attack gems include: Red, Blue, and Yellow gems
4. Combo bonus applies when matching 4+ gems total
5. White gems prepare healing for after enemy attack
6. Green gems prepare shield for enemy attack

#### Enemy Turn

1. Enemy deals pre-calculated damage (8-15 points)
2. Shield blocks damage first, then health
3. If shield is active, player plays defend animation
4. If no shield, player plays hurt animation
5. After damage, white gem healing is applied

#### Health Bar Animation

- **Foreground Bar**: Updates immediately when health changes
- **Background Bar**: Shrinks with delay for visual effect
- **Healing**: Both bars increase smoothly
- **Damage**: Foreground shrinks immediately, background follows

### Real-Time UI Display

- **Attack**: Shows current turn's damage output
- **Healing**: Shows pending healing amount
- **Shield**: Shows current shield value
- **Enemy Damage**: Shows next enemy attack damage

### Special Features

#### Enemy Shield Lock

- When objectives aren't completed, enemy health locks at 1 HP
- Complete all objectives to break the shield
- Red warning text appears: "Clear objective to break shield!"

#### Combo System

- **Threshold**: 3 gems minimum for combo
- **Bonus**: +2 damage per gem beyond threshold
- **Example**: 5 gems = base damage + (5-3) × 2 = base + 4 bonus damage

#### Death Prevention

- **No Healing When Dead**: White gems won't heal if health is 0
- **No Move Loss in Combat**: Running out of moves doesn't end combat
- **No Goal Loss in Combat**: Completing goals doesn't trigger defeat

## 🎯 Tips for Success

1. **Plan Your Moves**: Look for opportunities to create large matches
2. **Balance Resources**: Don't focus only on attack - healing and shields are crucial
3. **Attack Variety**: Use Red, Blue, and Yellow gems for damage - they all contribute equally
4. **Combo Strategy**: Try to match 4+ gems for bonus damage
5. **Shield Timing**: Build shields before enemy attacks
6. **Healing Strategy**: Use white gems when low on health
7. **Objective Priority**: Complete objectives to unlock enemy defeat

## 🎵 Audio Credits

### Sound Effects

- **Healing Magic**: [Healing Magic 4](https://pixabay.com/sound-effects/healing-magic-4-378668/) by [yodguard](https://pixabay.com/users/yodguard-12455005/) on Pixabay
- **Block Sound**: [Block](https://pixabay.com/sound-effects/block-6839/) by [soundmast123 (Freesound)](https://pixabay.com/users/freesound_community-46691455/) on Pixabay
- **Monster Attack**: [Monster Bite](https://pixabay.com/sound-effects/monster-bite-44538/) by [soundmast123 (Freesound)](https://pixabay.com/users/freesound_community-46691455/) on Pixabay
- **Player Hurt**: [Male Hurt 7](https://pixabay.com/sound-effects/male-hurt7-48124/) by [micahlg (Freesound)](https://pixabay.com/users/freesound_community-46691455/) on Pixabay
- **Attack Slash**: [RPG Slash](https://pixabay.com/sound-effects/search/rpg%20slash/) by [nekoninja (Freesound)](https://pixabay.com/users/freesound_community-46691455/) on Pixabay

### Background Music

- **Main Theme**: [Epic Battle](https://pixabay.com/music/main-title-epic-battle-francisco-samuel-123469/) by [Francis_Samuel](https://pixabay.com/users/francis_samuel-28842777/) on Pixabay

## 🎨 Asset Credits

### Base Game Framework

- **Unity Sample Project**: [Gem Hunter Match - 2D Sample Project](https://assetstore.unity.com/packages/essentials/tutorial-projects/gem-hunter-match-2d-sample-project-278941) by Unity Technologies

## 🛠️ Technical Features

### Combat System

- Turn-based combat with gem-based abilities
- Real-time damage calculation and display
- Shield mechanics with visual feedback
- Health bar animations with delayed background effects
- Sound effects for all combat actions

### UI System

- Real-time combat statistics display
- Dynamic health bar animations
- Enemy shield break warnings
- Combo damage calculation
- Pre-calculated enemy damage display

### Game Logic

- Multiple loss condition prevention
- Death state validation
- Objective-based enemy shield system
- Combo bonus calculations
- Shield consumption mechanics

## 🎮 Controls

- **Mouse/Touch**: Click and drag to swap adjacent gems
- **Match Detection**: Automatic gem matching and clearing
- **Combat**: Automatic turn progression

## 📋 System Requirements

- **Unity Version**: 2022.3.41f1 or compatible
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Platform**: Windows, Mac, Linux, Mobile

## 🚀 Getting Started

1. Open the project in Unity 2022.3.41f1
2. Ensure URP is configured
3. Load the main scene
4. Press Play to start playing!

## 📝 License

This project is based on Unity's free sample project and extends it with custom combat mechanics. All audio assets are used under Pixabay's license terms.

---

**Enjoy the game and may your gems be ever in your favor!** ✨
