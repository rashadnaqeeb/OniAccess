# Bonbon Tree

This article is related to
The Frosty Planet Pack
content.
Bonbon Tree
Bonbon Tree branches synthesize Light to produce Nectar.
Branches can be harvested for Wood.
Bonbon Trees can shed most environmental debris, and rarely become entombed.
Location
Nectar Biome
Seed
Bonbon Tree Seed
Base Seed Chance
0% (See
Seed
)
Produces
5
x
75 kg
Wood
Nectar
(
Amount Varies
)
Growth Speed
4.5/18
Spawn Temperature
-18.15 °C
/
-0.67 °F
Temperature
-75 °C ↔ -15 °C
/
-103 °F ↔ 5 °F
Lethal Temperature Threshold
Below
-100 °C
/
-148 °F
or above
20 °C
/
68 °F
Atmospheric Pressure
Ignored
Radiation Range
0 rads/cycle (Completely Safe) ↔ 12,200 rads/cycle (Maximum Hazard)
Survives In
Oxygen
Polluted Oxygen
Carbon Dioxide
Snow
Vacuum
Fertilizer
Snow
100 kg/cycle
Plant Height
2 Tiles
Decor
-10 (Radius: 2 Tiles)
Can be Planted in:
Code
Internal Id
SpaceTree
String Id
STRINGS.CREATURES.SPECIES.SPACETREE.NAME
Bonbon Tree
is a
Plant
that can live in
Vacuum
or cold environment and is found naturally in
Nectar Biome
. It grows up to 5 branches from its trunk, which produce
Nectar
when exposed to
Light
. The branches can be harvested for
Wood
; however, that will temporarily disrupt the nectar production. The nectar can be harvested from the trunk by a Duplicant skilled with (
Crop Tending
) or directly with pipes.
Seed
The
Bonbon Tree Seed
is the seed of a
Bonbon Tree
. As the tree itself does not have harvestable items, it can not directly produce new Bonbon Tree Seeds like traditional plants.
However, additional seeds can be obtained through a variety of methods:
Uprooting an
Bonbon Tree
- in addition to the ones found in the
Nectar Biome
, Bonbon Meteor from the Icy Nectar
Meteor Showers
also self-plants a
Bonbon Tree
at its impact.
Bonbon Tree Seeds can be obtained from
Care Packages
after Cycle 24.
Spigot Seals
have a 20% chance of dropping a
Bonbon Tree Seed
whenever they poop.
Growth
It takes 4.5 cycles (18 cycles if wild) for the trunk to mature, and an additional 4.5 cycles (18 cycles if wild) for each branch to mature. A Bonbon Tree branch additionally requires at least 300 Lux to grow. Unlike most plants,
Bonbon Trees
and its branches will never grow old and self-harvest.
While the farmer's touch bonus from a
Farm Station
improves the rate of trunk and branch growth, it does not affect nectar production.
Usage
Bonbon Trees
produces
Nectar
directly into its trunk, depending on the health of its branches. Each non-wilting Bonbon Tree Branch produces
16 kg
/cycle of
Nectar
when Lux exceeds 10000. If Lux is less than 10000, then nectar production is reduced proportional to the Lux level, with a minimum of 300 Lux. (1000 Lux would only produce
1.6 kg
/cycle of
Nectar
)
Spigot Seals
can drink the
Nectar
directly from a
Bonbon Tree
.
Pips
can eat
Bonbon Tree
branches. Unlike
Arbor Tree
branches, the branches need to be fully mature before a
Pip
will eat it.
Harvesting the branches from the
Bonbon Tree
yields
75 kg
of
Wood
. Comparing to
Arbor Trees
, this is 4 times less productive than an
Arbor Tree
.
Branch Placement
Unlike
Arbor Trees
,
Bonbon Trees
have fixed branch placements and grows in the Top-Left, Top-Middle, Top-Right, Middle-Left, Middle-Right cell of the
Bonbon Tree
.
Average nectar production from sunlight
Sunlight is a free source of illuminance, but it is only present for 525 out of 600 seconds per cycle and varies over time. As a result, a Bonbon Tree relying on sunlight alone will not operate at full productivity.
Nectar is produced only when the local illuminance is at least
300 Lux (Dim)
.
Let
P
(
τ
)
be the fraction of the tree’s per-cycle output produced at time
τ
in the cycle (so
P
(
τ
)
=
1
corresponds to full rate):
P
(
τ
)
=
1
1
0
0
0
0
×
{
0
,
E
max
sin
⁡
(
π
τ
5
2
5
)
<
3
0
0
,
E
max
sin
⁡
(
π
τ
5
2
5
)
,
3
0
0
≤
E
max
sin
⁡
(
π
τ
5
2
5
)
≤
1
0
0
0
0
,
1
0
0
0
0
,
E
max
sin
⁡
(
π
τ
5
2
5
)
>
1
0
0
0
0
.
where
E
max
is the peak per-tile illuminance (lux) for the asteroid. Values below
300 Lux (Dim)
produce no nectar, and values above
10,000 Lux (Bright)
are capped.
The simulation typically updates every
0
.
2
s
, so the cycle-average efficiency is approximated using a discrete mean:
η
=
1
6
0
0
×
5
∑
n
=
0
6
0
0
×
5
−
1
P
(
0
.
2
n
)
Bonbon Tree average efficiency and nectar production vs. peak sunlight
Peak sunlight
Average efficiency
η
Wild nectar / cycle
Domestic nectar / cycle
10,000
0.56
11.14 kg
44.54 kg
20,000
0.73
14.65 kg
58.6 kg
30,000
0.78
15.62 kg
62.49 kg
35,000
0.79
15.9 kg
63.58 kg
40,000
0.8
16.1 kg
64.39 kg
50,000
0.82
16.38 kg
65.52 kg
60,000
0.83
16.57 kg
66.27 kg
80,000
0.84
16.8 kg
67.21 kg
100,000
0.85
16.94 kg
67.77 kg
120,000
0.85
17.03 kg
68.14 kg
Database
Bonbon Tree
Edible Plant
The Bonbon Tree is a towering plant developed to thrive in below-freezing temperatures. It features multiple independently functioning branches that synthesize bright light to funnel nutrients into a hollow central core.
Once the tree is fully grown, the core secretes digestive enzymes that break down surplus nutrients and store them as thick, sweet fluid. This can be refined into
Sucrose
for the production of higher-tier foods, or used as-is to sustain Spigot Seal ranches.
Bonbon Trees are generally considered an eyesore, and would likely be eradicated if not for their delicious output.
History
U52-616718
: Introduced. Public testing of Frosty Planet Pack DLC
U52-618499
: Renamed from Bonbon Gourd to Bonbon Tree
U53-642443
: Pips can now eat Bonbon Tree branches
