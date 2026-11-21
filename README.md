# Wavebound-Beta-v0.2

## Project Status

**Current stage:** Beta-v0.2


**WaveBound** - 3D FPS игра, созданная на Unity(2022.3.62f3) 

Основной геймплей игры заключается в волнах или же раундах, в каждом раунде есть определённое количество разных врагов


## Browser
[[ Play on itch.io ]](<https://void-t.itch.io/wavebound>)

## Windows Version
[[ Download Windows build ]](<https://github.com/VoidTT1/Wavebound-Unity/releases/tag/v0.2-beta>)



## Gif:

![WaveBound Gameplay](Media/Gif/Gif1.gif) 
--------------------------------------------
![WaveBound Gameplay](Media/Gif/Gif2.gif)



## Controls

 - Movement - WASD
 - Interact/Pick up - E
 - Jump - space
 - Sprint - Shift
 - Crouch - LCtrl
 - Shoot - Left Mouse Button
 - Reload - R
 - Drop Item - G



## Геймплей
В самой игре тебе нужно покупать оружие и патроны к нему, чтобы отбиваться от врагов
Сейчас есть всего 4 вида оружия:
- Пистолет(Pistol),
- M249(Rifle),
- Smg(SMG),
- дробовик(Shotgun)

покупаешь ты их в магазине.
  
С самого старта у тебя уже есть 100 монет в инвентаре(получать монеты нужно за убийство врагов), на них ты покупаешь пистолет цена которого ровно 100 монет. После покупки начинается 1-й раунд(для того чтобы проходить раунды, нужно за отведённое время уничтожить определённое количество врагов которое записано сверху справа на протяжении всего раунда)

### Враги:
 - Обычный(15 урона, 50hp) - 20 монет
 - Взрывающийся(Враг который при сближении с тобой взрывается, нанося 45 урона, 30hp) - 30 монет
 - Враг со щитом(Когда есть щит идёт медленно, когда щит сломан бежит с нормальной скоростью, в обоих случаях наносит 20 урона, 50hp у щита и 100hp у самого скелета) - 40 монет
 - Скульптура(враг на которого нужно смотреть что бы он не двигался, имеет повышенную скорость и урон в 25 единиц, 200hp) - 65 монет
 - Подземный(Обычный враг который вылезает из земли возле тебя, 15 урона, 50hp) - 30 монет
   
 - Босс(Имеет 4 способности: 
   - призыв миньонов(призывает вокруг себя обычных врагов у которых 30hp и 15 урона), 
   - бросок камня(подбирает камень с земли и бросает его в игрока, при поподании немного отталкивает и наносит 30 урона), 
   - Таран(Останавливается и после чего бежит по прямой в сторону игрока, при попадании отталкивает и наносит 25 урона),
   - Удар об землю(прыгает и ударяется об землю создавая ударную волну вокруг себя которая отталкивает и наносит 25 урона)
 25 урона с обычного удара и 5000hp) - 3000 монет

### Раунды:
 - 1 раунд :Обычные 6                                                                                      (6)
 - 2 раунд: Обычные(10), Взрывающиеся(3)                                                                   (13)
 - 3 раунд: Обычные(15), Взрывающиеся(5), Скульптуры(3), Враги со щитом(5)                                 (30)
 - 4 раунд: Обычные(15), Взрывающиеся(7), Скульптуры(4), Враги со щитом(10), Подземные(4)                  (40)
 - 5 раунд: Взрывающиеся(10), Враги со щитом(10), Подземные(10), Скульптуры(5), Босс(1)                    (35 + Босс)


## Текущие функции
- ScriptableObject/Database system(Inventory, Shop, weapons and items), Inerfaces(IDamageable, IWeaponState...)
- First person weapon system
- WorldItem(Database)
- Enemy navigation NavMesh
- Rigidbody movement
- Physics of destruction(физика разрушения)
- Enum classes
- Localization
- Music and sound effects
- Animation Event, Sub-State machine
- Notification system
- Overlap, Raycast
- C# Action events
- Custom Knockback/External velocity system


# Technical section

## Repository Structure

```text

 Assets/
   Scripts/
     Attacks/
     - OverlapAttack.cs
     - kick.cs
     Camera/
      - Camera.cs
     NpcAI/
      - EnemyChase.cs
      - DefaultEnemy.cs
       Addons/
        - BlockedNpcController.cs
         EnemyHealth/
          - EnemyHealth.cs
          - BlockedNPCHealth.cs
         BossAddons/
          - FootStepAttackAddon.cs
          - StoneProjectile.cs
     Physics/
      - Exploison.cs
      - DestroyObj.cs
     Portals/
      - Portal.cs
      - Teleporter.cs
 
```



## Enemy AI
[EnemyChase.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/NpcAI/EnemyChase.cs)
В данном скрипте я использовал enum классы, для того что бы добавить врагу гибкости, по сути для его работы нужны: базовые компоненты(collider,rb), navMesh, скрипты: EnemyChase(или же DefaultEnemy) и скрипт атаки, хочешь что бы враг умел таранить? пожалуйста, делаешь скрипт и в нём вызываешь BeginExternalAction() что бы остановить работу enemyChase, и когда закончил включаешь обратно при помощи EndExternalAction(), делать так(        enemyChase.currentState = EnemyChase.EnemyState.ExternalAction;) настоятельно не рекомендую, так ты обойдёшь методы SetState и EnterState где меняется состояние для нормального функционирования врага, 
лучше все таки использовать SetState(); и через него вызывать ExternalAction, или же BeginExternalAction.

Эта система помогла мне легче написать аддоны для других видов врагов, прямой пример: [FootStepAttackAddon.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/NpcAI/Addons/BossAddons/FootStepAttackAddon.cs) - это аддон для босса(да, в основе опять же лежит EnemyChase), перед стартом анимации активируется BeginExternalAction(Основной метод вызывается через Animation Event).

*По мимо этого в скрипте используются привычные NavMesh от Unity для задания направления, а так же в другой версии скрипта [DefaultEnemy.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/NpcAI/DefaultEnemy.cs) при помощи navMesh реализован патруль


## Combat & Damage

Основным оружием для игрока является огнестрел, различные винтовки и пистолеты.
Для стрельбы используется известная система RayCast и интерфейс IDamageable для нанесения урона. 
Так же реализован и трассер, который я создавал прямо в Unity при помощи Trail renderer

Основным оружием по мимо способностей и особенностей для Enemy является OverlapAttack, его можно просмотреть здесь --> [OverlapAttack.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/Attacks/OverlapAttack.cs)


## Physics(of destruction)

 - В самой игре есть взрывы, и при помощи этих взрывов можно разрушать заборы на карте(пока что это больше вторичная механика потому что разрушаются пока что только заборы), за сам взрыв отвечает скрипт [Exploison](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/Physics/Exploison.cs), в нём я ввёл необычную механику - для игрока используется AddKnockback(по сути мы сами отталкиваем игрока), создал я это потому что система движения для игрока конфликтовала с Unity rb.AddExplosionForce, по этому я объединил её с mover

Обычные предметы(с rb и col), а так же врагов отбрасывает через AddExplosionForce который в ходе этого проекта я изучал в первые :/

 - Разрушение объектов я сделал при помощи скрипта [DestroyObj.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/Physics/DestroyObj.cs) который я взял из (https://gist.github.com/ditzel/73f4d1c9028cc3477bb921974f84ed56), в нём я немного подправил некоторые части(некоторые модели не резались), а так же добавил возможность устанавливать размер новых частей в MakeGameobject


## Inventory & Weapon Systems

Это достаточно весомая система, к которой я пришёл не сразу, начал её разработку я в другом своём проекте - (ещё не выложил), там я познакомился со ScriptableObject, для нормальной работы инвентаря мне нужно было создавать для каждого предмета свою базу и вешать на него worlditem, после чего сам инвентарь уже мог прочитать что за предмет мы взяли и сохранить его данные, из мира он соответственно удалялся.

В текущем проекте у меня то что лежит на сцене и то что лежит в инвентаре совершенно разное, то есть я могу повесить на какой ни будь cube, worlditem с базой данных оружия M249 и при подборе у меня в инвентаре будет M249, к этому я пришёл не сразу(хотя ну это же логично  :/)


## Other

### Оптимизация игры сделана через LOD, а также Oculision Culling

Так же в ходе проекта остались механики, которые я не добавил в Beta:
- [kick.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/Attacks/kick.cs) - удар ногой, удар который должен откидывать врагов активируя ragdoll от unity, а так же разрушать предметы, я это не ввёл потому что она ещё до конца не доработана
- [Portal.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/Portals/Portal.cs) - порталы, какое то время я экспериментировал со скриптами которые взял из интернета, хотел при помощи порталов сделать бесконечное пшеничное поле, идею бросил из за не реализуемости, вряд ли я буду снова  этим заниматься



# Project plans
Данный проект находится на 2м этапе разработки(Beta версия)
- 1.Alpha
- 2.Beta  <--
- 3.Demo
- 4.Realese


### В Demo версию я планирую добавить мультиплеер(через Mirror так как я уже с ним работал в []), новые предметы, новые оружия, новую музыку, увеличенные возможности настроек(назначение клавиш, выбор микрофона...), кат-сцены и механику удара ногой [kick.cs](https://github.com/VoidTT1/Wavebound-Unity/blob/main/Assets/Scripts/Attacks/kick.cs) с ragdoll как для врагов, так и для игрока(что бы можно было пинать друг друга в мультиплеере) + по возможности добавить и улучшить визуал игры(модели/UI) 

### В релизе я планирую сделать не 5, а 15 волн с совершенно другими врагами с уникальными способностями, а так же новыми боссами, новые карты, режимы сложности, новый визуал, новые механики, а так же обновление того что уже было
После Demo версии я планирую остановить разработку, по этому не знаю когда смогу сделать релиз, и сделаю ли вообще, в любом случае за этот проект я смог сильно прокачать свои навыки программирования и владением Unity :)


# Resources used
Множество реусрсов было взято из Asset Store, такие как: [3D Modern Menu UI](https://assetstore.unity.com/packages/tools/gui/3d-modern-menu-ui-116144) и [Free Quick Effects Vol. 1](https://assetstore.unity.com/packages/vfx/particles/free-quick-effects-vol-1-304424)

 Models:
- [RetroWeaponPack](https://kuptchi.itch.io/f) (Изначально он создан для Unreal Engine, но у меня получилось его настроить под Unity и скажу так, это было не просто)
- [StylizedSkeleton](https://hotstrikestudio.itch.io/free-stylized-skeleton)

Scripts and Systems:
 - [Outline](https://github.com/chrisnolet/QuickOutline)
 - [MeshDestroy](https://gist.github.com/ditzel/73f4d1c9028cc3477bb921974f84ed56)

 

Все остальные ресурсы так же были бесплатными и без авторского права(ссылок уже вспомню)



# Developer

**Solo Developer - VoidTT1**


## License

Исходный код, представленный в этом репозитории, предназначен для портфолио и демонстрационных целей
