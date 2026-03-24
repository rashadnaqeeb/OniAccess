# Critter Morphs

All
critters
have a base variant. When a critter lays an egg, there's a chance that the egg will produce a different variant (so call "morph") of that critter.
For example, a
Hatch
can morph into a "Sage Hatch", "Stone Hatch", or "Smooth Hatch". Below you find the initial chances for each morph. See
Egg Mechanics
to see how a critters' egg chances can change over time.
Notes:
In the tables below:
An underlined value means that this chance can be changed through diet or temperature.
An empty cell means that the morph is not possible.
Each morph has a different egg. So you don't need to wait for an egg to hatch to see what morph you'll get.
The morph chance can be seen in the critter's info window under "Status" -> "Egg Chances".
Morphs
Hatch
Default Egg Chance
Hatchling Egg
Stone Hatchling Egg
Smooth Hatchling Egg
Sage Hatchling Egg
Hatch
96%
2%
2%
Stone Hatch
32%
65%
2%
Smooth Hatch
11%
22%
67%
Sage Hatch
33%
67%
Dietary Modifiers
None
Sedimentary Rock
Raw Metal
Dirt
Drecko
Default Egg Chance
Drecklet Egg
Glossy Drecklet Egg
Drecko
98%
2%
Glossy Drecko
35%
65%
Dietary Modifiers
None
Mealwood
Shine Bug
Default Egg Chance
Shine Nymph Egg
Sun Nymph Egg
Royal Nymph Egg
Coral Nymph Egg
Azure Nymph Egg
Abyss Nymph Egg
Radiant Nymph Egg
Shine Bug
98%
2%
Sun Bug
33%
65%
2%
Royal Bug
33%
65%
2%
Coral Bug
33%
65%
2%
Azure Bug
33%
65%
2%
Abyss Bug
33%
65%
2%
Radiant Bug
2%
98%
Dietary Modifiers
None
Gristle Berry
Fried Mushroom
Pepper Bread
Stuffed Berry
Phosphorus
Barbeque
Puft
Puftlet Egg
Puftlet Prince Egg
Dense Puftlet Egg
Squeaky Puftlet Egg
Puft
94%
2%
2%
2%
Puft Prince
98%
2%
Dense Puft
31%
2%
67%
Squeaky Puft
31%
2%
67%
Nearby Creature Modifiers
None
Not penned with Puft Prince
Penned with Puft Prince
Penned with Puft Prince
Pacu
Fry Egg
Tropical Fry Egg
Gulp Fry Egg
Pacu
96%
2%
2%
Tropical Pacu
32%
66%
2%
Gulp Fish
32%
2%
66%
Body Temperature Modifiers
None
Pacu Body Temperature between
35 °C ↔ 80 °C
/
95 °F ↔ 176 °F
Pacu Body Temperature between
-30 °C ↔ 5 °C
/
-22 °F ↔ 41 °F
Divergent
Default Egg Chance
Sweetle Egg
Grubgrub Egg
Sweetle
98%
2%
Grubgrub
33%
67%
Pollination Modifiers
None
Tends to
Grubfruit Plant
/
Spindly Grubfruit Plant
Egg Chance Mechanics
Initially, each critter has breeding weights
w
0
,
w
1
,
…
, which are then normalized so their sum equals 1.
Example:
Consider
Abyss Bugs
with initial weights:
Azure Bug Egg
: 0.33
Abyss Bug Egg
: 0.66
Radiant Bug Egg
: 0.02
After normalization, weights become:
Azure Bug Egg
: 0.3267
Abyss Bug Egg
: 0.6535
Radiant Bug Egg
: 0.0198
These normalized weights represent the probability of laying a particular egg morph at that instant.
Modifiers
adjust probabilities by adding weights, followed by re-normalization. How frequent a modifier adjusts probabilities depends on the type of modifier. The final weights after adjusting are clamped to remain within the range
[
0
,
1
]
.
Invertible Modifiers
Some modifiers are invertible, meaning critters
not
meeting the required conditions have weights negatively adjusted. For instance,
Pokeshell
not in
Ethanol
have their
Oakshell
egg chances reduced each second.
Predicting Egg Chances
To predict egg-laying probability over multiple periods:
P
n
=
1
−
(
1
−
w
0
)
(
1
1
+
d
)
n
Where:
n
is the number of periods (cycles, seconds, etc.)
d
is the weight change per period (from the modifier tables)
w
0
is the initial normalized weight
Example
A Hatch eating Sedimentary Rock for 30 cycles, assuming:
Initial weight
w
0
=
0
.
0
1
9
6
Weight change
d
=
0
.
0
5
per cycle
Calculation:
P
3
0
=
1
−
(
1
−
0
.
0
1
9
6
)
(
1
1
+
0
.
0
5
)
3
0
≈
0
.
7
7
3
1
5
8
Thus, after 30 cycles, the Hatch has an approximately 77.3% chance of laying a Stone Hatch Egg.
Dietary Modifiers
Dietary modifiers adjust breeding chances based on food intake, proportional to its calories consumed. Most well-fed, tame critters look for food roughly once per cycle; thus, for simplicity, the table below is calibrated to represent a per-cycle effect. Note glum and/or wild critters eat significantly less than satisfied, tame critters and should be accounted for when predicting egg chances:
Most glum, tame critters eat every 5 cycles
Most satisfied, wild critters eat every 4 cycles
Most glum, wild critters eat every 10 cycles
Egg
Critters
Diet
Weight
Invertible
Stone Hatch Egg
Hatch
Stone Hatch
Smooth Hatch
Sedimentary Rock
0.05
No
Sage Hatch Egg
Hatch
Sage Hatch
Dirt
0.05
No
Smooth Hatch Egg
Stone Hatch
Smooth Hatch
Copper Ore
Iron Ore
Gold Amalgam
Wolframite
0.05
No
Sun Bug Egg
Royal Bug
Sun Bug
Shine Bug
Gristle Berry
0.05
No
Royal Bug Egg
Royal Bug
Sun Bug
Coral Bug
Fried Mushroom
0.05
No
Coral Bug Egg
Royal Bug
Azure Bug
Coral Bug
Pepper Bread
0.05
No
Azure Bug Egg
Azure Bug
Abyss Bug
Coral Bug
Stuffed Berry
0.05
No
Abyss Bug Egg
Azure Bug
Abyss Bug
Refined Phosphorus
0.05
No
Radiant Bug Egg
Radiant Bug
Abyss Bug
Barbeque
0.05
No
Glossy Drecko Egg
Glossy Drecko
Drecko
Mealwood
0.025
No
Cuddle Pip Egg
Cuddle Pip
Pip
Thimble Reed
0.025
No
Regal Bammoth Egg
Bammoth
Regal Bammoth
Squash Fries
0.05
No
Husky Mooteor
Husky Moo
Gassy Moo
Plant Husk
0.05
No
Gassy Mooteor
Husky Moo
Gassy Moo
Gas Grass
0.05
No
Nearby Creature Modifiers
These modifiers adjust breeding chances based on other critters present in the same
Room
. Checked every second.
Egg
Critters
Nearby Creatures
Weight
Invertible
Puft Prince Egg
Puft Prince
Dense Puft
Puft
Squeaky Puft
Puft Prince
-0.00025
Yes
Dense Puft Egg
Dense Puft
Puft
Puft Prince
8.333e-05
No
Squeaky Puft Egg
Puft
Squeaky Puft
Puft Prince
8.333e-05
No
Body Temperature Modifiers
Modifiers based on a critter's body temperature. Checked every second.
Egg
Critters
Body Temperature Range
Weight
Invertible
Molten Slickster Egg
Slickster
Longhair Slickster
Molten Slickster
100 °C ↔ 250 °C
/
212 °F ↔ 482 °F
8.333e-05
No
Longhair Slickster Egg
Slickster
Longhair Slickster
Molten Slickster
20 °C ↔ 60 °C
/
68 °F ↔ 140 °F
8.333e-05
No
Tropical Pacu Egg
Tropical Pacu
Gulp Fish
Pacu
35 °C ↔ 80 °C
/
95 °F ↔ 176 °F
8.333e-05
No
Gulp Fish Egg
Tropical Pacu
Gulp Fish
Pacu
-30 °C ↔ 5 °C
/
-22 °F ↔ 41 °F
8.333e-05
No
Delecta Vole Egg
Delecta Vole
Shove Vole
60 °C ↔ 100 °C
/
140 °F ↔ 212 °F
8.333e-05
No
Pollination Modifiers
These modifiers occur when critters pollinate specific plants. Checked whenever pollination occurs on a
Grubfruit Plant
/
Spindly Grubfruit Plant
.
Egg
Critters
Pollination
Weight
Invertible
Grubgrub Egg
Sweetle
Grubgrub
Spindly Grubfruit Plant
Grubfruit Plant
0.025
No
Element Creature Modifiers
Modifiers occur when a critter is in a specific element.
Pokeshells
require
substantial liquid mass
to produce morphs, whereas
Plug Slugs
produces
Sponge Slug Egg
in any amount of liquid. Checked every second.
Egg
Critters
Element
Weight
Invertible
Oakshell Roe
Pokeshell
Oakshell
Sanishell
Ethanol
0.00025
Yes
Sanishell Roe
Pokeshell
Oakshell
Sanishell
Water
0.00025
Yes
Smog Slug Egg
Sponge Slug
Smog Slug
Plug Slug
Unbreathable
0.00025
Yes
Sponge Slug Egg
Sponge Slug
Smog Slug
Plug Slug
Liquid
0.00025
Yes
Blum Lumb Egg
Blum Lumb
Lumb
Carbon Dioxide
0.00025
Yes
Decor Modifiers
Modifiers occur when a critter is within a cell of sufficient
Decor
. Checked every 4 seconds.
Egg
Critters
Decor
Weight
Invertible
Shatter Flox Egg
Flox
Shatter Flox
≥100
0.00033332
Yes
