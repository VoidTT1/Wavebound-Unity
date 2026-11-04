# Wavebound-Beta-v0.2

**WaveBound** - 3D FPS игра, созданная на Unity(2022.3.62f3) 

Основной геймплей игры заключается в волнах или же раундах, в каждом раунде есть определённое количество разновидных врагов

### Browser
[Play on itch.io](<ITCH_IO_URL>)

### Gameplay Video
[Youtube Video](<YOUTUBE_URL>)

### Windows Version
[Download Windows build](<https://github.com/VoidTT1/Wavebound-Unity/releases/tag/v0.2-beta>)


![WaveBound Gameplay](Media/gameplay.gif)


## Геймплей
В самой игре тебе нужно покупать оружие и патроны к нему для того что бы отбиватся от врагов
Сейчас есть всего 4 вида оружия:
- Пистолет(Pistol),
- M249(Rifle),
- Smg(SMG),
- дробовик(Shootgun) покупаешь ты их в магазине.
  
С самого старта у тебя уже есть 100 монет в инвентаре(получать монеты нужно за убийство врагов), на них ты покупаешь пистолет цена которого ровно 100 монет, после покупки начинается 1-й раунд(для того что бы проходить раунды/волны нужно за отведённое время уничтожить определённое количество врагов которое записанно сверху справа на протяжении всего раунда)

### Враги:
 - Обычный(15 урона, 50hp) - 20 монет
 - Взрывающийся(Враг который при сближении с тобой взрывается нанося 45 урона, 30hp) - 30 монет
 - Враг со щитом(Когда есть щит идёт медленно, когда щит сломанн бежит с нормальной скоростью, в обоих случаях наносит 20 урона, 50hp у щита и 100hp у самого скелета) - 40 монет
 - Скульптура(враг на которого нужно смотреть что бы он не двигался, имеет повышенную скорость и урон в 25 едениц, 200hp) - 65 монет
 - Подземный(Обычный враг который вылезает из земли возле тебя, 15 урона, 50hp) - 30 монет
   
 - Босс(Имеет 4 способности: 
   - призыв миньёнов(призывает вокруг себя обычных врагов у которых 30hp и 15 урона), 
   - бросок камня(подбирает камень с земли и бросает его в игрока, при поподании немного отталкивает и наносит 30 урона), 
   - Таран(Остонавливается и после чего бежит по прямой в сторону игрока, при попадании отталкивает и наносит 25 урона),
   - Удар об землю(прыгает и ударяется об землю создавая удраную волну вокруг себя которая отталкивает и наносит 25 урона)
 25 урона с обычного удара и 5000hp) - 3000 монет

### Раунды:
 - 1 раунд :Обычные 6                                                                                      (6)
 - 2 раунд: Обычные(10), Взрывающиеся(3)                                                                   (13)
 - 3 раунд: Обычные(15), Взрывающиеся(5), Скульптуры(3), Враги со щитом(5)                                 (30)
 - 4 раунд: Обычные(15), Взрывающиеся(7), Скульптуры(4), Враги со щитом(10), Подземные(4)                  (40)
 - 5 раунд: Взрывающиеся(10), Враги со щитом(10), Подземные(10), Скульптуры(5), Босс(1)                    (35 + Босс)


## Текущие функции
- ScriptableObject/DataBase system(Inventory, Shop, weapons and items), Inerfaces(IDamageable, IWeaponState...)
- First person weapon system
- WorldItem(DataBase)
- Enemy navigation NavMesh
- Rigidbody movement
- Physics of destruction(физика разрушения)
- Enum classes
- Localization
- Music and sound effects
- Animation Event, Sub-State machine
- Notification system
- OverLap, RayCast
- C# Action events
- Custom Knockback/External velocity system


# ⚙️ Technical Showcase
## Repository Structure

```text
WaveBound
  Assets
    Attacks
      OverlapAttack.cs
      kick.cs
    Camera
      Camera.cs
  Media
 
```

## Enemy AI
В этом проекте я старался делать все системы и скрипты правильно, а значит они должны быть максимально гибкими и система ИИ врагов не стала исключением

## Combat & Damage

- Interface-based damage handling
- `Physics.OverlapSphereNonAlloc`
- LayerMask filtering
- Obstacle checks
- Animation Event driven attack timing
- Area damage
- Physics interactions


## Custom Knockback System

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

## Boss Abilities

Отдельно я бы хотел выделить босса, а точнее его аддоны, потому что в основе лежит привычный EnemyChase(погоня и атака), по сути любой нпс может стать боссом если к нему прикрепить опрделённые способности
То есть всю систему нпс я постарался сделать максимально гибкой
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
