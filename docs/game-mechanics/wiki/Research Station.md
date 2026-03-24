# Research Station

Research Station
Conducts Novice Research to unlock new technologies.
Research stations are necessary for unlocking all research tiers.
Dimensions
2×2 tiles
Category
Stations
Type
Science Buildings
Power
-60 W
Heat
+
1.13 kDTU/s
Overheat
at
75 °C
/
167 °F
Decor
0
Requires
50 kg
Dirt
per research point
Duplicant
operation
Storage Capacity
Dirt 750
kg
Effects
Produces
Novice Research
Points
Auto In
Green
: Enable
Red
: Disable
Other Properties
Susceptible to
flooding
Materials
Metal Ore
400 kg
Research Station
operated by
Duplicants
will produce
Novice Research
Points necessary to unlock new
technologies
. It has to be supplied with
Dirt
to function.
Research Station is available at the start of the game and is needed for every research node in the game.
Usage
Research Station is mandatory to progress deeper on the tech tree. It is usually built early and used constantly as most of the researches in the first row are very useful. Multiple Research Stations can work on the same
Research Task
at the same time, however doing that early on is very likely to lock the entire
colony
into researching, as it not only needs operating duplicants but also other ones delivering
Dirt
.
Duplicants with high
Science
attribute are the most useful on the Research Station. At the start of the game, picking Duplicant with the highest Science possible regardless of their
Decor
expectation is recommended as increasing Decor near the Research Station is easy (by placing it near the
Printing Pod
for example, it also will provide
lit workspace
bonus).
Using Duplicants with low Science can still be a strategy, and the increase in this stat they might get by researching will help them level up faster later on.
A Duplicant with high research skill will automatically replace another Duplicant with low research skill at Research Station.
Mechanics
The Research Station uses 50 kg of Dirt to produce a research point. Its base material consumption rate is 3333.3… g/s. Dividing 50 kg with the consumption rate gives 15 s per research point produced in the base case where a Duplicant with 0 science works at an unlit Research Station outside a Laboratory room.
The consumption rate (
m
˙
) varies for each Research Station depending on whether it's in a Laboratory Room and the
science
attribute of the Duplicant using it (
S
) as well as whether they have the Lit Workspace buff. You can calculate it using the following equation:
m
˙
=
3
3
3
3
.
3
‾
g
s
⋅
(
1
+
0
.
1
3
‾
⋅
S
+
0
.
0
5
⋅
L
i
t
+
0
.
0
3
‾
⋅
L
a
b
)
where Lit and Lab is 1 if the respective buff is applied and 0 if not.
The Duplicant's science attribute includes buffs and debuffs from
Skills
, traits and other states.
Note, that even if the consumption rate should be lower based on the equation due to the Duplicant's negative skill (for example because of
Zombie Spores
), the minimum value is 2777.5 g/s.
As shown, skill points, Lit Workspace and the room buff only contribute a 13.3%, 5% and 3.3% additive bonus as opposed to the usual 40%, 15% and 10% they do with other buildings.
Another way to formulate the previous equation, which is conceptually closer (but is mathematically equivalent) to how the game calculates it uses a base rate of 1111.1… g/s and applies a hidden 200% buff to the Research Station:
m
˙
=
1
1
1
1
.
1
‾
g
s
⋅
(
1
+
2
+
0
.
4
⋅
S
+
0
.
1
5
⋅
L
i
t
+
0
.
1
⋅
L
a
b
)
A Duplicant with a high, but practically achievable science attribute of 30 at an ideally situated Research Station would produce a point every 3 seconds.
Duplicants are considered to be on the bottom-left tile of the structure for purposes such as Decor and Lit Workspace.
If multiple Research Stations are working on a research and one of them finishes the last Novice Research needed, all other Novice Research production will be halted, however they can be resumed and used for another research task later.
Blueprints
Available blueprints
Retro LED Research Station
Notes
Modders Note:
The values 1 and 2 in the equation above are static values written into the code instead of variables.  These static values are what make the other percentage modifiers 1/3rd as beneficial as they are for other buildings.
ResearchCenter.OnWorkTick - The Lit Workspace bonus is written that the minimum lit modifier value is 1.
ResearchCenter.OnWorkTick - The speed float adds 1 + skill modifier + lit modifier
Unknown Location - <Unfound third static value>
History
RP-379337
: Fix
lit workspace buff
not correctly applying when researching.
See also
Super Computer
Virtual Planetarium
Telescope
v
·
d
Buildings
Base
Oxygen
Power
Food
Plumbing
Ventilation
Refinement
Medicine
Furniture
Stations
Utilities
Automation
Shipping
Rocketry
Radiation
Artifact Analysis Station
Atmo Suit Checkpoint
Atmo Suit Dock
Blastshot Maker
Botanical Analyzer
Clothing Refashionator
Crafting Station
Data Miner
Exosuit Forge
Farm Station
Geotuner
Grooming Station
Jet Suit Checkpoint
Jet Suit Dock
Lead Suit Checkpoint
Lead Suit Dock
Materials Study Terminal
Milking Station
Orbital Data Collection Lab
Oxygen Mask Checkpoint
Oxygen Mask Dock
Power Control Station
Remote Controller
Remote Worker Dock
Research Station
Shearing Station
Skill Scrubber
Soldering Station
Super Computer
Telescope
Textile Loom
Virtual Planetarium
Virtual Planetarium
see all buildings
