# Starmap

This article may be
outdated
. It was last updated for
U45-546664
. These versions were tagged as updating the game mechanics discussed here:
U56-674504
This article is related to Base Game content.
There is a version of this article for the
DLC, see
Starmap (Spaced Out)
.
The
Starmap
allows travelling to other nearby asteroids, planets and other destinations with a
Rocket
.
To unlock and upkeep the Starmap a
Telescope
is required while the Telescope itself is also needed to research destinations before one can travel to them.
Starmap in DLC
For the
Spaced Out DLC
the
Starmap
and the Rocketry program has been entirely reworked.
See:
Starmap in Spaced Out
Rockets in Spaced Out
Telescope in Spaced Out
Interface
Starmap
The center part shows Destinations which are found every 10,000 kilometers away from the starting asteroid with possibly several being the same distance away. There are 10 possible positions in a tier for a Destination to be located, and at most 4 Destinations can be found on the same tier.
To research Destinations which are further away, one Destination a tier below it must be analyzed. Time to analyze depends on distance.
What Destination can be found depends on the distance. Some destinations are guaranteed while others are randomly generated. The list of destinations and their distance tier is determined by the worldgen seed (aka asteroid coordinates), however their position within that tier is randomized each time the world is created (this variance is purely cosmetic).
Sometimes there are no destinations in a 10,000 km distance "band", starting at 60,000 km. This does not prevent further Destination spawns, it is merely an empty space.
Each 10,000 km takes an additional 3
cycles
to travel to and back from (2.7 cycles if the pilot has the Rocket Navigation
skill
). Rocket missions can be effectively shortened further via the
Mission Control Station
, which gives rockets a temporary (but repeatable) 20% speed boost for 1 cycle at a time.
Destination Status
The right part shows details and possible actions for the selected Destination.
Analysis must be done with a
Telescope
first to unlock a Destination for Missions.
Research is done from the top to the bottom of the list. Each done by one
Research Module
and yields 50
Data Bank
the first time.
Research targets mark those which reveal ??? materials which will either be
Niobium
,
Fullerene
,
Isosap
,
Abyssalite
or nothing. All but Volcanic Planets have at least one ??? and there is basically a 50% chance to have another. Those materials appear in much less quantity.
World Composition of each Destination is shown in % which will be used to calculate how much of each material will be brought back. You will bring back (% of material / total % of type) * Cargo Bays * 1000 kilograms. For example, if an Organic Mass has only Algae and Slime as Solid and both are listed at flat 25%: 500 kg of each material will be returned per Solid Cargo Bay. If 5%
Isosap
was found on the same Destination, 90.909 kg will be returned ((5/55)*1*1000), along with 454.545 kg ((25/55)*1*1000) of Algae and Slime.
In the case a material is the sole suitable material on a Destination: 1000 kg per Cargo Bay will be returned. For example, if Water was at 19% and the only liquid present, one
Liquid Cargo Tank
would bring back 1000 kg (1000*(19/19). This makes
Gas Giants
very attractive for Rare Materials because they are the only Solid material there.
Critters
and
seeds
do not use the weight system, and instead provide a fixed amount for a type per
Biological Cargo Bay
.
Rocket Hangar
The left part lists complete Rockets and all their details. Reach is also displayed on the center part when a rocket is selected.
List of Rockets
Shows completed Rockets which can be renamed and clicked at to select a rocket as well as their Status.
Launch Checklist
Destination Selected.
Astronaut was assigned and boarded the
Command Capsule
over a
Gantry
.
Atmo Suit
was delivered to the Command Capsule.
Cargo Bays
must be completely empty before launch
The Rocket should not only have an Engine and a
Command Capsule
(to be even listed) but also a Fuel Tank.
Max Range
Total oxidizable fuel,
Petroleum
,
Liquid Hydrogen
and
Iron
need
Liquid Oxygen
or
Oxylite
to burn.
Main Engine efficiency, different fuels pack more punch than others.
Average Oxidizer efficiency, Liquid Oxygen has 133% and Oxylite 100% efficiency, the better the efficiency, the less fuel is needed to achieve the same distance.
Total thrust, total range based on the above without taking Weight Penalty into consideration
Weight Penalty, the heavier the rocket, the bigger the penalty will get, exponentially. The formula is
Penalty = Max[Shipweight, (Shipweight/300)^3.2]
[
1
]
Total Range, Total Thrust minus the Weight Penalty
Mass
Dry Mass = weight of the rocket
Wet Mass = weight of the fuel
Total = Dry Mass + Wet Mass
Storage
List of
Cargo Bays
installed and their Capacity.
Fuel
List of Fuel Tanks and the mass with which they are filled with.
Oxidizer
List of Oxidizer Tanks and Booster and the mass with which they are filled with.
Passengers
List of Duplicants in the Rocket.
Destinations
Each range bracket will have either guaranteed or random destinations. Guaranteed destinations always appear, and random destinations are added from a pool. There will be 15 to 24 random destinations, and no more than 3 random destinations are added to a given bracket. This gives an upper limit of 4 destinations per distance bracket, since only the 10,000km bracket has more than one guaranteed destination - and it cannot have any random destinations. The one exception is the 170,000km bracket, which (possibly due to an off-by-one error
[
2
]
) can only have up to 2 destinations.
The composition of each destination is determined randomly: Each destination type has a list of resources, each of which has a weight range. Each individual destination will roll a random weight for each resource type, and the chosen weights determine the final composition percentages. All resources
except
for bio-resources and rare resources have a weight range of 100 to 200. This means that a destination type will have an even split of resources on average, and no resource will offer more than double of any other resource.
Rare resources are added to destinations regardless of type, and will occur on almost every destination. There is a 2.21% chance to have no rare resources, 24.26% to have one, and 73.53% to have two. Each rare type is equally likely.
Abyssalite
has a weight of 50 to 100. (50% as much as regular resources)
Isosap
has a weight of 30 to 60. (30%)
Niobium
has a weight of 10 to 20. (10%)
Fullerene
has a weight of 0.5 to 1. (0.5%)
There is also a 33% chance that a destination will have a
Neural Vacillator
charge as one of the research opportunities. With the exception of a destination having three research opportunities, it is impossible to tell if a given destination has a neural vacillator charge in advance.
Unlike destinations, the number and type of rare resources varies for a given worldgen seed, and is randomly determined during worldgen.
Mass
Each destination has a current mass, which is within the range of its minimum and maximum mass according to its size. When obtaining common and rare resources (but not any other kinds, such as bio-resources), the total amount of mass retrieved is subtracted from the amount of remaining mass of the destination. Resource extraction of common and rare resources is therefore limited by the amount of remaining mass: once the mass hits the minimum amount, no further resources may be extracted.
Mass is continually
replenished
by a small amount every second. This amount is listed in the Starmap interface in a rate of kg/cycle. The rate of replenishment is dependent only on the destination type, and not on the destination's size.
The possible sizes of destination are as follows:
Size
Minimum Mass
Maximum Mass
Available Mass Limit
Tiny
63,994 t
64,000 t
6 t
Small
127,998 t
128,000 t
12 t
Medium
255,982 t
256,000 t
18 t
Large
383,980 t
384,000 t
20 t
Artifacts
Artifacts
are rare resources which can only be obtained from space missions. Each destination type has a fixed set of probabilities per rarity tier of artifact, as listed below:
Destination
Artifact Rarity
Artifact Probabilities
Nothing
Rarity 0
Rarity 1
Rarity 2
Rarity 3
Rarity 4
Rarity 5
None
100%
Bad
50%
25%
15%
10%
Mediocre
50%
25%
15%
10%
Good
50%
25%
15%
10%
Great
50%
25%
15%
10%
Amazing
50%
15%
25%
10%
Perfect
50%
30%
20%
Destination Types
Possible Destinations
Icon
Name
Notes
Size
Replenishment
Artifact
Rarity
Common
Resources
Bio-Resources
Size:||Small (from:Small$12000) (available mass:12000)
Replenish:|| 166.7 kg/cycle
Full charge: 72 cycles (from:6)
Artifacts:||2
(Mediocre) (from:2
(Mediocre))
Carbon Asteroid
Two are set to spawn 10,000 km away and closest to the starting asteroid. More may appear between 40,000 km and 60,000 km.
Small
166.7 kg/cycle
Full charge: 72 cycles
2
(Mediocre)
≈33.3%
Refined Carbon
≈33.3%
Coal
≈33.3%
Diamond
Size:||Small (from:Small$12000) (available mass:12000)
Replenish:|| 83.3 kg/cycle
Full charge: 144 cycles (from:12)
Artifacts:||2
(Mediocre) (from:2
(Mediocre))
Metallic Asteroid
One is set to spawn 20,000 km away. More may appear between 50,000 km and 140,000 km.
Small
83.3 kg/cycle
Full charge: 144 cycles
2
(Mediocre)
≈33.3%
Iron
≈33.3%
Copper
≈33.3%
Obsidian
3
Smooth Hatch
Size:||Small (from:Small$12000) (available mass:12000)
Replenish:|| 83.3 kg/cycle
Full charge: 144 cycles (from:12)
Artifacts:||2
(Mediocre) (from:2
(Mediocre))
Oily Asteroid
May spawn 20,000 km and 160,000 km away.
Small
83.3 kg/cycle
Full charge: 144 cycles
2
(Mediocre)
≈25%
Solid Methane
≈25%
Solid Carbon Dioxide
≈25%
Crude Oil
≈25%
Petroleum
Size:||Tiny (from:Tiny$6000) (available mass:6000)
Replenish:|| 55.6 kg/cycle
Full charge: 108 cycles (from:18)
Artifacts:||1
(Bad) (from:1
(Bad))
Satellite
May spawn 30,000 km and 40,000 km away.
Tiny
55.6 kg/cycle
Full charge: 108 cycles
1
(Bad)
≈33%
Steel
≈33%
Copper
≈33%
Glass
Size:||Small (from:Small$12000) (available mass:12000)
Replenish:|| 55.6 kg/cycle
Full charge: 216 cycles (from:18)
Artifacts:||3
(Good) (from:3
(Good))
Rocky Asteroid
One is set to spawn 30,000 km away and more may be found between 40,000 km and 150,000 km.
Small
55.6 kg/cycle
Full charge: 216 cycles
3
(Good)
≈33%
Copper Ore
≈33%
Sedimentary Rock
≈33%
Igneous Rock
3
Stone Hatch
Size:||Medium (from:Medium$18000) (available mass:18000)
Replenish:|| 41.7 kg/cycle
Full charge: 432 cycles (from:24)
Artifacts:||4
(Great) (from:4
(Great))
Interstellar Ice
One is set to spawn 40,000 km away and more may be found between 60,000 km and 140,000 km.
Medium
41.7 kg/cycle
Full charge: 432 cycles
4
(Great)
≈33%
Ice
≈33%
Solid Carbon Dioxide
≈33%
Solid Oxygen
3
Wort Seed
4
Sleet Wheat Grain
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 41.7 kg/cycle
Full charge: 480 cycles (from:24)
Artifacts:||2
(Mediocre) (from:2
(Mediocre))
Living Planet
May spawn between 40,000 km and 160,000 km.
Large
41.7 kg/cycle
Full charge: 480 cycles
2
(Mediocre)
≈50%
Aluminum Ore
≈50%
Solid Oxygen
1
Pip
4
Arbor Acorn
Size:||  (from:undefined) (available mass:20000)
Replenish:|| (from:0)
Artifacts:||0
(None) (from:0
(None))
Shattered Planet
One is set to spawn 50,000 km away. Only one can exist per game. You cannot land on this planet.
0
(None)
Size:||Medium (from:Medium$18000) (available mass:18000)
Replenish:|| 33.3 kg/cycle
Full charge: 540 cycles (from:30)
Artifacts:||4
(Great) (from:4
(Great))
Organic Mass
One is set to spawn 50,000 km away and more may be found between 70,000 km and 130,000 km.
Medium
33.3 kg/cycle
Full charge: 540 cycles
4
(Great)
≈33%
Slime
≈33%
Algae
≈33%
Polluted Oxygen
1
Gassy Moo
4
Gas Grass Seed
Size:||Medium (from:Medium$18000) (available mass:18000)
Replenish:|| 33.3 kg/cycle
Full charge: 540 cycles (from:30)
Artifacts:||1
(Bad) (from:1
(Bad))
Salty Dwarf
May spawn 50,000 km and 170,000 km away.
Medium
33.3 kg/cycle
Full charge: 540 cycles
1
(Bad)
≈33%
Salt Water
≈33%
Solid Carbon Dioxide
≈33%
Brine
3
Dasha Saltvine Seed
Size:||Medium (from:Medium$18000) (available mass:18000)
Replenish:|| 11.1 kg/cycle
Full charge: 1620 cycles (from:90)
Artifacts:||1
(Bad) (from:1
(Bad))
Chlorine Planet
May spawn 70,000 km and 130,000 km away.
Medium
11.1 kg/cycle
Full charge: 1620 cycles
1
(Bad)
≈50%
Solid Chlorine
≈50%
Bleach Stone
Size:||Medium (from:Medium$18000) (available mass:18000)
Replenish:|| 23.8 kg/cycle
Full charge: 756 cycles (from:42)
Artifacts:||5
(Amazing) (from:5
(Amazing))
Dusty Dwarf
May spawn between 70,000 km and 140,000 km.
Medium
23.8 kg/cycle
Full charge: 756 cycles
5
(Amazing)
≈33%
Regolith
≈33%
Mafic Rock
≈33%
Sedimentary Rock
Size:||Medium (from:Medium$18000) (available mass:18000)
Replenish:|| 23.8 kg/cycle
Full charge: 756 cycles (from:42)
Artifacts:||5
(Amazing) (from:5
(Amazing))
Red Dwarf
May spawn 70,000 km and 150,000 km away.
Medium
23.8 kg/cycle
Full charge: 756 cycles
5
(Amazing)
≈33%
Aluminum
≈33%
Methane
≈33%
Fossil
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 18.5 kg/cycle
Full charge: 1080 cycles (from:54)
Artifacts:||5
(Amazing) (from:5
(Amazing))
Terrestrial Planet
May spawn between 80,000 km and 170,000 km.
Large
18.5 kg/cycle
Full charge: 1080 cycles
5
(Amazing)
≈25%
Water
≈25%
Algae
≈25%
Dirt
≈25%
Oxygen
4
Fry Egg
4
Blossom Seed
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 18.5 kg/cycle
Full charge: 1080 cycles (from:54)
Artifacts:||5
(Amazing) (from:5
(Amazing))
Volcanic Planet
May spawn between 80,000 km and 170,000 km.
Large
18.5 kg/cycle
Full charge: 1080 cycles
5
(Amazing)
≈33%
Magma
≈33%
Igneous Rock
≈33%
Abyssalite
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 16.7 kg/cycle
Full charge: 1200 cycles (from:60)
Artifacts:||6
(Perfect) (from:6
(Perfect))
Oxidized Planetoid
May spawn 90,000 km and 110,000 km away.
Large
16.7 kg/cycle
Full charge: 1200 cycles
6
(Perfect)
≈50%
Rust
≈50%
Solid Carbon Dioxide
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 16.7 kg/cycle
Full charge: 1200 cycles (from:60)
Artifacts:||6
(Perfect) (from:6
(Perfect))
Ice Giant
May spawn between 90,000 km and 140,000 km.
Large
16.7 kg/cycle
Full charge: 1200 cycles
6
(Perfect)
≈25%
Ice
≈25%
Solid Carbon Dioxide
≈25%
Solid Oxygen
≈25%
Solid Methane
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 16.7 kg/cycle
Full charge: 1200 cycles (from:60)
Artifacts:||6
(Perfect) (from:6
(Perfect))
Gas Giant
May spawn between 90,000 km and 150,000 km.
A very good source of Rare Resources - 100% of the cargo bay will be filled with those since the planet has no solids.
Large
16.7 kg/cycle
Full charge: 1200 cycles
6
(Perfect)
≈50%
Natural Gas
≈50%
Hydrogen
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 12.8 kg/cycle
Full charge: 1560 cycles (from:78)
Artifacts:||2
(Mediocre) (from:2
(Mediocre))
Helium Giant
May spawn 100,000 km and 160,000 km away.
Best source of Niobium - it's the planet's only solid material.
Large
12.8 kg/cycle
Full charge: 1560 cycles
2
(Mediocre)
≈33%
Liquid Hydrogen
≈33%
Water
≈33%
Niobium
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 11.9 kg/cycle
Full charge: 1680 cycles (from:84)
Artifacts:||3
(Good) (from:3
(Good))
Glimmering Planet
May spawn 120,000 km and 150,000 km away.
Large
11.9 kg/cycle
Full charge: 1680 cycles
3
(Good)
≈50%
Tungsten
≈50%
Wolframite
Size:||Small (from:Small$12000) (available mass:12000)
Replenish:|| 11.1 kg/cycle
Full charge: 1080 cycles (from:90)
Artifacts:||1
(Bad) (from:1
(Bad))
Gilded Asteroid
May spawn 130,000 km and 170,000 km away.
Best source for obtaining large amounts of Fullerene. While on most planet it's listed as "trace amounts", Gilded Asteroid will yield about 33% of cargo mass of Fullerenes, enough for tonnes of Supercoolant.
Small
11.1 kg/cycle
Full charge: 1080 cycles
1
(Bad)
≈33%
Gold
≈33%
Fullerene
≈33%
Pyrite
Size:||  (from:undefined) (available mass:12000)
Replenish:|| (from:0)
Artifacts:||6
(Perfect) (from:6
(Perfect))
Temporal Tear
One is set to spawn 180,000 km away. Only one can exist per game. First dupe to reach the destination does not return.
6
(Perfect)
Size:||Large (from:Large$20000) (available mass:20000)
Replenish:|| 16.7 kg/cycle
Full charge: 1200 cycles (from:60)
Artifacts:||1
(Bad) (from:1
(Bad))
Possibilities by Distance
Key:
= always present,
= maybe present
Destination Type
Distances (x 1,000 km)
10
20
30
40
50
60
70
80
90
100
110
120
130
140
150
160
170
180
Carbon Asteroid
Metallic Asteroid
Oily Asteroid
Satellite
Rocky Asteroid
Interstellar Ice
Living Planet
Shattered Planet
Organic Mass
Salty Dwarf
Chlorine Planet
Dusty Dwarf
Red Dwarf
Terrestrial Planet
Volcanic Planet
Oxidized Planetoid
Ice Giant
Gas Giant
Helium Giant
Glimmering Planet
Gilded Asteroid
Temporal Tear
References
↑
https://forums.kleientertainment.com/forums/topic/96211-new-rocketry-mechanics/
↑
https://en.wikipedia.org/wiki/Off-by-one_error
