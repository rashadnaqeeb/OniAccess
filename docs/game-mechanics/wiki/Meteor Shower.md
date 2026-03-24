# Meteor Shower

Some meteors falling.
Meteor showers
are recurring events that bombard the
Space Biome
with falling rocks and debris. They provide a renewable source of materials like
Regolith
but can also damage buildings and tiles if left unmanaged.
Space Scanners
can detect incoming meteor showers, giving advance warning to close
Bunker Doors
and retract equipment. In
Spaced Out!
, showers are visible on the starmap as they travel toward their destination.
Activation
Base Game
Meteor showers are active immediately from the start of the game. The first meteor event triggers at cycle 1, with subsequent events following at regular intervals (every 14 cycles for the base game). Meteors fall in the
Space Biome
at the top of the map regardless of whether the surface has been breached.
Spaced Out!
Meteor seasons on an asteroid only become active once that asteroid has been
visited
by either:
A
Duplicant
landing from a rocket
A
Rover
(from the
Rover's Module
)
Until an asteroid is visited, no meteor showers will occur there. This gives players time to establish infrastructure before having to deal with meteor protection.
[
1
]
[
2
]
Fullerene Meteor Shower
The Fullerene Meteor Shower becomes active on the
Tundra Asteroid
after firing the
Temporal Tear Opener
. The showers occur daily, but will stop after reloading the game.
Demolior
Demolior
is a special asteroid event exclusive to
The Prehistoric Planet Pack
asteroids. This special shower is active immediately when the game starts, and spawns with
1,000
hitpoints. This shower is not reoccurring.
Material from Meteors
When a meteor impacts, its mass is distributed in two ways:
Debris
Small items scattered around the impact site. Metal meteors like
Iron Meteor
,
Copper Meteor
, and
Gold Meteor
produce only debris. This is dependent on having a positive Debris Count Range (See below)
Natural Tiles
Some meteors create natural tiles on impact instead of (or in addition to) debris. The split is approximately:
95-98%
becomes natural tiles
2-5%
becomes debris
The amount of tiles added is variable and is defined by its tile count property.
Meteor
Debris Count Range
Tile Count
Distribution
Algae Meteor
2–4
Yes
95-98% tile, 2-5% debris
Bleach Stone Meteor
2–4
Yes
95-98% tile, 2-5% debris
Slime Meteor
1–2
Yes
95-98% tile, 2-5% debris
Uranium Meteor
1–2
Yes
95-98% tile, 2-5% debris
[
3
]
Phosphoric Meteor
1–2
Yes
95-98% tile, 2-5% debris
Dust Fluff Meteor
1–2
Yes
95-98% tile, 2-5% debris
Fullerene Meteor
2–4
No
0% tile, 100% debris
Iron Meteor
2–4
No
0% tile, 100% debris
Copper Meteor
2–4
No
0% tile, 100% debris
Gold Meteor
2–4
No
0% tile, 100% debris
Iridium Meteor
2–4
No
0% tile, 100% debris
Rock Meteor
0
Yes
96.5% tile, 0% debris
Ice Meteor
or
0
Yes
96.5% tile, 0% debris
Snow Meteor
0
Yes
96.5% tile, 0% debris
Oxylite Meteor
0
Yes
95-98% tile, 0% debris
Radioactive Meteor
0
Yes
95-98% tile, 0% debris
Dust Meteor
0
No
0% tile, 0% debris
Difficulty Settings
The difficulty setting affects meteor showers as follows:
Setting
Season Period
Meteor Rate
Meteor Mass
Calm Period
Clear Skies
-
-
-
-
Spring Showers
2× longer
1.5× slower
1×
1×
Default
1×
1×
1×
1×
Cosmic Storm
1×
1.25× faster
0.8×
1×
Doomsday
1×
2× faster
0.5×
0.5× shorter
Clear Skies
disables meteor showers entirely.
Spring Showers
produces about
33%
of Default material. The doubled period and slower meteor rate combine to significantly reduce output.
Cosmic Storm
produces about
100%
of Default material. more meteors, but slightly smaller mass.
Doomsday
produces about
160%
of Default material for showers with bombardment cycles. The halved calm period increases bombardment time, which combines with the doubled meteor rate to outweigh the halved mass.
[
4
]
Continuous showers (most Spaced Out!) have no calm period, so Doomsday only provides the rate/mass tradeoff, resulting in ~100% net material.
Mechanics
Meteor showers work on a three-tier hierarchy:
Seasons
determine how often meteor showers occur on each planetoid, and which showers are possible
Showers
control how long meteors fall and which meteor types appear
Meteors
are the individual falling objects with their own mass, materials, and behavior
Meteor Seasons
Each asteroid has one meteor season assigned to it. A season triggers a meteor shower at regular intervals (the
period
).
When a season has multiple possible showers, one is selected at random with equal probability.
[
5
]
Parameter
Description
Period
Time between meteor showers, measured in cycles. Events occur at exactly 1, 1+period, 1+2×period, etc.
Start Active
The season is automatically marked as active and will begin the normal cadence of showers.
Travel Time
Showers appear on the starmap and travel to your planetoid before arriving. Provides an advanced visual warning.
Affected by Difficulty
Whether the season's period is modified by the meteor difficulty setting (e.g., doubled for "Spring Showers").
Meteor Showers
Each shower has a fixed
duration
during which it is active. Meteors alternate between bombardment phases and calm periods:
Phase
What Happens
Bombardment
Meteors fall at regular intervals (defined by "meteor interval"). A random duration within the shower's range.
Calm Period
No meteors fall. A random duration within the shower's range.
The shower cycles between these phases until its total duration expires. Total material output depends on how much time is spent in bombardment vs calm.
Continuous Showers:
Most
Spaced Out! showers bombard for their entire duration with no breaks. Only the base game showers (Iron, Gold, Copper) and the Regolith Asteroid's Regolith shower cycle between bombardment and calm.
Parameter
Description
Duration
Total length of the shower in seconds
Meteor Interval
Seconds between each meteor spawn during bombardment
Bombardment Period
Min-max range for how long each bombardment phase lasts
Calm Period
Min-max range for how long each break lasts
Meteor Types
Which meteor types can appear, and their relative weights
Meteors
Each meteor has its own properties:
Parameter
Description
Mass Range
Min-max mass in kg. Actual mass is randomized on spawn.
Temperature
Temperature of spawned material
Tile Range
Number of natural tiles created on impact. If 0, no tiles are created.
Debris Count Range
Number of debris created on impact. If 0, no debris is created.
Min Height / Max Height
Controls depth-reduction for tile placement. When tiles of the same element exist below the impact point, fewer tiles are placed as depth increases toward Max Height.
Entity Damage
Damage dealt to buildings and creatures on impact
Tile Damage
Damage dealt to tiles in the impact area
When a meteor's
Add Tiles
property is greater than 0, a random 95-98% of its mass becomes natural tiles and the remaining 2-5% becomes debris.
All meteors fall at angles up to 10° from vertical, allowing them to clip corners of tiles. Moreover, falling meteors emit
50 kg/s
of a given exhaust element.
World Size Scaling
Smaller planetoids receive proportionally fewer meteors. The meteor spawn rate scales with world width:
A 256-tile wide world receives the base meteor rate
A 128-tile wide world receives
half
as many meteors
A 160-tile wide world (like the
Regolith Asteroid
) receives about
62%
as many meteors
This means smaller planetoids accumulate less material per shower, but are also easier to protect with bunker tiles.
Meteor Season Summary
Base Game
Spring Showers
Default
Cosmic Storm
Doomsday
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
MeteorShowers
380:
256:
380:
29.37 tons
,
18.91 kg
,
12.94 kg
,
20.33 kg
256:
19.82 tons
,
12.74 kg
,
8.76 kg
,
13.71 kg
28 cycles
Iron Meteor Shower
Gold Meteor Shower
Copper Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
MeteorShowers
380:
256:
380:
88.13 tons
,
56.73 kg
,
38.88 kg
,
60.99 kg
256:
59.37 tons
,
38.26 kg
,
26.15 kg
,
41.11 kg
14 cycles
Iron Meteor Shower
Gold Meteor Shower
Copper Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
MeteorShowers
380:
256:
380:
88.12 tons
,
56.72 kg
,
38.88 kg
,
60.98 kg
256:
59.38 tons
,
38.26 kg
,
26.18 kg
,
41.1 kg
14 cycles
Iron Meteor Shower
Gold Meteor Shower
Copper Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
MeteorShowers
380:
256:
380:
142.85 tons
,
106.06 kg
,
62.09 kg
,
97.56 kg
256:
96.24 tons
,
71.45 kg
,
41.82 kg
,
65.73 kg
14 cycles
Iron Meteor Shower
Gold Meteor Shower
Copper Meteor Shower
Spaced Out!
Spring Showers
Default
Cosmic Storm
Doomsday
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
RegolithMoonMeteorShowers
160:
96:
160:
5.18 tons
,
2.9 kg
,
7.77 kg
,
1.53 kg
96:
3109.16 kg
,
1.74 kg
,
4.66 kg
,
0.93 kg
40 cycles
Regolith Meteor Shower
Iron Meteor Shower
Ice Meteor Shower
MiniRadioactiveOceanMeteorShowers
128:
128:
1.31 kg
,
81.82 kg
40 cycles
Uranium Meteor Shower
MiniBadlandsMeteorShowers
128:
128:
6.95 kg
,
18.64 kg
40 cycles
Ice Meteor Shower
SpacedOutStyleWarpMeteorShowers
128:
128:
56.67 kg
,
0.81 kg
,
2.32 kg
,
337.75 kg
,
0.81 kg
,
1.34 kg
,
6.21 kg
40 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
GassyMooteorShowers
96:
96:
0.10
20 cycles
Gassy Mooteor Shower
SpacedOutStyleRocketMeteorShowers
128:
128:
3.9 kg
,
376.35 kg
40 cycles
Oxylite Meteor Shower
ClassicStyleStartMeteorShowers
380:
240:
380:
165 kg
,
2.37 kg
,
6.85 kg
,
965 kg
,
2.37 kg
,
3.83 kg
,
18.38 kg
240:
105 kg
,
1.51 kg
,
4.31 kg
,
611.17 kg
,
1.51 kg
,
2.43 kg
,
11.57 kg
40 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
ClassicStyleWarpMeteorShowers
160:
160:
333.43 kg
,
2.63 kg
,
2.3 kg
40 cycles
Gold Meteor Shower
Iron Meteor Shower
MiniMetallicSwampyMeteorShowers
128:
128:
1.22 kg
,
2.14 kg
,
269.94 kg
,
1.22 kg
,
85 kg
40 cycles
Slimy Meteor Shower
Gold Meteor Shower
MiniForestFrozenMeteorShowers
128:
128:
3.9 kg
,
376.35 kg
40 cycles
Oxylite Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
RegolithMoonMeteorShowers
160:
96:
160:
15.53 tons
,
8.62 kg
,
23.13 kg
,
4.6 kg
96:
9.32 tons
,
5.21 kg
,
13.98 kg
,
2.85 kg
20 cycles
Regolith Meteor Shower
Iron Meteor Shower
Ice Meteor Shower
MiniRadioactiveOceanMeteorShowers
128:
128:
3.71 kg
,
231.82 kg
20 cycles
Uranium Meteor Shower
MiniBadlandsMeteorShowers
128:
128:
20.84 kg
,
55.93 kg
20 cycles
Ice Meteor Shower
SpacedOutStyleWarpMeteorShowers
128:
128:
170 kg
,
2.44 kg
,
6.95 kg
,
997.17 kg
,
2.44 kg
,
3.96 kg
,
18.64 kg
20 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
GassyMooteorShowers
96:
96:
0.10
20 cycles
Gassy Mooteor Shower
SpacedOutStyleRocketMeteorShowers
128:
128:
11.4 kg
,
1100.1 kg
20 cycles
Oxylite Meteor Shower
ClassicStyleStartMeteorShowers
380:
240:
380:
496.67 kg
,
7.14 kg
,
20.52 kg
,
2895 kg
,
7.14 kg
,
11.5 kg
,
55.07 kg
240:
313.33 kg
,
4.5 kg
,
12.93 kg
,
1833.5 kg
,
4.5 kg
,
7.28 kg
,
34.7 kg
20 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
ClassicStyleWarpMeteorShowers
160:
160:
979.52 kg
,
7.72 kg
,
6.9 kg
20 cycles
Gold Meteor Shower
Iron Meteor Shower
MiniMetallicSwampyMeteorShowers
128:
128:
3.67 kg
,
6.24 kg
,
789.04 kg
,
3.67 kg
,
255 kg
20 cycles
Slimy Meteor Shower
Gold Meteor Shower
MiniForestFrozenMeteorShowers
128:
128:
11.4 kg
,
1100.1 kg
20 cycles
Oxylite Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
RegolithMoonMeteorShowers
160:
96:
160:
15.52 tons
,
8.65 kg
,
23.2 kg
,
4.64 kg
96:
9.31 tons
,
5.2 kg
,
13.95 kg
,
2.8 kg
20 cycles
Regolith Meteor Shower
Iron Meteor Shower
Ice Meteor Shower
MiniRadioactiveOceanMeteorShowers
128:
128:
3.67 kg
,
229.09 kg
20 cycles
Uranium Meteor Shower
MiniBadlandsMeteorShowers
128:
128:
20.69 kg
,
55.52 kg
20 cycles
Ice Meteor Shower
SpacedOutStyleWarpMeteorShowers
128:
128:
168 kg
,
2.42 kg
,
6.9 kg
,
977.87 kg
,
2.42 kg
,
3.88 kg
,
18.51 kg
20 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
GassyMooteorShowers
96:
96:
0.10
20 cycles
Gassy Mooteor Shower
SpacedOutStyleRocketMeteorShowers
128:
128:
11.28 kg
,
1088.52 kg
20 cycles
Oxylite Meteor Shower
ClassicStyleStartMeteorShowers
380:
240:
380:
496 kg
,
7.13 kg
,
20.48 kg
,
2882.13 kg
,
7.13 kg
,
11.45 kg
,
54.96 kg
240:
314.67 kg
,
4.52 kg
,
12.97 kg
,
1827.07 kg
,
4.52 kg
,
7.26 kg
,
34.8 kg
20 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
ClassicStyleWarpMeteorShowers
160:
160:
983.71 kg
,
7.75 kg
,
6.97 kg
20 cycles
Gold Meteor Shower
Iron Meteor Shower
MiniMetallicSwampyMeteorShowers
128:
128:
3.62 kg
,
6.18 kg
,
780.74 kg
,
3.62 kg
,
252 kg
20 cycles
Slimy Meteor Shower
Gold Meteor Shower
MiniForestFrozenMeteorShowers
128:
128:
11.28 kg
,
1088.52 kg
20 cycles
Oxylite Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
RegolithMoonMeteorShowers
160:
96:
160:
24.82 tons
,
8.62 kg
,
23.13 kg
,
4.6 kg
96:
14.9 tons
,
5.18 kg
,
13.9 kg
,
2.79 kg
20 cycles
Regolith Meteor Shower
Iron Meteor Shower
Ice Meteor Shower
MiniRadioactiveOceanMeteorShowers
128:
128:
3.71 kg
,
231.82 kg
20 cycles
Uranium Meteor Shower
MiniBadlandsMeteorShowers
128:
128:
20.75 kg
,
55.67 kg
20 cycles
Ice Meteor Shower
SpacedOutStyleWarpMeteorShowers
128:
128:
168.33 kg
,
2.42 kg
,
6.92 kg
,
981.08 kg
,
2.42 kg
,
3.9 kg
,
18.56 kg
20 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
GassyMooteorShowers
96:
96:
0.10
20 cycles
Gassy Mooteor Shower
SpacedOutStyleRocketMeteorShowers
128:
128:
11.4 kg
,
1100.1 kg
20 cycles
Oxylite Meteor Shower
ClassicStyleStartMeteorShowers
380:
240:
380:
495 kg
,
7.12 kg
,
20.49 kg
,
2878.92 kg
,
7.12 kg
,
11.44 kg
,
54.98 kg
240:
313.33 kg
,
4.5 kg
,
12.93 kg
,
1817.42 kg
,
4.5 kg
,
7.22 kg
,
34.7 kg
20 cycles
Copper Meteor Shower
Ice Meteor Shower
Slimy Meteor Shower
ClassicStyleWarpMeteorShowers
160:
160:
979.52 kg
,
7.72 kg
,
6.9 kg
20 cycles
Gold Meteor Shower
Iron Meteor Shower
MiniMetallicSwampyMeteorShowers
128:
128:
3.63 kg
,
6.24 kg
,
789.04 kg
,
3.63 kg
,
252.5 kg
20 cycles
Slimy Meteor Shower
Gold Meteor Shower
MiniForestFrozenMeteorShowers
128:
128:
11.4 kg
,
1100.1 kg
20 cycles
Oxylite Meteor Shower
Frosty Planet Pack
Spring Showers
Default
Cosmic Storm
Doomsday
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
CeresMeteorShowers
256:
240:
160:
128:
64:
256:
16.56 kg
,
45.64 kg
,
0.14
240:
15.52 kg
,
42.77 kg
,
0.13
160:
10.42 kg
,
28.72 kg
,
0.09
128:
8.34 kg
,
22.98 kg
,
0.07
64:
4.17 kg
,
11.49 kg
,
0.04
40 cycles
Icy Nectar Meteor Shower
MiniCeresStartShowers
128:
128:
5.3 kg
,
6.2 kg
,
188.18 kg
40 cycles
Oxylite Meteor Shower
Blizzard Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
CeresMeteorShowers
256:
240:
160:
128:
64:
256:
49.79 kg
,
137.23 kg
,
0.43
240:
46.55 kg
,
128.3 kg
,
0.40
160:
31.03 kg
,
85.53 kg
,
0.27
128:
25.01 kg
,
68.94 kg
,
0.22
64:
12.51 kg
,
34.47 kg
,
0.11
20 cycles
Icy Nectar Meteor Shower
MiniCeresStartShowers
128:
128:
15.8 kg
,
18.68 kg
,
550.05 kg
20 cycles
Oxylite Meteor Shower
Blizzard Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
CeresMeteorShowers
256:
240:
160:
128:
64:
256:
49.66 kg
,
136.85 kg
,
0.54
240:
46.69 kg
,
128.68 kg
,
0.50
160:
31.13 kg
,
85.79 kg
,
0.34
128:
24.83 kg
,
68.42 kg
,
0.27
64:
12.41 kg
,
34.21 kg
,
0.13
20 cycles
Icy Nectar Meteor Shower
MiniCeresStartShowers
128:
128:
15.64 kg
,
18.5 kg
,
544.26 kg
20 cycles
Oxylite Meteor Shower
Blizzard Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
CeresMeteorShowers
256:
240:
160:
128:
64:
256:
49.68 kg
,
136.91 kg
,
0.86
240:
46.55 kg
,
128.3 kg
,
0.80
160:
31.03 kg
,
85.53 kg
,
0.54
128:
24.9 kg
,
68.62 kg
,
0.43
64:
12.51 kg
,
34.47 kg
,
0.22
20 cycles
Icy Nectar Meteor Shower
MiniCeresStartShowers
128:
128:
15.75 kg
,
18.59 kg
,
550.05 kg
20 cycles
Oxylite Meteor Shower
Blizzard Meteor Shower
Prehistoric Planet Pack
Spring Showers
Default
Cosmic Storm
Doomsday
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
LargeImpactor
256:
240:
160:
-
Demolior
PrehistoricMeteorShowers
256:
240:
160:
256:
405.48 kg
,
1.12 kg
,
1.05 kg
,
0.99 kg
240:
375.07 kg
,
1.03 kg
,
0.97 kg
,
0.92 kg
160:
256.18 kg
,
0.7 kg
,
0.66 kg
,
0.61 kg
100 cycles
Copper Meteor Shower
Iron Meteor Shower
Gold Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
LargeImpactor
256:
240:
160:
-
Demolior
PrehistoricMeteorShowers
256:
240:
160:
256:
1207.22 kg
,
3.33 kg
,
3.12 kg
,
2.94 kg
240:
1127.98 kg
,
3.11 kg
,
2.91 kg
,
2.76 kg
160:
750.14 kg
,
2.06 kg
,
1.94 kg
,
1.84 kg
50 cycles
Copper Meteor Shower
Iron Meteor Shower
Gold Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
LargeImpactor
256:
240:
160:
-
Demolior
PrehistoricMeteorShowers
256:
240:
160:
256:
1200.22 kg
,
3.29 kg
,
3.11 kg
,
2.94 kg
240:
1122.09 kg
,
3.08 kg
,
2.9 kg
,
2.77 kg
160:
746.11 kg
,
2.07 kg
,
1.92 kg
,
1.86 kg
50 cycles
Copper Meteor Shower
Iron Meteor Shower
Gold Meteor Shower
Season Name
Asteroids
(by width)
Material Per Cycle
(by width)
Period
Shower 1
Shower 2
Shower 3
LargeImpactor
256:
240:
160:
-
Demolior
PrehistoricMeteorShowers
256:
240:
160:
256:
1198.02 kg
,
3.31 kg
,
3.09 kg
,
2.94 kg
240:
1118.78 kg
,
3.09 kg
,
2.89 kg
,
2.76 kg
160:
750.14 kg
,
2.06 kg
,
1.94 kg
,
1.84 kg
50 cycles
Copper Meteor Shower
Iron Meteor Shower
Gold Meteor Shower
Meteor Shower Summary
Spring Showers
Default
Cosmic Storm
Doomsday
Name
Asteroids
(by width)
Composition
Active Period (s)
Inactive Period (s)
Meteor Interval
Shower Duration (s)
Material Per Event
(by width)
Affected By Difficulty
Slimy Meteor Shower
380:
240:
128:
50 %
Slime Meteor
25 %
Algae Meteor
25 %
Phosphoric Meteor
300 (Full Duration)
-
1 per 4.50s
300
380:
19.8 tons
,
284.63 kg
,
284.63 kg
240:
12.6 tons
,
181.13 kg
,
181.13 kg
128:
6.8 tons
,
97.75 kg
,
97.75 kg
Yes
Gassy Mooteor Shower
96:
100 %
Gassy Mooteor
15 (Full Duration)
-
1 per 3.12s
15
96:
2.00
No
Iridium Meteor Shower
256:
240:
160:
100 %
Iridium Meteor
30 (Full Duration)
-
1 per 0.75s
30
256:
2255 kg
240:
2090 kg
160:
1375 kg
Yes
Blizzard Meteor Shower
128:
66.67 %
Snow Meteor
33.33 %
Dust Fluff Meteor
600 (Full Duration)
-
1 per 4.50s
600
128:
495.69 kg
,
268 kg
Yes
Uranium Meteor Shower
128:
45.45 %
Uranium Meteor
36.36 %
Dust Fluff Meteor
18.18 %
Dust Meteor
150 (Full Duration)
-
1 per 6.75s
150
128:
3272.73 kg
,
52.36 kg
Yes
Regolith Meteor Shower
160:
96:
85.71 %
Dust Meteor
14.29 %
Rock Meteor
250±150
750±450
1 per 1.88s
9000
160:
621.18 tons
96:
373.04 tons
Yes
Gold Meteor Shower
380:
256:
66.67 %
Dust Meteor
26.67 %
Gold Meteor
6.67 %
Rock Meteor
75±25
1000±200
1 per 0.60s
3000
380:
199.95 tons
,
1588.53 kg
256:
134.71 tons
,
1070.27 kg
Yes
Bleach Stone Meteor Shower
81.25 %
Bleach Stone Meteor
18.75 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 4.50s
300
Yes
Iron Meteor Shower
256:
240:
160:
96:
57.14 %
Iron Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Dust Meteor
300 (Full Duration)
-
1 per 6.75s
300
256:
295.71 kg
,
154.29 kg
240:
276 kg
,
144 kg
160:
184 kg
,
96 kg
96:
111.71 kg
,
58.29 kg
Yes
Gold Meteor Shower
256:
240:
160:
128:
57.14 %
Gold Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Rock Meteor
75 (Full Duration)
-
1 per 1.50s
75
256:
42.36 tons
,
335.14 kg
240:
39.04 tons
,
308.86 kg
160:
26.58 tons
,
210.29 kg
128:
21.59 tons
,
170.86 kg
Yes
Fullerene Meteor Shower
85.71 %
Fullerene Meteor
14.29 %
Dust Meteor
30 (Full Duration)
-
1 per 0.50s
30
No
Icy Nectar Meteor Shower
256:
240:
160:
128:
64:
88 %
Snow Meteor
8 %
Ice Meteor
4 %
Bonbon Meteor
300 (Full Duration)
-
1 per 2.10s
300
256:
1825.51 kg
,
662.38 kg
,
5.72
240:
1710.62 kg
,
620.69 kg
,
5.36
160:
1148.92 kg
,
416.88 kg
,
3.60
128:
919.14 kg
,
333.5 kg
,
2.88
64:
459.57 kg
,
166.75 kg
,
1.44
Yes
Dense Dust Meteor Shower
50 %
Dust Meteor
33.33 %
Rock Meteor
16.67 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 5.25s
300
Yes
Copper Meteor Shower
380:
256:
240:
160:
128:
66.67 %
Copper Meteor
33.33 %
Rock Meteor
150 (Full Duration)
-
1 per 3.75s
150
380:
115.8 tons
,
460 kg
256:
79.13 tons
,
314.33 kg
240:
73.34 tons
,
291.33 kg
160:
50.18 tons
,
199.33 kg
128:
40.53 tons
,
161 kg
Yes
Dust Fluff Meteor Shower
50 %
Dust Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 6.00s
300
Yes
Iron Meteor Shower
380:
256:
62.5 %
Dust Meteor
25 %
Rock Meteor
12.5 %
Iron Meteor
250±150
750±450
1 per 1.88s
6000
380:
1719.63 tons
,
1707.75 kg
256:
1159.45 tons
,
1151.44 kg
Yes
Copper Meteor Shower
380:
256:
50 %
Copper Meteor
50 %
Rock Meteor
250±150
750±450
1 per 8.25s
4200
380:
547.16 tons
,
1086.75 kg
256:
370.56 tons
,
736 kg
Yes
Ice Meteor Shower
380:
240:
160:
128:
96:
93.33 %
Snow Meteor
6.67 %
Ice Meteor
300 (Full Duration)
-
1 per 2.10s
300
380:
2206.18 kg
,
822.18 kg
240:
1387.93 kg
,
517.24 kg
160:
932.19 kg
,
347.4 kg
128:
745.75 kg
,
277.92 kg
96:
559.31 kg
,
208.44 kg
Yes
Oxylite Meteor Shower
128:
50 %
Oxylite Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 6.00s
300
128:
15.05 tons
,
156 kg
Yes
Name
Asteroids
(by width)
Composition
Active Period (s)
Inactive Period (s)
Meteor Interval
Shower Duration (s)
Material Per Event
(by width)
Affected By Difficulty
Slimy Meteor Shower
380:
240:
128:
50 %
Slime Meteor
25 %
Algae Meteor
25 %
Phosphoric Meteor
300 (Full Duration)
-
1 per 3.00s
300
380:
29.8 tons
,
428.38 kg
,
428.38 kg
240:
18.8 tons
,
270.25 kg
,
270.25 kg
128:
10.2 tons
,
146.63 kg
,
146.63 kg
Yes
Gassy Mooteor Shower
96:
100 %
Gassy Mooteor
15 (Full Duration)
-
1 per 3.12s
15
96:
2.00
No
Iridium Meteor Shower
256:
240:
160:
100 %
Iridium Meteor
30 (Full Duration)
-
1 per 0.50s
30
256:
3355 kg
240:
3135 kg
160:
2090 kg
Yes
Blizzard Meteor Shower
128:
66.67 %
Snow Meteor
33.33 %
Dust Fluff Meteor
600 (Full Duration)
-
1 per 3.00s
600
128:
747.23 kg
,
404 kg
Yes
Uranium Meteor Shower
128:
45.45 %
Uranium Meteor
36.36 %
Dust Fluff Meteor
18.18 %
Dust Meteor
150 (Full Duration)
-
1 per 4.50s
150
128:
4636.36 kg
,
74.18 kg
Yes
Regolith Meteor Shower
160:
96:
85.71 %
Dust Meteor
14.29 %
Rock Meteor
250±150
750±450
1 per 1.25s
9000
160:
931.36 tons
96:
559.15 tons
Yes
Gold Meteor Shower
380:
256:
66.67 %
Dust Meteor
26.67 %
Gold Meteor
6.67 %
Rock Meteor
75±25
1000±200
1 per 0.40s
3000
380:
299.92 tons
,
2382.8 kg
256:
202.26 tons
,
1606.93 kg
Yes
Bleach Stone Meteor Shower
81.25 %
Bleach Stone Meteor
18.75 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 3.00s
300
Yes
Iron Meteor Shower
256:
240:
160:
96:
57.14 %
Iron Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Dust Meteor
300 (Full Duration)
-
1 per 4.50s
300
256:
440.29 kg
,
229.71 kg
240:
414 kg
,
216 kg
160:
276 kg
,
144 kg
96:
170.86 kg
,
89.14 kg
Yes
Gold Meteor Shower
256:
240:
160:
128:
57.14 %
Gold Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Rock Meteor
75 (Full Duration)
-
1 per 1.00s
75
256:
63.12 tons
,
499.43 kg
240:
58.97 tons
,
466.57 kg
160:
39.04 tons
,
308.86 kg
128:
31.56 tons
,
249.71 kg
Yes
Fullerene Meteor Shower
85.71 %
Fullerene Meteor
14.29 %
Dust Meteor
30 (Full Duration)
-
1 per 0.50s
30
No
Icy Nectar Meteor Shower
256:
240:
160:
128:
64:
88 %
Snow Meteor
8 %
Ice Meteor
4 %
Bonbon Meteor
300 (Full Duration)
-
1 per 1.40s
300
256:
2744.65 kg
,
995.88 kg
,
8.60
240:
2565.93 kg
,
931.03 kg
,
8.04
160:
1710.62 kg
,
620.69 kg
,
5.36
128:
1378.71 kg
,
500.26 kg
,
4.32
64:
689.35 kg
,
250.13 kg
,
2.16
Yes
Dense Dust Meteor Shower
50 %
Dust Meteor
33.33 %
Rock Meteor
16.67 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 3.50s
300
Yes
Copper Meteor Shower
380:
256:
240:
160:
128:
66.67 %
Copper Meteor
33.33 %
Rock Meteor
150 (Full Duration)
-
1 per 2.50s
150
380:
173.7 tons
,
690 kg
256:
117.73 tons
,
467.67 kg
240:
110.01 tons
,
437 kg
160:
73.34 tons
,
291.33 kg
128:
59.83 tons
,
237.67 kg
Yes
Dust Fluff Meteor Shower
50 %
Dust Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 4.00s
300
Yes
Iron Meteor Shower
380:
256:
62.5 %
Dust Meteor
25 %
Rock Meteor
12.5 %
Iron Meteor
250±150
750±450
1 per 1.25s
6000
380:
2579.45 tons
,
2561.63 kg
256:
1738.45 tons
,
1726.44 kg
Yes
Copper Meteor Shower
380:
256:
50 %
Copper Meteor
50 %
Rock Meteor
250±150
750±450
1 per 5.50s
4200
380:
822.18 tons
,
1633 kg
256:
552.95 tons
,
1098.25 kg
Yes
Ice Meteor Shower
380:
240:
160:
128:
96:
93.33 %
Snow Meteor
6.67 %
Ice Meteor
300 (Full Duration)
-
1 per 1.40s
300
380:
3304.1 kg
,
1231.34 kg
240:
2081.89 kg
,
775.86 kg
160:
1387.93 kg
,
517.24 kg
128:
1118.63 kg
,
416.88 kg
96:
838.97 kg
,
312.66 kg
Yes
Oxylite Meteor Shower
128:
50 %
Oxylite Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 4.00s
300
128:
22 tons
,
228 kg
Yes
Name
Asteroids
(by width)
Composition
Active Period (s)
Inactive Period (s)
Meteor Interval
Shower Duration (s)
Material Per Event
(by width)
Affected By Difficulty
Slimy Meteor Shower
380:
240:
128:
50 %
Slime Meteor
25 %
Algae Meteor
25 %
Phosphoric Meteor
300 (Full Duration)
-
1 per 2.40s
300
380:
29.76 tons
,
427.8 kg
,
427.8 kg
240:
18.88 tons
,
271.4 kg
,
271.4 kg
128:
10.08 tons
,
144.9 kg
,
144.9 kg
Yes
Gassy Mooteor Shower
96:
100 %
Gassy Mooteor
15 (Full Duration)
-
1 per 3.12s
15
96:
2.00
No
Iridium Meteor Shower
256:
240:
160:
100 %
Iridium Meteor
30 (Full Duration)
-
1 per 0.40s
30
256:
3344 kg
240:
3124 kg
160:
2068 kg
Yes
Blizzard Meteor Shower
128:
66.67 %
Snow Meteor
33.33 %
Dust Fluff Meteor
600 (Full Duration)
-
1 per 2.40s
600
128:
739.83 kg
,
400 kg
Yes
Uranium Meteor Shower
128:
45.45 %
Uranium Meteor
36.36 %
Dust Fluff Meteor
18.18 %
Dust Meteor
150 (Full Duration)
-
1 per 3.60s
150
128:
4581.82 kg
,
73.31 kg
Yes
Regolith Meteor Shower
160:
96:
85.71 %
Dust Meteor
14.29 %
Rock Meteor
250±150
750±450
1 per 1.00s
9000
160:
931.03 tons
96:
558.49 tons
Yes
Gold Meteor Shower
380:
256:
66.67 %
Dust Meteor
26.67 %
Gold Meteor
6.67 %
Rock Meteor
75±25
1000±200
1 per 0.32s
3000
380:
299.84 tons
,
2382.19 kg
256:
202.26 tons
,
1606.93 kg
Yes
Bleach Stone Meteor Shower
81.25 %
Bleach Stone Meteor
18.75 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 2.40s
300
Yes
Iron Meteor Shower
256:
240:
160:
96:
57.14 %
Iron Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Dust Meteor
300 (Full Duration)
-
1 per 3.60s
300
256:
441.6 kg
,
230.4 kg
240:
415.31 kg
,
216.69 kg
160:
278.63 kg
,
145.37 kg
96:
168.23 kg
,
87.77 kg
Yes
Gold Meteor Shower
256:
240:
160:
128:
57.14 %
Gold Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Rock Meteor
75 (Full Duration)
-
1 per 0.80s
75
256:
62.46 tons
,
494.17 kg
240:
58.47 tons
,
462.63 kg
160:
39.2 tons
,
310.17 kg
128:
31.23 tons
,
247.09 kg
Yes
Fullerene Meteor Shower
85.71 %
Fullerene Meteor
14.29 %
Dust Meteor
30 (Full Duration)
-
1 per 0.50s
30
No
Icy Nectar Meteor Shower
256:
240:
160:
128:
64:
88 %
Snow Meteor
8 %
Ice Meteor
4 %
Bonbon Meteor
300 (Full Duration)
-
1 per 1.12s
300
256:
2736.99 kg
,
993.1 kg
,
10.72
240:
2573.59 kg
,
933.81 kg
,
10.08
160:
1715.72 kg
,
622.54 kg
,
6.72
128:
1368.49 kg
,
496.55 kg
,
5.36
64:
684.25 kg
,
248.28 kg
,
2.68
Yes
Dense Dust Meteor Shower
50 %
Dust Meteor
33.33 %
Rock Meteor
16.67 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 2.80s
300
Yes
Copper Meteor Shower
380:
256:
240:
160:
128:
66.67 %
Copper Meteor
33.33 %
Rock Meteor
150 (Full Duration)
-
1 per 2.00s
150
380:
172.93 tons
,
686.93 kg
256:
117.34 tons
,
466.13 kg
240:
109.62 tons
,
435.47 kg
160:
72.57 tons
,
288.27 kg
128:
58.67 tons
,
233.07 kg
Yes
Dust Fluff Meteor Shower
50 %
Dust Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 3.20s
300
Yes
Iron Meteor Shower
380:
256:
62.5 %
Dust Meteor
25 %
Rock Meteor
12.5 %
Iron Meteor
250±150
750±450
1 per 1.00s
6000
380:
2578.87 tons
,
2561.05 kg
256:
1738.16 tons
,
1726.15 kg
Yes
Copper Meteor Shower
380:
256:
50 %
Copper Meteor
50 %
Rock Meteor
250±150
750±450
1 per 4.40s
4200
380:
822.18 tons
,
1633 kg
256:
553.52 tons
,
1099.4 kg
Yes
Ice Meteor Shower
380:
240:
160:
128:
96:
93.33 %
Snow Meteor
6.67 %
Ice Meteor
300 (Full Duration)
-
1 per 1.12s
300
380:
3297.88 kg
,
1229.02 kg
240:
2088.11 kg
,
778.18 kg
160:
1392.07 kg
,
518.78 kg
128:
1110.34 kg
,
413.79 kg
96:
836.9 kg
,
311.89 kg
Yes
Oxylite Meteor Shower
128:
50 %
Oxylite Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 3.20s
300
128:
21.77 tons
,
225.6 kg
Yes
Name
Asteroids
(by width)
Composition
Active Period (s)
Inactive Period (s)
Meteor Interval
Shower Duration (s)
Material Per Event
(by width)
Affected By Difficulty
Slimy Meteor Shower
380:
240:
128:
50 %
Slime Meteor
25 %
Algae Meteor
25 %
Phosphoric Meteor
300 (Full Duration)
-
1 per 1.50s
300
380:
29.7 tons
,
426.94 kg
,
426.94 kg
240:
18.8 tons
,
270.25 kg
,
270.25 kg
128:
10.1 tons
,
145.19 kg
,
145.19 kg
Yes
Gassy Mooteor Shower
96:
100 %
Gassy Mooteor
15 (Full Duration)
-
1 per 3.12s
15
96:
2.00
No
Iridium Meteor Shower
256:
240:
160:
100 %
Iridium Meteor
30 (Full Duration)
-
1 per 0.25s
30
256:
3327.5 kg
240:
3107.5 kg
160:
2090 kg
Yes
Blizzard Meteor Shower
128:
66.67 %
Snow Meteor
33.33 %
Dust Fluff Meteor
600 (Full Duration)
-
1 per 1.50s
600
128:
743.53 kg
,
402 kg
Yes
Uranium Meteor Shower
128:
45.45 %
Uranium Meteor
36.36 %
Dust Fluff Meteor
18.18 %
Dust Meteor
150 (Full Duration)
-
1 per 2.25s
150
128:
4636.36 kg
,
74.18 kg
Yes
Regolith Meteor Shower
160:
96:
85.71 %
Dust Meteor
14.29 %
Rock Meteor
250±150
375±225
1 per 0.62s
9000
160:
1489.27 tons
96:
893.73 tons
Yes
Gold Meteor Shower
380:
256:
66.67 %
Dust Meteor
26.67 %
Gold Meteor
6.67 %
Rock Meteor
75±25
500±100
1 per 0.20s
3000
380:
560.67 tons
,
4454.33 kg
256:
377.7 tons
,
3000.73 kg
Yes
Bleach Stone Meteor Shower
81.25 %
Bleach Stone Meteor
18.75 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 1.50s
300
Yes
Iron Meteor Shower
256:
240:
160:
96:
57.14 %
Iron Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Dust Meteor
300 (Full Duration)
-
1 per 2.25s
300
256:
440.29 kg
,
229.71 kg
240:
414 kg
,
216 kg
160:
276 kg
,
144 kg
96:
167.57 kg
,
87.43 kg
Yes
Gold Meteor Shower
256:
240:
160:
128:
57.14 %
Gold Meteor
28.57 %
Dust Fluff Meteor
14.29 %
Rock Meteor
75 (Full Duration)
-
1 per 0.50s
75
256:
62.71 tons
,
496.14 kg
240:
58.56 tons
,
463.29 kg
160:
39.04 tons
,
308.86 kg
128:
31.56 tons
,
249.71 kg
Yes
Fullerene Meteor Shower
85.71 %
Fullerene Meteor
14.29 %
Dust Meteor
30 (Full Duration)
-
1 per 0.50s
30
No
Icy Nectar Meteor Shower
256:
240:
160:
128:
64:
88 %
Snow Meteor
8 %
Ice Meteor
4 %
Bonbon Meteor
300 (Full Duration)
-
1 per 0.70s
300
256:
2738.26 kg
,
993.56 kg
,
17.16
240:
2565.93 kg
,
931.03 kg
,
16.08
160:
1710.62 kg
,
620.69 kg
,
10.72
128:
1372.32 kg
,
497.94 kg
,
8.60
64:
689.35 kg
,
250.13 kg
,
4.32
Yes
Dense Dust Meteor Shower
50 %
Dust Meteor
33.33 %
Rock Meteor
16.67 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 1.75s
300
Yes
Copper Meteor Shower
380:
256:
240:
160:
128:
66.67 %
Copper Meteor
33.33 %
Rock Meteor
150 (Full Duration)
-
1 per 1.25s
150
380:
172.74 tons
,
686.17 kg
256:
116.77 tons
,
463.83 kg
240:
109.05 tons
,
433.17 kg
160:
73.34 tons
,
291.33 kg
128:
58.87 tons
,
233.83 kg
Yes
Dust Fluff Meteor Shower
50 %
Dust Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 2.00s
300
Yes
Iron Meteor Shower
380:
256:
62.5 %
Dust Meteor
25 %
Rock Meteor
12.5 %
Iron Meteor
250±150
375±225
1 per 0.62s
6000
380:
4126.1 tons
,
4097.59 kg
256:
2779.92 tons
,
2760.72 kg
Yes
Copper Meteor Shower
380:
256:
50 %
Copper Meteor
50 %
Rock Meteor
250±150
375±225
1 per 2.75s
4200
380:
1312.88 tons
,
2607.63 kg
256:
884.42 tons
,
1756.63 kg
Yes
Ice Meteor Shower
380:
240:
160:
128:
96:
93.33 %
Snow Meteor
6.67 %
Ice Meteor
300 (Full Duration)
-
1 per 0.70s
300
380:
3298.92 kg
,
1229.41 kg
240:
2081.89 kg
,
775.86 kg
160:
1387.93 kg
,
517.24 kg
128:
1113.45 kg
,
414.95 kg
96:
833.79 kg
,
310.73 kg
Yes
Oxylite Meteor Shower
128:
50 %
Oxylite Meteor
50 %
Dust Fluff Meteor
300 (Full Duration)
-
1 per 2.00s
300
128:
22 tons
,
228 kg
Yes
See Also
Space Scanner
- Detects incoming meteor showers
Bunker Door
/
Bunker Tile
- Protection from meteor damage
Blastshot Maker
- Protection from meteor damage
Space Biome
- Where meteors land
References
↑
The starting planetoid is always considered "visited" from cycle 1.
↑
Spawning Duplicants through Debug Mode does not count as a visit, but Sandbox Mode does.
↑
Uranium meteors are configured to place 6 tiles with
maxHeight = 1
, causing 5 tiles to be removed by depth-reduction, resulting in about
83% material loss
. This makes the
Meteor Blaster
particularly valuable as destroying meteors yields more material than excavating tiles.
↑
Some showers (like Gassy Mooteor and Fullerene) are marked "Not Affected by Difficulty" and ignore these multipliers entirely.
↑
Random selection is capped from at 5 showers, but no season currently has more than 5.
History
LU-357226
: Added two new meteor showers, one with Copper meteors, and one with Gold Amalgam meteors.
