# Wavebound-Beta-v0.2

**WaveBound** is a 3D first-person wave survival shooter developed in Unity and C#.

Fight through increasingly difficult enemy waves, earn resources, purchase weapons and upgrades, and prepare for the next round.

> **Project type:** Solo game development / Gameplay Programming Portfolio  
> **Current version:** Beta  
> **Platform:** Windows / WebGL *(if available)*

---

## 🎮 Play the Game

### Browser
[▶ Play on itch.io](<ITCH_IO_URL>)

### Windows
[⬇ Download latest Windows build](<GITHUB_RELEASE_URL>)

### Gameplay Video
[▶ Watch gameplay](<YOUTUBE_URL>)

---

## 🎥 Gameplay

![WaveBound Gameplay](Media/gameplay.gif)

> Replace this with a short 8–15 second GIF showing combat, enemy waves, weapon switching and preferably the boss.

---

## 📖 About the Project

WaveBound is a wave-based FPS focused on progressively more difficult combat encounters.

Each round introduces increased pressure through enemy variety and limited resources. Between and during waves, the player can obtain weapons, ammunition, upgrades and other items to prepare for upcoming encounters.

The current Beta contains **5 playable waves**, while the game architecture is designed to support a larger progression of waves and additional maps in future versions.

The current map is a farm environment, but the gameplay systems and progression are not tied to a specific location or enemy type.

---

## ✨ Current Features

- Wave-based round progression
- Multiple enemy types with unique behaviours
- Boss encounter with multiple abilities
- First-person weapon system
- Multiple weapon types
- Ammo and reload management
- Inventory and hotbar
- Item pickups and world items
- Shop and currency system
- Weapon upgrades
- Item spawning between / during rounds
- Enemy navigation using Unity NavMesh
- Rigidbody-based player movement
- Knockback and physics interactions
- Settings menu
- Localization
- Music and sound effects
- Animation Event based combat timing

---

# ⚙️ Technical Showcase

This repository contains **selected gameplay programming systems** from WaveBound.

The complete Unity project is intentionally not published. Some game systems, third-party assets and production files are excluded from the public repository.

The source code available here focuses on systems that demonstrate my C# and Unity gameplay programming experience.

---

## 🤖 Enemy AI & State Management

Enemy behaviour is controlled by a state-based architecture.

Typical enemy flow:

```text
Chase
  ↓
Attack
  ↓
Cooldown
  ↓
Chase
```

Special enemy and boss behaviours can temporarily take control of the enemy through an external-action state before returning control to the base AI.

### Examples

- `EnemyChase.cs`
- `BossController.cs`
- Boss ability addons
- Special enemy behaviour scripts

### Concepts demonstrated

- Enums for explicit gameplay states
- State transitions
- NavMesh pathfinding
- Separation between base AI and special abilities
- Animation Event integration
- Behaviour composition through additional components

---

## 💥 Combat & Damage

Combat systems use reusable damage interfaces instead of directly depending on concrete enemy or player classes.

```csharp
IDamageable
```

allows different objects to receive damage through the same contract.

The combat code demonstrates:

- Interface-based damage handling
- `Physics.OverlapSphereNonAlloc`
- LayerMask filtering
- Obstacle checks
- Animation Event driven attack timing
- Area damage
- Physics interactions

### Example systems

- `OverlapAttack.cs`
- `Explosion.cs`
- Enemy melee attacks
- Boss area attacks

---

## 💨 Custom Knockback System

The player movement controller directly manages Rigidbody velocity.

Because of this, using Unity's standard `AddExplosionForce` on the player would cause horizontal knockback velocity to be overwritten by the movement controller on the next physics update.

To solve this, player knockback is stored separately as an **external velocity**:

```text
Player input velocity
        +
External knockback velocity
        ↓
Final Rigidbody velocity
```

The external velocity gradually decays without disabling player control.

Normal physics objects still use Unity's standard:

```csharp
Rigidbody.AddExplosionForce(...)
```

This keeps player movement responsive while preserving physical knockback behaviour.

---

## 👑 Boss Ability System

The boss uses the same base movement / chase system as regular enemies while additional ability components temporarily take control when an ability is executed.

Abilities currently include systems such as:

- Charge attack
- Area / stomp attack
- Projectile attack
- Enemy summoning

This avoids duplicating the entire enemy controller for every boss ability.

---

## 🌊 Wave System

Rounds are configured as data rather than being hard-coded into individual enemy scripts.

Each wave can control values such as:

- Enemy composition
- Maximum simultaneous enemies
- Spawn interval
- Round duration
- Intermission duration
- Item spawn probability

This makes wave balancing possible without changing the underlying gameplay logic.

---

## 🎒 Inventory & Weapon Systems

WaveBound also contains a larger data-driven inventory and weapon-state system.

It supports:

- Inventory slots
- Hotbar weapon selection
- Ammo state persistence
- Weapon upgrades
- Item stacking
- Currency
- Shops
- World item pickups

The complete implementation is not included in this public repository, but it can be demonstrated during a technical interview.

---

# 📂 Repository Structure

```text
WaveBound/
│
├── Code/
│   ├── AI/
│   │   ├── EnemyChase.cs
│   │   ├── BossController.cs
│   │   └── ...
│   │
│   ├── Combat/
│   │   ├── OverlapAttack.cs
│   │   ├── Explosion.cs
│   │   └── ...
│   │
│   ├── Player/
│   │   ├── Mover.cs
│   │   └── ...
│   │
│   ├── Interfaces/
│   │   └── ...
│   │
│   └── ...
│
├── Media/
│   ├── gameplay.gif
│   └── screenshots/
│
└── README.md
```

> The repository contains selected source code for portfolio purposes and is not intended to be a complete playable Unity project.

---

# 🛠️ Technologies

| Technology | Usage |
|---|---|
| Unity | Game engine |
| C# | Gameplay and systems programming |
| Unity Physics | Collision, overlap queries and Rigidbody interactions |
| NavMesh | Enemy navigation and pathfinding |
| Animator / Animation Events | Attack and gameplay synchronization |
| TextMeshPro | UI |
| ScriptableObjects | Data-driven gameplay configuration |

**Unity version:** `<UNITY_VERSION>`

---

# 🎮 Controls

| Input | Action |
|---|---|
| `W A S D` | Movement |
| Mouse | Look |
| Left Mouse Button | Fire |
| `R` | Reload |
| `E` | Interact / pick up |
| `1–4` | Hotbar / weapon selection |
| `Shift` | Sprint |
| `Ctrl` | Crouch |
| `Space` | Jump |
| `Esc` | Settings / close menu |

> Adjust these controls to match the current Beta exactly.

---

# 📸 Screenshots

| Combat | Shop |
|---|---|
| ![](Media/screenshots/combat.png) | ![](Media/screenshots/shop.png) |

| Boss Fight | Inventory |
|---|---|
| ![](Media/screenshots/boss.png) | ![](Media/screenshots/inventory.png) |

---

# 🚧 Project Status

**Current stage:** Beta

The current public Beta includes the core gameplay loop and 5 waves.

Future development may include:

- More waves
- Additional maps
- New enemy types
- New weapons
- Additional boss encounters
- Gameplay balancing
- Performance improvements

---

# 📦 Download

The latest Windows build is available through GitHub Releases:

[⬇ Download WaveBound Beta](<GITHUB_RELEASE_URL>)

A browser version is available on itch.io if WebGL is supported:

[▶ Play WaveBound](<ITCH_IO_URL>)

---

# ℹ️ Source Code Notice

This repository is a **programming portfolio**, not the complete production project.

Only selected source code is publicly available.

The full Unity project contains additional proprietary project code and third-party assets that are not distributed through this repository.

Additional systems can be demonstrated during a technical interview.

---

# 👤 Developer

**Solo Developer — VoidTT1**

Main areas of work:

- Gameplay programming
- Enemy AI
- Combat systems
- Inventory / weapon systems
- UI systems
- Game progression
- Physics interactions
- Optimization and debugging

---

## License

The public source code in this repository is provided for portfolio and demonstration purposes.

Third-party assets used by the playable game remain subject to their respective licenses.
