# Skills

This page is about the Skills system that applies to standard
Duplicants
. For the Skills system introduced in
The Bionic Booster Pack
, see
Bionic Duplicant § Skills
.
Part of a series of articles about
Duplicants
Morale
,
Interests
,
Skills
,
Stress
,
Stress Responses
,
Overjoyed Responses
,
Movement & Reach
Attributes
Construction
,
Excavation
,
Machinery
,
Athletics
,
Science
,
Cuisine
,
Creativity
,
Strength
,
Medicine
,
Agriculture
,
Husbandry
Errands
Combat
,
Build
,
Care
,
Cook
,
Decorate
,
Dig
,
Farm
,
Life Support
,
Operate
,
Ranching
,
Research
,
Storage
,
Supply
,
Tidy
,
Toggle
List of Duplicants
A Duplicant visits the
Printing Pod
to learn a Skill.
As
Duplicants
work in the colony, they will gain Skill Points to upgrade their
attributes
and gain new abilities. Skill Points are spent using the Skills menu.
Mechanics
Viewing the Skills menu
Duplicants earn
experience
over time, which in turn earns
Skill Points
. Each Skill Point earned can be spent in the Skills menu to learn a Skill. Each Skill bestows benefits on the Duplicant such as increased
attributes
or the ability to perform new kinds of tasks. Once a Skill is learned, the Duplicant gains access to the Skill's hat as a cosmetic option. The Skill will remain learned unless the Duplicant uses a
Skill Scrubber
, which refunds all spent Skill Points and relinquishes all hats.
In order to spend a Skill Point, a Duplicant must visit an active
Printing Pod
or
Mini-Pod
.
The position of a Skill in the Skills menu signifies its
morale
properties:
A Skill's column, shown by alternating bands of dark and light background color, denotes its
tier
. Learning a Skill will increase the Duplicant's total morale need by the Skill's numeric tier, meaning higher-tiered Skills come with greater morale costs.
A Skill's row denotes its respective
interest
, which is shown in parentheses under the Skill's name. If a Duplicant is interested in the same type of work as the Skill, the effective morale cost for that skill is reduced by 1. Skills that a Duplicant is interested in are marked with a heart icon.
Most Skills cannot be learned unless certain lower-tiered Skills have already been learned. Each Skill has its own predetermined set of prerequisites.
Experience Gain
All Duplicants passively earn experience at a base rate of 0.5 experience per second. While a duplicant is actively performing a task, they gain 0.6 experience per second by default.
If the duplicant has an interest in the corresponding skill group, the rate increases according to:
X
P
gain
=
0
.
6
×
(
1
+
0
.
5
f
frequency
)
where
f
frequency
represents how frequently that errand occurs:
Frequency type
f
frequency
Common sources
All day
0.9
researching, telescopes
Most of the day
0.75
ranching, constructing, digging, demolishing, cooking, power control stations, oil refinery, farm stations, analyzing geysers
Part of the day
0.5
cleaning, harvesting, fabricating, refining
Barely ever
0.25
medical stations, rocket control stations, egg incubator, attacking
These frequency categories are assigned internally to each errand type and are not shown in-game.
A rarer less frequent errand yields proportionally more experience for duplicants with an interest.
Experience for level up
The amount of total experience needed to earn some number of total Skill Points is given by the following formula:
TotalExperience
=
1
5
0
0
0
0
×
(
SkillPoints
1
5
)
1
.
4
4
The Skills menu will display a progress bar showing experience earned towards the next Skill Point. The number displayed in the progress bar is the amount of experience earned since the most recent Skill Point was earned. The maximum value in the progress bar at any given time is computed by taking the total experience needed for the next Skill Point and subtracting the total experience needed for the most recent Skill Point.
Example:
A Duplicant has earned 1 Skill Point and the progress bar shows 3367 / 5204.
Total experience needed for 1 Skill Point is
1
5
0
0
0
0
×
(
1
1
5
)
1
.
4
4
≈
3
0
3
7
.
5
2
Total experience needed for 2 Skill Points is
1
5
0
0
0
0
×
(
2
1
5
)
1
.
4
4
≈
8
2
4
1
.
4
3
The maximum value in the progress bar is
8
2
4
1
.
4
3
−
3
0
3
7
.
5
2
≈
5
2
0
3
.
9
1
Total experience earned so far is
3
0
3
7
.
5
2
+
3
3
6
7
≈
6
4
0
4
.
5
2
Skills
Digging
Hard Digging
Tier 1
Prerequisites
None
Enables
Very Firm Material mining
Boosts
+2
Excavation
Superhard Digging
Tier 2
Prerequisites
Hard Digging
Enables
Abyssalite
+ mining (
see Dig
)
Boosts
+2
Excavation
Super-Duperhard Digging
Tier 3
Prerequisites
Hard Digging
Superhard Digging
Enables
Diamond
and
Obsidian
mining (
see Dig
)
Boosts
+2
Excavation
Hazmat Digging
Tier 4
Prerequisites
Hard Digging
Superhard Digging
Super-Duperhard Digging
Enables
Corium
mining  (
see Dig
)
Building
Improved Construction I
Tier 1
Prerequisites
None
Boosts
+2
Construction
Improved Construction II
Tier 2
Prerequisites
Improved Construction I
Boosts
+2
Construction
Demolition
Tier 3
Prerequisites
Improved Construction I
Improved Construction II
Enables
Demolish
Gravitas
Buildings
Boosts
+2
Construction
Farming
Improved Farming I
Tier 1
Prerequisites
None
Boosts
+2 Agriculture
Crop Tending
Tier 2
Prerequisites
Improved Farming I
Enables
Crop Tending
Micronutrient Fertilizer
Crafting
Boosts
+2
Agriculture
Improved Farming II
Tier 3
Prerequisites
Improved Farming I
Crop Tending
Enables
Can salvage
Plant Husk
Botanical Analyzer
Usage
Boosts
+2
Agriculture
Ranching
Critter Ranching I
Tier 2
Prerequisites
Improved Farming I
Enables
Critter
Wrangling
Grooming Station
Usage
Boosts
+2
Husbandry
Critter Ranching II
Tier 3
Prerequisites
Improved Farming I
Critter Ranching I
Enables
Milking Station
Usage
Boosts
+2
Husbandry
Cooking
Grilling
Tier 1
Prerequisites
None
Enables
Electric Grill
Usage
Gas Range
Usage
Boosts
+2
Cuisine
Grilling II
Tier 2
Prerequisites
Grilling
Enables
Spice Grinder
Usage
Boosts
+2
Cuisine
Decorating
Art Fundamentals
Tier 1
Prerequisites
None
Enables
Crude artwork quality
Blank Canvas
Usage
Large Sculpting Block
Usage
Boosts
+2
Creativity
Aesthetic Design
Tier 2
Prerequisites
Art Fundamentals
Enables
Mediocre artwork quality
Clothing Refashionator
Usage
Artifact Analysis
Boosts
+2
Creativity
Masterworks
Tier 3
Prerequisites
Art Fundamentals
Aesthetic Design
Enables
Master artwork quality
Fossil Fragments
and
Ancient Specimen
excavation
Boosts
+2
Creativity
Researching
Advanced Research
Tier 1
Prerequisites
None
Enables
Super Computer
Usage
Emulsifier
Usage
Boosts
+2
Science
Field Research
Tier 2
Prerequisites
Advanced Research
Enables
Geographical Analysis
Geotuner
Usage
Boosts
+2
Science
Astronomy
Tier 3
Prerequisites
Advanced Research
Field Research
Enables
Virtual Planetarium
Usage
Mission Control Station
Usage
Boosts
+2
Science
Applied Sciences Research
Tier 3
Prerequisites
Advanced Research
Field Research
Enables
Materials Study Terminal
Usage
Boosts
+2
Science
Astronomy
Tier 2
Prerequisites
Advanced Research
Enables
Telescope
Usage
Mission Control Station
Usage
Boosts
+2
Science
Data Analysis Researcher
Tier 3
Prerequisites
Advanced Research
Astronomy
Enables
Telescope
Usage
Mission Control Station
Usage
Virtual Planetarium
Usage
Boosts
+2
Science
Suit Wearing
Suit Sustainability Training
Tier 2
Prerequisites
Improved Carrying I
Rocket Piloting
Enables
Slows
Exosuit
Durability Damage
Boosts
+2
Athletics
Exosuit Training
Tier 3
Prerequisites
Improved Carrying I
Suit Sustainability Training
Rocket Piloting
Enables
Exosuit
Penalty Reduction
Boosts
+2
Athletics
Rocket Piloting
Tier 4
Prerequisites
Advanced Research
Field Research
Astronomy
Improved Carrying I
Suit Sustainability Training
Exosuit Training
Enables
Command Capsule
Usage
Rocket Navigation
Tier 5
Prerequisites
Advanced Research
Field Research
Astronomy
Improved Carrying I
Suit Sustainability Training
Exosuit Training
Rocket Piloting
Boosts
+10%
Piloting
Rocketry
Rocket Piloting
Tier 1
Prerequisites
None
Enables
Rocket Control Station
Usage
Rocket Piloting II
Tier 3
Prerequisites
Advanced Research
Astronomy
Rocket Piloting
Boosts
+200%
Piloting
Supplying
Improved Carrying I
Tier 1
Prerequisites
None
Boosts
+2
Strength
+400 kg Carrying Capacity
Improved Carrying II
Tier 2
Prerequisites
Improved Carrying I
Boosts
+2
Strength
+800 kg Carrying Capacity
Operating
Improved Tinkering
Tier 1
Prerequisites
None
Boosts
+2
Machinery
Electrical Engineering
Tier 2
Prerequisites
Improved Tinkering
Enables
Generator Tuning
Microchip
Crafting
Soldering Station
Usage
Boosts
+2
Machinery
Mechatronics Engineering
Tier 3
Prerequisites
Improved Carrying I
Improved Carrying II
Improved Tinkering
Electrical Engineering
Enables
Conveyor Rail
Construction
Boosts
+2
Machinery
+2
Construction
Doctoring
Medicine Compounding
Tier 1
Prerequisites
None
Enables
Apothecary
Usage
Boosts
+2
Medicine
Bedside Manner
Tier 2
Prerequisites
Medicine Compounding
Enables
Sick Bay
Usage
Boosts
+2
Medicine
Advanced Medical Care
Tier 3
Prerequisites
Medicine Compounding
Bedside Manner
Enables
Disease Clinic
Usage
Boosts
+2
Medicine
Tidying
Improved Strength
Tier 1
Prerequisites
None
Boosts
+2
Strength
Plumbing
Tier 2
Prerequisites
Improved Strength
Enables
Pipe
Emptying
Boosts
+2
Strength
Pyrotechnics
Tier 3
Prerequisites
Improved Strength
Plumbing
Enables
Blastshot Maker
Usage
