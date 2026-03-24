# Experiment 52B

This article is related to
Spaced Out
content.
Experiment 52B
A Sap-producing cybernetic tree that shows signs of sentience.
It is rooted firmly in place, and is waiting for some brave soul to bring it food.
Effect
Converts
50 g
per second of Food into Sap (
1 kg
/
200 kcal
)
Spawn Temperature
19.85 °C
/
67.73 °F
Temperature
-100 °C ↔ 100 °C
/
-148 °F ↔ 212 °F
Lethal Temperature Threshold
Below
-273.15 °C
/
-459.67 °F
or above
750 °C
/
1382 °F
Atmospheric Pressure
Ignored
Radiation Range
Ignored
Plant Size
5×5
Decor
35 (Radius: 6 Tiles)
Code
Internal Id
SapTree
String Id
STRINGS.CREATURES.SPECIES.SAPTREE.NAME
Experiment 52B
is a
Sap
-producing cybernetic tree that shows signs of sentience and is found on the
Marshy Asteroid
.  If the Marshy Asteroid is replaced by another asteroid such as a
Ceres Fragment Asteroid
then Experiment 52B will spawn there as well as a
Tungsten Volcano
.
The point of interest spawns Experiment 52B within the confines of an abandoned Gravitas greenhouse. It produces
Sap
, which has to be heated to get
Isosap
. It does not need an atmosphere and cannot be cultivated or uprooted. The tree is hostile and will lash out at any nearby Duplicant that comes near, inflicting damage to nearby critters and Duplicants.
Experiment 52B is considered a stationary creature that occupy the building layer. It is not a critter nor a plant, but behave most similarly to plants; buildings can't be constructed on top and will "die" the moment the hive's body temperatures exceeds its livable range. Like plants,
Conduction Panels
do not directly interact with Experiment 52B. They do not count as a plant for the purposes of
pip planting
.
Feeding
The tree eats the same food that Duplicants eat, i.e. anything that has
calorie ratings
.
Experiment 52B is immune to
Food Poisoning
.
Sap Production
After the tree has consumed enough kcals of food, it begins to spew Sap onto the ground. It consumes 50 g/s of food while producing 1 kg of Sap per 200 kcal, meaning foods with higher calories per kg produce Sap at a higher rate. Once 5 kg of Sap has been produced, it vents the Sap at a rate of 1.5 kg/s (3.33 s), during which time it doesn't consume food.
Food must be placed directly under the tree. This is the only area in which it is able to grab the food.
Experiment 52B does not need an atmosphere and does work in vacuum. Sap output temperature is that of the experiment itself, up to
100 °C
/
212 °F
. Keeping the tree hot and insulated is beneficial since it significantly reduces heat needed to boil the Sap. Keeping the tree below Sap's freeze point of
20 °C
/
68 °F
(real
17 °C
/
62.6 °F
) is inadvisable since Sap output forms natural tiles at the base that stifle the tree.
Analysis
Higher calorie density foods result in more time spent venting (because it reaches 5 kg of Sap faster) and thus less food consumed per cycle. Over a full cycle, around 25 kg of 1000 kcal/kg food can be consumed, or around 20 kg of 2800 kcal/kg food, or around 15 kg of 6000 kcal/kg food.
Sap must be heated above
125 °C
/
257 °F
in order to produce usable Isosap. As this temperature is above Experiment 52B's overheat temperature, it must be moved to a different area before heating. Heating Sap will produce 25% mass as Isosap and the remainder as
Steam
, essentially producing 250 g of Isosap and 750 g of water per 200 kcal of food.
Producing 100 kg of
Insulite
requires 15 kg of Isosap, costing 12000 kcal.
Producing 100 kg of
Visco-Gel Fluid
requires 35 kg of Isosap, costing 28000 kcal.
As the tree has a limited consumption of mass per cycle, and only one tree exists, the only way to increase
per cycle
production is to feed it foods with higher calorie density. Feeding Experiment 52B on
Meal Lice
(600 kcal/kg) will produce
1
2
0
0
kg
kCal
×
6
0
0
kCal
kg
×
3
0
kg
cycle
=
9
0
kg
cycle
(
excluding
Sap
venting), but feeding it
Surf'n'Turf
or
Frost Burger
(6000 kcal/kg) will instead produce 450 kg/cycle (
including
Sap
venting).
Sap's boiling point is exactly the minimum input temperature of a
Steam Turbine
, which simplifies heat management significantly. Due to the relatively small amounts of water produced, reusing it in agriculture does not significantly reduce the food costs of Sap production; e.g., feeding a
Gristle Berry
(2000 kcal) will produce 10 kg of Sap and subsequently 7.5 kg of steam/water, while the berry required 120 kg of water to grow.
Production Rate by Food Type
The following table shows the production rate for each food type. This table properly takes into account the production and venting phase of the tree per cycle.
We can see from the "Food Consumed" column that even though the tree consumes food at a fixed 0.050kg/s (which would amount to 30kg/cycle), the effective consumption rate is reduced due to the fact that it takes exactly
3.33
seconds (5kg / 1.5kg/s) to vent the Sap each time 5kg has been accumulated, and this happens multiple times per cycle. Hence, the effective consumption and production rate is significantly affected by the kcal density of the food as more venting is required during a cycle.
For reference, the right two columns show the effective amount of
Insulite
or
Visco-Gel Fluid
that could be produced assuming all the Sap was converted to those materials.
Production Rate by Food Type (including production and venting phase)
Food
Food Density
(kcal/kg)
Food Consumed
(kg/cycle)
Food Consumed
(kcal/cycle)
Sap Produced
(kg/cycle)
Steam Produced
(kg/cycle)
Isosap Produced
(kg/cycle)
Insulite Produced
(kg/cycle)
Visco-Gel Produced
(kg/cycle)
Grubfruit
250
28.8
7,200
36
27
9
60
25.71
Ovagro Fig
325
28.46
9,249.01
46.25
34.68
11.56
77.08
33.03
Lettuce
400
28.13
11,250
56.25
42.19
14.06
93.75
40.18
Meal Lice
600
27.27
16,363.64
81.82
61.36
20.45
136.36
58.44
Muckroot
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Mush Bar
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Nutrient Bar
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Pikeapple
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Sherberry
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Snac Fruit
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Spindly Grubfruit
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Sweatcorn
800
26.47
21,176.47
105.88
79.41
26.47
176.47
75.63
Jawbo Fillet
1,000
25.71
25,714.29
128.57
96.43
32.14
214.29
91.84
Pacu Fillet
1,000
25.71
25,714.29
128.57
96.43
32.14
214.29
91.84
Raw Shellfish
1,000
25.71
25,714.29
128.57
96.43
32.14
214.29
91.84
Mush Fry
1,050
25.53
26,808.51
134.04
100.53
33.51
223.4
95.74
Frost Bun
1,200
25
30,000
150
112.5
37.5
250
107.14
Pikeapple Skewer
1,200
25
30,000
150
112.5
37.5
250
107.14
Plant Meat
1,200
25
30,000
150
112.5
37.5
250
107.14
Roast Grubfruit Nut
1,200
25
30,000
150
112.5
37.5
250
107.14
Toasted Mimillet
1,500
24
36,000
180
135
45
300
128.57
Bristle Berry
1,600
23.68
37,894.74
189.47
142.11
47.37
315.79
135.34
Cooked Seafood
1,600
23.68
37,894.74
189.47
142.11
47.37
315.79
135.34
Meat
1,600
23.68
37,894.74
189.47
142.11
47.37
315.79
135.34
Raw Egg
1,600
23.68
37,894.74
189.47
142.11
47.37
315.79
135.34
Liceloaf
1,700
23.38
39,740.26
198.7
149.03
49.68
331.17
141.93
Pickled Meal
1,800
23.08
41,538.46
207.69
155.77
51.92
346.15
148.35
Bog Jelly
1,840
22.96
42,244.9
211.22
158.42
52.81
352.04
150.87
Gristle Berry
2,000
22.5
45,000
225
168.75
56.25
375
160.71
Swampy Delights
2,240
21.84
48,932.04
244.66
183.5
61.17
407.77
174.76
Grubfruit Preserve
2,400
21.43
51,428.57
257.14
192.86
64.29
428.57
183.67
Mushroom
2,400
21.43
51,428.57
257.14
192.86
64.29
428.57
183.67
Swamp Chard Heart
2,400
21.43
51,428.57
257.14
192.86
64.29
428.57
183.67
Pemmican
2,600
20.93
54,418.6
272.09
204.07
68.02
453.49
194.35
Fried Mushroom
2,800
20.45
57,272.73
286.36
214.77
71.59
477.27
204.55
Omelette
2,800
20.45
57,272.73
286.36
214.77
71.59
477.27
204.55
Smoked Fish
2,800
20.45
57,272.73
286.36
214.77
71.59
477.27
204.55
Veggie Poppers
2,862.5
20.31
58,138.22
290.69
218.02
72.67
484.49
207.64
Soufflé Pancakes
3,600
18.75
67,500
337.5
253.13
84.38
562.5
241.07
Tofu
3,600
18.75
67,500
337.5
253.13
84.38
562.5
241.07
Barbeque
4,000
18
72,000
360
270
90
600
257.14
Berry Sludge
4,000
18
72,000
360
270
90
600
257.14
Pepper Bread
4,000
18
72,000
360
270
90
600
257.14
Plume Squash
4,000
18
72,000
360
270
90
600
257.14
Spicy Tofu
4,000
18
72,000
360
270
90
600
257.14
Fish Taco
4,200
17.65
74,117.65
370.59
277.94
92.65
617.65
264.71
Mixed Berry Pie
4,200
17.65
74,117.65
370.59
277.94
92.65
617.65
264.71
Shellfish Tempura
4,200
17.65
74,117.65
370.59
277.94
92.65
617.65
264.71
Stuffed Berry
4,400
17.31
76,153.85
380.77
285.58
95.19
634.62
271.98
Mushroom Wrap
4,800
16.67
80,000
400
300
100
666.67
285.71
Curried Beans
5,000
16.36
81,818.18
409.09
306.82
102.27
681.82
292.21
Nosh Noms
5,000
16.36
81,818.18
409.09
306.82
102.27
681.82
292.21
Tender Brisket
5,000
16.36
81,818.18
409.09
306.82
102.27
681.82
292.21
Squash Fries
5,400
15.79
85,263.16
426.32
319.74
106.58
710.53
304.51
Frost Burger
6,000
15
90,000
450
337.5
112.5
750
321.43
Surf'n'Turf
6,000
15
90,000
450
337.5
112.5
750
321.43
Hexalent Fruit
6,400
14.52
92,903.23
464.52
348.39
116.13
774.19
331.8
Mushroom Quiche
6,400
14.52
92,903.23
464.52
348.39
116.13
774.19
331.8
The consumption/production rates are computed using the following formulas (where
d
is food density in kcal/kg):
k
c
a
l
c
o
n
s
u
m
e
d
p
e
r
c
y
c
l
e
=
1
0
0
0
⋅
6
0
0
1
0
0
0
0
.
0
5
d
+
5
1
.
5
=
1
8
0
0
0
0
⋅
d
/
6
0
0
0
1
+
d
/
6
0
0
0
f
o
o
d
c
o
n
s
u
m
e
d
(
k
g
/
c
y
c
l
e
)
=
1
0
0
0
⋅
6
0
0
d
1
0
0
0
0
.
0
5
d
+
5
1
.
5
=
3
0
⋅
1
1
+
d
/
6
0
0
0
s
a
p
p
r
o
d
u
c
e
d
(
k
g
/
c
y
c
l
e
)
=
5
⋅
6
0
0
1
0
0
0
0
.
0
5
d
+
5
1
.
5
=
9
0
0
⋅
d
/
6
0
0
0
1
+
d
/
6
0
0
0
Importing Food
Excess resources or by-products from other asteroids can be converted to food and shipped to the Marshy Asteroid via rocket or
Interplanetary Payload
. A single 200 kg payload can fully feed the tree for most of 7 cycles, assuming no food rots, making Experiment 52B a very efficient resource sink with low logistical overhead. Due to the high mass density of food relative to its ingredients, food should always be prepared before shipping.
Good candidates for Experiment 52B food are resources which have few other uses, and are produced as byproducts while refining more useful materials or on fixed schedules such as meteor showers or
Geyser
output. Particularly strong candidates for conversion include:
Carbon Dioxide
to
Meat
via
Slickster
ranching
Chlorine
to
Meat
via
Squeaky Puft
ranching
Sulfur
to
Grubfruit Preserve
and
Meat
via
Divergent
/
Grubfruit Plant
hybrid ranching/farming
Meteoric
Regolith
to
Meat
via
Shove Vole
ranching
Local Food Production
All the usual means of food production can be used on the Marshy Asteroid, though its limited local resources can pose difficulties.
Food can be produced with no resource inputs via
Pips
wild planting/ranching,
Drecko
and
Balm Lily
farms, or
Pacu
,
Shove Vole
, or
Sweetle
starvation ranches.
For raw wild plants,
Dusk Cap
are typically the best choice due to high calorie density of the uncooked food whilst still having amongst the highest calorie/cycle/plant.
Plume Squash
is by far the best wild plant option if available. Initial setup for the temperature requirements can be difficult however.
Tips
The output Sap is created at Experiment 52B internal temperature. Experiment 52B can be heated up to
99.9 °C
/
211.82 °F
to reduce the energy required to boil Sap afterwards.
Experiment 52B exchanges heat with the cell below its bottom middle cell (but no other cell around it). Placing a heat conductive tile here allows manipulating its temperature, especially if it's in vacuum.
If kept in a vacuum, deep-frozen food can be dropped on
Airflow Tiles
,
Mesh Tile
or
Insulated Tiles
without cooling Experiment 52B.
Sap is dropped from its first and last cell, but Experiment 52B can eat food on any cell beneath it. Keeping these cells non-solid prevents Sap from exchanging heat with deep-frozen food.
Experiment 52B will be marked as
Dead
if its body temperature exceeds
750 °C
/
1382 °F
.  Cooling Experiment 52B back down to below
750 °C
/
1382 °F
and reloading the game will remove the status effect and render it functional again.
Experiment 52B is listed as a Decor building, but does not count as one for rooms.
Experiment 52B does not count as a plant for
Pip
wild planting purposes.
Database
Experiment 52B
Plant?
Experiment 52B is an aggressive, yet sessile creature that produces 5 kilograms of sap per 1000 kcal it consumes.
Duplicants would do well to maintain a safe distance when delivering food to Experiment 52B.
While this creature may look like a tree, its taxonomy more closely resembles a giant land-based coral with cybernetic implants.
Although normally lab-grown creatures would be given a better name than Experiment 52B, in this particular case the experimenting scientists weren't sure that they were done.
History
U56-674504
: Resin renamed to
Sap
.
