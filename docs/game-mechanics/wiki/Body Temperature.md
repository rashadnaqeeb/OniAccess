# Body Temperature

Duplicants naturally regulate their
Body Temperature
(Homeostasis) toward
37 °C
/
98.6 °F
. If their temperature is below this point, they gain
580 DTU/s
of
Heat
; if above, they lose
580 DTU/s
. Additionally, Duplicants continuously generate
84 DTU/s
of heat.
Although deviations in Body Temperature do not directly harm
Duplicants
, it influences various temperature-related effects and interactions which do harm Duplicants. (Counter-intuitively,
Scalding
and
Frostbite
is
not
affected by body temperature and is only related to the temperature of the environment)
Environmental Exchange
Creatures are considered
entities
within a cell, and thus is a specific application of
Thermal Conductivity
rules. They exchange heat with their surroundings every game tick (0.2 seconds) based on the following formula:
q
=
Δ
T
⋅
Δ
t
⋅
min
⁡
(
k
,
0
.
6
)
⋅
A
L
Where:
q
: Heat transferred in DTU per game tick (Environmental Exchange)
Δ
T
: Difference in temperature between the cell and Creature
Δ
t
: The passing time, which is always one tick
0.2 s
k
: Thermal conductivity of the cell (capped at 0.6)
A
: Surface area (1 for Duplicants, 17.5 for
Bammoths
, otherwise defaults to 10)
L
: Insulation thickness (Variable for Duplicants; see below. 0.025 for
Bammoths
, otherwise defaults to 0.01)
Note that unlike debris and other creatures, Duplicants and Bammoths do not exchange heat with cells below them.
Example:
A Duplicant is typically at
37 °C
/
98.6 °F
with an Insulation Thickness of
0.002 meters
. Standard Clothing has an Insulation Thickness of
0.0025
, totaling an Insulation Thickness of
0.0045 meters
.
If this Duplicant is in
22 °C
/
71.6 °F
Oxygen
which has 0.024 Thermal Conductivity, then the heat transferred per game tick is
q
=
(
2
2
−
3
7
)
⋅
0
.
2
⋅
0
.
0
2
4
0
.
0
0
4
5
=
−
1
6
This is the same value that is reported in-game in the Environmental Exchange field in the Body Temperature Tooltip, although the value in-game is incorrectly labeled as DTU/s
Chilly and Toasty Surroundings
Chilly/Toasty Surroundings have the following effects:
-5
Athletics
-50%/cycle Stamina
+10%/cycle
Stress
Duplicants obtain
Chilly Surroundings
if their average environment exchange is below
−39 DTU/tick
for more than 6 seconds. Similarly, Duplicants obtain
Toasty Surroundings
if their average environmental exchange is greater than
8 DTU/tick
for more than 6 seconds. The average environmental exchange is loosely the
Environmental  Exchange
averaged over 4 seconds.
For a given thermal conductivity
k
and Insulation Thickness
L
, the temperature for Chilly/Toasty Surroundings to take into effect:
T
❄️
<
3
7
−
1
9
5
⋅
L
min
⁡
(
k
,
0
.
6
)
T
♨
>
3
7
+
4
0
⋅
L
min
⁡
(
k
,
0
.
6
)
Duplicants in
Atmo Suits
,
Lead Suits
, and
Jet Suits
are immune to the effects of Toasty/Chilly Surroundings.
The Frost-Proof trait provides immunity to Chilly Surroundings.
Curried Beans
and
Spicy Tofu
provide immunity to Chilly Surroundings for 1 cycle.
A Warming Station, like the
Wood Heater
and
Space Heater
, give immunity to Chilly Surroundings within its vicinity. Duplicants will seek out Warming Stations to cure Chilly Surroundings. They gain Frost Resistant for 0.2 cycles, becoming immune to Chilly Surroundings and Soggy Feet.
Duplicants will seek out Cooling Stations, like the
Water Cooler
, to cure Toasty Surroundings. They gain Heat Resistant for 0.2 cycles,  becoming immune to Toasty Surroundings.
Tolerances for Standard Clothing
This category also includes clothing like
Pajamas
and
Snazzy Suits
. A Duplicant with standard clothing has an insulation thickness of
0.0045 meters
.
Element
Chilly Surroundings
Toasty Surroundings
Oxygen
0.44 °C
/
32.79 °F
44.5 °C
/
112.1 °F
Chlorine
-71.33 °C
/
-96.39 °F
59.22 °C
/
138.6 °F
Carbon Dioxide
-23.1 °C
/
-9.58 °F
49.33 °C
/
120.79 °F
Sour Gas
-11.75 °C
/
10.85 °F
47 °C
/
116.6 °F
Natural Gas
11.93 °C
/
53.47 °F
42.14 °C
/
107.85 °F
Hydrogen
31.78 °C
/
89.2 °F
38.07 °C
/
100.53 °F
Water
35.56 °C
/
96.01 °F
37.3 °C
/
99.14 °F
Tolerances for Warm Coat
A Duplicant with a
Warm Coat
has an insulation thickness of
0.01 meters
.
Element
Chilly Surroundings
Toasty Surroundings
Oxygen
-44.25 °C
/
-47.65 °F
53.67 °C
/
128.61 °F
Chlorine
-203.74 °C
/
-334.73 °F
86.38 °C
/
187.48 °F
Carbon Dioxide
-96.56 °C
/
-141.81 °F
64.4 °C
/
147.92 °F
Sour Gas
-71.33 °C
/
-96.39 °F
59.22 °C
/
138.6 °F
Natural Gas
-18.71 °C
/
-1.68 °F
48.43 °C
/
119.17 °F
Hydrogen
25.39 °C
/
77.7 °F
39.38 °C
/
102.88 °F
Water
33.8 °C
/
92.84 °F
37.66 °C
/
99.79 °F
