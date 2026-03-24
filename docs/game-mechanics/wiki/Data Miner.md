# Data Miner

This article may be
outdated
. It was last updated for
U54-644175
. These versions were tagged as updating the game mechanics discussed here:
U54-646687
This article is related to
The Bionic Booster Pack
content.
Data Miner
Mass-produces
Data Banks
that can be processed into Research points.
Duplicants will not fabricate items unless recipes are queued.
Data banks can also be used to program robo-pilot.
Research
Data Science
Dimensions
3×2 tiles
Category
Stations
Power
-1 kW
Heat
+
3.5 kDTU/s
Overheat
at
75 °C
/
167 °F
Decor
0
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
The
Data Miner
is a specialized building that automatically produces
Data Banks
, which can then be processed into Research points or operate autonomous machinery like
Robo-Pilot Module
and
Remote Controller
. Unlike other buildings, its productivity is influenced by the building's temperature: the colder the building, the more efficiently it produces Data Banks.
The productivity rate ranges from 60% to 533.33% and is linearly interpolated across the temperature range from
-263.15 °C
/
-441.67 °F
to
51.85 °C
/
125.33 °F
. Temperatures outside this range are clamped to the nearest boundary. At 100% productivity (T =
23.23 °C
/
73.81 °F
), the Data Miner produces one Data Bank every 200 seconds.
The Data Miner requires 5 kg of Plastic per Data Bank unit, which is the same conversion ratio as the
Orbital Data Collection Lab
. With a sustainable source of Plastic, the lab can supply a colony with data banks indefinitely.
The mathematical formula for productivity is given by the following piecewise function:
P
(
T
)
=
{
5
3
3
.
3
3
%
f
o
r
−
2
7
3
,
1
5
<
T
<
−
2
6
3
,
1
5
0
.
6
+
(
5
.
3
‾
−
0
.
6
)
×
(
1
−
T
+
2
6
3
.
1
5
3
1
5
)
f
o
r
−
2
6
3
,
1
5
≤
T
≤
5
8
,
1
5
6
0
%
f
o
r
5
8
,
1
5
<
T
<
2
9
2
9
.
8
5
}
Note: the maximum value is given as such since that is the highest temperature this building can withstand when made of
Wolframite
before melting. Long before that the building would overheat however before utterly failing it could work for some time.
Example Productivity Rates:
For
T
=
−
1
0
0
°
C
:
P
(
−
1
0
0
)
=
0
.
6
+
(
5
.
3
‾
−
0
.
6
)
×
(
1
−
−
1
0
0
+
2
6
3
.
1
5
3
1
5
)
≈
2
.
8
8
1
8
≈
2
8
8
.
1
8
%
For
T
=
0
°
C
:
P
(
0
)
=
0
.
6
+
(
5
.
3
‾
−
0
.
6
)
×
(
1
−
0
+
2
6
3
.
1
5
3
1
5
)
≈
1
.
3
7
9
1
≈
1
3
7
.
9
1
%
For
T
=
1
0
0
°
C
:
(beyond the intended range, clamp to
T
=
5
1
.
8
5
°
C
:
P
(
5
1
.
8
5
)
=
0
.
6
+
(
5
.
3
‾
−
0
.
6
)
×
(
1
−
5
1
.
8
5
+
2
6
3
.
1
5
3
1
5
)
=
0
.
6
=
6
0
%
The mathematical formula for recipe time is given by the following function:
t
(
P
(
T
)
)
=
2
0
0
1
P
(
T
)
Temperature, T (°C)
Productivity, P(T) (%)
Recipe Time, t (s)
-273.15
533.33%
37.50
-263.15
533.33%
37.50
-260
528.60%
37.84
-240
498.55%
40.11
-220
468.49%
42.69
-200
438.44%
45.62
-180
408.39%
48.97
-160
378.34%
52.86
-140
348.28%
57.43
-120
318.23%
62.85
-100
288.18%
69.40
-80
258.12%
77.48
-60
228.07%
87.69
-40
198.02%
101.00
-20
167.97%
119.07
0
137.91%
145.02
20
107.86%
185.43
25.2303
100.00%
200.00
40
77.81%
257.04
51.85
60%
333.33
60
60%
333.33
Note:
The in-game description is somewhat misleading. The Data Miner does not require Duplicant operation, and it is significantly slower than an
Orbital Data Collection Lab
.
The production time of the Orbital Data Collection Lab is 33 seconds, improved by the operating duplicant's Machinery attribute, so even at maximum productivity rate, a Data Miner takes about 14% longer per Data Bank versus a 0-Machinery duplicant at an orbital lab.
Its main advantage is that it can function outside of a Rocket interior and in non-space environments, allowing Data Banks to be produced fully automatically without any Duplicant labor.
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
