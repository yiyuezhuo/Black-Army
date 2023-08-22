# Black Army

In November 1920, Nestor Makhno and its Anarchists troops, Black Army, is backstabed by Bolsheviks. Command Black Army to resist as long as possible, for the mother of Anarchist!

## Screenshots:

<img src="https://img.itch.zone/aW1hZ2UvMjE5Nzg1MS8xMjk4OTE5NS5wbmc=/original/z%2FDlKP.png">
<img src="https://img.itch.zone/aW1hZ2UvMjE5Nzg1MS8xMzE5OTk5OC5wbmc=/original/9IVw6V.png">
<img src="https://img.itch.zone/aW1hZ2UvMjE5Nzg1MS8xMjk4OTE5Ni5wbmc=/original/z6zZ9C.png">
<img src="https://img.itch.zone/aW1hZ2UvMjE5Nzg1MS8xMjk4OTE5NC5wbmc=/original/rU4BX4.png">

## Combat Resolution

### Principle

Basically, every troop will generate "chance" points, which will be modified by the leader's corresponding ability (guerrilla if one side has small troops (< 5000 men), otherwise operational.) and the current hex global situation. Then generated potential combat will be divided into 3 categories:

- Disadvantage: suffer heavy casualties but soften up the enemy at least.
- Balanced: take moderate casualties and soften up the enemy and sometimes will take more advance.
- Advantage: Launch an attack in good time, breaks the defensive line, or even surround and annihilate the local enemy. Smart counterattack to cancel enemy's advance... etc.

According to the Rule of Engagement specified by the player or AI, the commander will only tale a part of chances, they will also sometime miss good chances and use bad chances according to their ability and traits:

- Disengagement: Tried to evade all combat even very good chance.
- Passive: Exploit only advantage combat.
- Balanced (Attack): Use Advantage and balanced chance but reject disadvantage.
- Aggressive: Accepe all chance and corresponding heavy loss.

Usually, one side will choose balanced RoE while the opposite will use passive. Once one side is under heavy debuff introduced by area combat modifier (denoting one side lost important position and traffic line to the opposite), they will decide not to engage in this area and fallback to the next position (maybe a neighbor hex or a hex has good terrain for defense), so they will choose disengagement, while opposite may choose balanced or aggressive to exploit its local advantage.

High-mobility unit will generate more chance points, while they have the same "chance defense" points. So if successful, the high-mobility unit will make many advantage combats. But if they're "pinned down" by failed attack or counter-attack, they make no difference compared to foot infantry.

### Model

## TODO List

Currently, only navigation, basic movement & turn and re-orgnization are implemented. 

- [x] Fix WebGL export (IL2CPP issue) and 3D view selection issue (canvas's event camera)
- [x] Fix arrow shape and progression display issue due to shader. (Shader's Queue tag is not set to the expected "Transparent" used by other 2D renders).
- [x] Global tactical modifier driven combat resolution and UI binding
- [x] Animations of sub-turns resolving
- [ ] Add tactic value effect and move speed modifier.
- [ ] Add surrender when unit combat in bad situation.
- [ ] Inconsistency handling for defensive chance/asset.
- [ ] Simple Supply & Reforcement & Replacement
- [ ] VP & Global morale effects on Anarchists (especially move speed and supply)
- [ ] Simple AI
- [ ] Deep research on Anarchists and Red army's deployment and strength. 

## Acknowledgement

RatbyteBoss's [Free Hex Tile Game Assets](https://ratbyteboss.itch.io/hex-tile-assets)

Flags: https://en.wikipedia.org/wiki/Revolutionary_Insurgent_Army_of_Ukraine

Alexandre Skirda's [Nestor Makhno: Anarchy’s Cossack (The Struggle for Free Soviets in Ukraine 1917–1921)](https://theanarchistlibrary.org/library/alexandre-skirda-nestor-makhno-anarchy-s-cossack)

Michael Palij's [The Anarchism of Nestor Makhno, 1918-1921: An Aspect of the Ukrainian Revolution](http://www.ditext.com/palij/11.html)
