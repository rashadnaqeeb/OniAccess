# Steam Turbine

Steam Turbine
Draws in
Steam
from the tiles directly below the machine's foundation and uses it to generate electrical
Power
.
Outputs Water.
Useful for converting geothermal energy and waste heat into usable power.
Research
Renewable Energy
Dimensions
5×3 tiles
Rotation
Mirrored
Category
Power
Power
0-850 W
Heat
+
4 kDTU/s
Overheat
at
1000 °C
/
1832 °F
Decor
0
Piping
Output: Water Output
Requires
Steam
: 2 kg/s @
125 °C
/
257 °F
+
Building temperature: below
100 °C
/
212 °F
Electrical Engineering
Effects
Water
: same mass as input Steam @
95 °C
/
203 °F
Auto In
Green
: Enable
Red
: Disable
Other Properties
Susceptible to
flooding
Materials
Refined Metal
800 kg
Plastics
200 kg
Steam Turbine
is the primary device in harvesting
Power
from
Heat
, usually
magma
or
Aquatuners
, and can do it exceptionally well provided you can keep the temperature of the
Steam
passing the turbine high.
Steam Turbines are also an incredibly effective heat deletion device capable of removing significant amounts of heat from the environment and turning the heat into power instead; this makes the Steam Turbine useful in many cooling devices.
It requires a
Duplicant
with Electrical Engineering
skill
to build.
Requirements
The
steam
below the generator must be at least
125 °C
/
257 °F
. If the steam below the generator becomes less than
125 °C
/
257 °F
the Steam Turbine will cease to produce any power.
Additionally, the steam turbine itself must be kept below
100 °C
/
212 °F
. If its temperature gets higher than
100 °C
/
212 °F
, the warning message "Turbine too hot" will appear, and the turbine will cease to operate.
Mechanics
To use the generator it must have hot
steam
below its base, a
water
output and a
power
connection. Note: not all of the inlets need to be uncovered for full functionality (see below).
While active, every non-blocked inlet will use 0.4 kg of Steam (for a max of 2 kg with 5 inlets) per second. It outputs Water with the same mass as input Steam at a fixed temperature of
95 °C
/
203 °F
.
Maximum Power Production
Power produced
P
=
(
8
5
2
1
)
⋅
m
˙
⋅
(
T
steam
−
9
5
)
W
Power to heat ratio
(
8
5
2
1
)
4
.
1
7
9
=
0
.
9
6
8
5
6
1
6
2
9
0
0
6
7
1
1
≈
0
.
9
6
9
W
kDTU/s
P
is the produced electrical power in W,
m
˙
is the mass flow rate in kg/s and
T
steam
is the temperature of the steam in Celsius.
The power output depends on the consumption rate and the temperature of the steam. Assuming the maximum steam consumption rate (2 kg/s), power output is 242 watts at
125 °C
/
257 °F
, capped at 850 watts at
200 °C
/
392 °F
or higher.
With a lower flow rate, a higher
temperature
is required for the same heat deletion and power production in accordance with the following table:
Inlets
Required Temp
for Max Power
Flow Rate
(kg/s)
Heat Deleted
(kDTU/s)
Power Produced
(W)
5
200 °C
/
392 °F
2
877.59
850
4
226.25 °C
/
439.25 °F
1.6
877.59
850
3
270 °C
/
518 °F
1.2
877.59
850
2
357.5 °C
/
675.5 °F
0.8
877.59
850
1*
357.5 °C
/
675.5 °F
0.4
438.8
425
Note for 1 inlet:
A steam turbine can absorb 0.08 kg steam in each tick (0.2 second) from each inlet. But a steam turbine needs more than 0.1 kg Steam to start conversion. As a result, when only one inlet is unblocked, the Turbine works only one tick every two ticks. So the average power generation is limited to 425 W, not 850 W. Therefore, the required temperature difference for max power is also reduced by half.
This does not affect the heat deletion and production. They still delete the heat of 0.4 kg Steam per second, and in turn, produce 10% of that + 4 kDTU themselves.
Thus, to summarize this section:
If the Steam is below
200 °C
/
392 °F
, have as many inputs unblocked as possible to maximize power generation through having the Turbine suck in as much as steam as possible.
If the Steam is above
200 °C
/
392 °F
, block the inputs to just two to maximize power generation while minimizing steam consumption and heat generation of the Turbine.
Heat Deletion
Heat Removed from Steam
q
removed
=
4
.
1
7
9
⋅
m
˙
⋅
(
T
steam
−
9
5
)
Heat Produced by Turbine
Q
˙
out
=
q
removed
1
0
+
4
As long as the steam turbine works (own temperature below
100 °C
/
212 °F
and steam temperature above
125 °C
/
257 °F
) it will reduce the input steam in temperature to
95 °C
/
203 °F
Water. How much heat is deleted
q
removed
in kDTU/s depends on the specific heat capacity of water (
4.179
DTU
g
∘
C
/
2.32
DTU
g
∘
F
), the flow rate
m
˙
in kg/s, and the steam temperature
T
steam
in Celsius.
The heat produced by the steam turbine
Q
˙
out
is 10% of the removed heat per second plus 4 kDTU/s operational cost.
Example with max pressure (2 kg/s) and 5 inlets
Temperature
Heat Deleted
(kDTU/s)
Heat Transfered to Turbine
(kDTU/s)
Real Heat Deleted
(kDTU/s)
Power Produced
(W)
125 °C
/
257 °F
250.74
25.07 + 4 = 29.07
221.67
242.85
137.5 °C
/
279.5 °F
355.21
35.52 + 4 = 39.52
315.69
344.04
150 °C
/
302 °F
459.69
45.97 + 4 = 49.97
409.7
445.23
200 °C
/
392 °F
877.59
87.76 + 4 = 91.76
785.83
850
300 °C
/
572 °F
1713.39
171.34 + 4 = 175.34
1538.05
850*
400 °C
/
752 °F
2549.19
254.92 + 4 = 258.92
2290.27
850*
500 °C
/
932 °F
3384.99
338.50 + 4 = 342.50
3042.49
850*
750 °C
/
1382 °F
5474.49
547.45 + 4 = 551.45
4923.04
850*
1000 °C
/
1832 °F
7563.99
756.40 + 4 = 760.40
6803.59
850*
Note
power produced is capped at 850W.
Usage
Self-Cooled Steam Turbine
A Steam Turbine can be used to cool itself using its own
95 °C
/
203 °F
exhaust water to maintain a temperature of less than
100 °C
/
212 °F
, this requires limiting the steam temperature to around
135 °C
/
275 °F
and power output to around 330 watts. Maximizing the heat transfer between the exhaust water and the Steam Turbine involves snaking radiant pipes behind the Steam Turbine. This works for any material radiant pipe (even lead) if the atmosphere is more than 500 g
Hydrogen
. Copper and gold radiant pipes even manage to cool the turbine in an +1500g
Oxygen
atmosphere. This can be made yet more effective by adding in one or more liquid layers.
A self-cooling Steam Turbine produces the most power per unit heat deleted of all setups, and they can be very attractive due to not needing an Aquatuner and the associated plumbing, circuit and automation, while being able to cool up to 292.53 kDTU/s without problems. The drawback to this is that, with their limited steam temperature, you will require more of them to match the heat deletion or power production of comparable ATST setups, and so they are best used when space and materials are less important than efficiency.
Maximum Steam Temperature for sustainable self-cooling
There is only limited cooling potential in the exhaust water: raising the temperature of 2000 g/s of water from
95 °C
/
203 °F
to
100 °C
/
212 °F
requires 41.79 kDTU/s, substituting this into the equation *Heat produced by turbine* and solving for steam temperature results in a value of
140.2 °C
/
284.36 °F
, this is where the heat produced by the Turbine exactly equals the available cooling in the water. The Steam Turbine would generate 365 watts.
While theoretically a self-cooled Steam Turbine could run on
140 °C
/
284 °F
steam this is an unstable equilibrium, if the Turbine gets too hot the flow of exhaust water halts and it is unable to cool itself, typically stalling until the player intervenes. Furthermore heat exchange between the exhaust water and Steam Turbine is imperfect and there may be heat bleed between the Steam chamber and the Steam Turbine. Due to these factors a practical self-cooled Steam Turbine typically runs on steam temperatures not exceeding
135 °C
/
275 °F
and generates around 330 watts.
Variable number of inlets at high temperatures
To remove as much heat as possible, the steam turbine should not be obstructed, but for power efficiency, as much heat (kDTU) should be turned into usable energy (J), or rather, no heat should be wasted.
When the temperature of the steam rises too high multiple ports can be automatically blocked by doors one at a time. Above
200 °C
/
392 °F
the first door closes, above
226.25 °C
/
439.25 °F
the second, and above
270 °C
/
518 °F
the third. The fourth door however should not close at
357.5 °C
/
675.5 °F
, but rather at
444 °C
/
831.2 °F
.
The temperature at which two inlets have the 850 W cap is
357.5 °C
/
675.5 °F
, any hotter and all DTU above is wasted, a gradual increase.
Taking only one inlet immediately cuts the used DTU's in half, but for every degree the steam gets hotter it also only wastes half of the DTU until it hits
620 °C
/
1148 °F
where it has its own cap of 425 W.
To gain 850 W, only 877.59 kDTU are needed (With two inlets this is at
357.5 °C
/
675.5 °F
), everything in excess is wasted. And half of the DTU deleted with one inlet are wasted, therefore we get following table:
Temperature
1 Inlet
2 Inlets
Heat deleted
Power produced
Heat wasted
Heat deleted
Power produced
Heat wasted
357 °C
/
674.6 °F
437.96 kDTU
212.19 W
218.98 kDTU
875.92 kDTU
848.76 W
0.00 kDTU
358 °C
/
676.4 °F
439.63 kDTU
213.00 W
219.82 kDTU
879.26 kDTU
850.00 W
2.07 kDTU
359 °C
/
678.2 °F
441.30 kDTU
213.81 W
220.65 kDTU
882.60 kDTU
850.00 W
5.41 kDTU
443 °C
/
829.4 °F
581.72 kDTU
281.84 W
290.86 kDTU
1163.43 kDTU
850.00 W
286.24 kDTU
444 °C
/
831.2 °F
583.39 kDTU
282.65 W
291.69 kDTU
1166.78 kDTU
850.00 W
289.58 kDTU
445 °C
/
833 °F
585.06 kDTU
283.46 W
292.53 kDTU
1170.12 kDTU
850.00 W
292.93 kDTU
446 °C
/
834.8 °F
586.73 kDTU
284.27 W
293.37 kDTU
1173.46 kDTU
850.00 W
296.27 kDTU
498 °C
/
928.4 °F
673.65 kDTU
326.39 W
336.83 kDTU
1347.31 kDTU
850.00 W
470.12 kDTU
499 °C
/
930.2 °F
675.33 kDTU
327.20 W
337.66 kDTU
1350.65 kDTU
850.00 W
473.46 kDTU
500 °C
/
932 °F
677.00 kDTU
328.01 W
338.50 kDTU
1354.00 kDTU
850.00 W
476.80 kDTU
The exact tipping point is
444.84 °C
/
832.71 °F
.
Synergy with Thermo Aquatuners
Using Water or Polluted Water as Coolant
At maximum operational power without wasted heat, a Steam Turbine can delete 877,590 DTU/s (785,830 DTU/s real deletion), and a
Thermo Aquatuner
using
Water
or
Polluted Water
as coolant introduces 585,060 DTU/s. This means that the ratio of Steam Turbines needed per Thermo Aquatuner is precisely 2:3 when using Water or Polluted Water as the coolant, and a system built with such a ratio would stabilize at
200 °C
/
392 °F
, assuming ideal conditions. In practice, minor loss of heat can be expected, resulting in slightly less power than anticipated.
Watts of power used per Thermo Aquatuner:
1
2
0
0
−
(
5
8
5
,
0
6
0
8
7
7
,
5
9
0
⋅
8
5
0
)
=
6
3
3
.
3
‾
Heat (DTU/s) deleted per spent watt:
5
8
5
,
0
6
0
6
3
3
.
3
‾
≈
9
2
3
.
7
8
Using Super Coolant as Coolant
When upgrading to
Super Coolant
, a Thermo Aquatuner's performance improves to introducing 1,181,600 DTU/s at the same cost of power. When comparing this to the Steam Turbine's heat deletion capacity, this results in an awkward ratio of Steam Turbines needed per Thermo Aquatuner of approximately 1:1.35. So long as the actual ratio built is higher than this, all heat can be processed without any losses. Three Steam Turbines to two Thermo Aquatuners is a reasonably good approximation of the ratio.
Watts of power used per Thermo Aquatuner:
1
2
0
0
−
(
1
,
1
8
1
,
6
0
0
8
7
7
,
5
9
0
⋅
8
5
0
)
≈
5
5
.
5
4
7
Heat (DTU/s) deleted per spent watt:
1
,
1
8
1
,
6
0
0
5
5
.
5
4
7
≈
2
1
,
2
7
2
Comparing Power Efficiency of Coolants
Using Super Coolant is approximately 23 times more efficient than using Water or Polluted Water as coolant in a Steam Turbine/Thermo Aquatuner combo:
(
2
1
,
2
7
2
9
2
3
.
7
8
≈
2
3
)
Trivia
Reintroducing exhaust water
Will cooling the steam with output water give less energy? No, steam turbines yield their power proportional to the amount of heat they delete.
Experiment:
First, let's assume we have 10 kg Steam at
200 °C
/
392 °F
, for 5 seconds it will provide 850 W → a total of 8,358 MDTU deleted for 4250 J.
Then, let's assume the same 10 kg of Steam at
200 °C
/
392 °F
but add the
95 °C
/
203 °F
Water back in instantaneously and lets assume the Steam Turbine could suck up water
95 °C
/
203 °F
hot. For the first second we get 850 W. The second second the steam will be
179 °C
/
354.2 °F
for 680 W, the third second
162.2 °C
/
323.96 °F
for 544 W. On an infinite timescale the water will have reached
95 °C
/
203 °F
a total of 8,358 MDTU deleted for 4250 J.
Of course a steam turbine will stop at
125 °C
/
257 °F
, but the heat will remain to be harnessed later.
As a matter of fact the opposite holds true since the more important limit is the
200 °C
/
392 °F
maximum temperature, or rather the 850 W power cap. If we run the same thought experiment as before, 10 kg of Steam at
300 °C
/
572 °F
will still run for 5 seconds at 850 W and produce 4250 J of energy... however, the cooled Steam can run for 9 seconds at ever decreasing power (see table below), producing about 5684.69 J while doing so.
seconds passed
Steam
temperature
Temperature
difference
Power
produced
0
300 °C
/
572 °F
205.00
850.00 W
1
259 °C
/
498.2 °F
164.00
850.00 W
2
226.2 °C
/
439.16 °F
131.20
850.00 W
3
199.96 °C
/
391.93 °F
104.96
849.68 W
4
178.97 °C
/
354.15 °F
83.97
679.74 W
5
162.17 °C
/
323.91 °F
67.17
543.79 W
6
148.74 °C
/
299.73 °F
53.74
435.03 W
7
137.99 °C
/
280.38 °F
42.99
348.03 W
8
129.39 °C
/
264.9 °F
34.39
278.42 W
9
122.51 °C
/
252.52 °F
temp too low
Steam from multiple rooms
For the steam turbine to function only 1 port requires
125 °C
/
257 °F
. Therefore it is possible to create setup where a steam turbine accepts steam from 2 steam rooms where the temperature of one steam room is at
100 °C
/
212 °F
and the other above
125 °C
/
257 °F
. This effectively allows you to cool a room to
100 °C
/
212 °F
.
Cool and Hot Steam Vents
This means with one steam room above
125 °C
/
257 °F
, the steam from a
Cool Steam Vent
can be sucked up.
It also means, it can take in steam that is much hotter. One port above
500 °C
/
932 °F
Steam (0.4 kg/s) from a
Steam Vent
on one side, and three ports above
135 °C
/
275 °F
Steam (1.2 kg/s) are effectively 1.6 kg/s of
226.25 °C
/
439.25 °F
Steam and produce 850 W.
Other tips
Like most generators it will continue to run, consuming its power source, unless turned off manually or by an
automation
connection.
It can often be useful to allow the Steam Turbine to simply waste excess power, if it is primarily being used to delete heat.
Steam Turbines cannot add to the packets from their output port, so each one should have a dedicated uncloggable pipe segment.
A metal refinery with a coolant of petroleum, crude oil, or super coolant combined with a steam turbine actually generates excess power when refining iron or steel due to the large amounts of heat generated by the refinery. This is further improved with the operating duplicant's
Machinery
attribute, since the refinery draws its 1200 W of power for less than the full 40 s/batch.
An Aquatuner cooling Super Coolant will run energy neutral when combined with a steam turbine that has engie's tune-up. Even better, a single aquatuner/turbine setup with all 5 input ports open will not reach thermal equilibrium until ~
236 °C
/
456.8 °F
steam temperature, meaning the setup requires no automation. With dupe's time for engie's tune-up and buffered by a power grid, this setup can provide energy-free cooling. Care must be taken not to deplete the heat in the cooling target, otherwise the setup will start drawing power.
Steam Turbines exchange heat with their foundation tiles, and as such
can be cooled with a heat-conductive foundation tile
that's not in contact with Steam. Considering how high those tiles'
Thermal Conductivity
is, one tile can be enough to cool a Turbine when using active cooling.
Bugs
For the purpose of heat transference calculation to the steam turbine during operation (10% + 4 kDTU/s), the game ignores the mass of 200 kg of plastics. In effect the game displays the mass as 1000 kg but calculation itself is done only for the 800 kg of the metallic component of the mass.
Blueprints
Available blueprints
Regal Neutronium Steam Turbine
History
OC-252151
: Introduced.
OC-252656
: Steam turbine now works on a pressure differential and no longer requires water cooling.
QLM3-326232
: Steam turbine reworked.
RP-379337
: Steam turbines correctly obtain boost from Engie's Tune-up.
U47-561558
: Steam Turbine shows obstructed tiles for steam intake before placing it.
Reference
Navigation
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
Battery
Coal Generator
Compact Discharger
Conductive Wire
Conductive Wire Bridge
Dev Generator
Heavi-Watt Conductive Joint Plate
Heavi-Watt Conductive Wire
Heavi-Watt Joint Plate
Heavi-Watt Wire
Hydrogen Generator
Jumbo Battery
Large Discharger
Large Power Transformer
Manual Generator
Natural Gas Generator
Peat Burner
Petroleum Generator
Power Bank Charger
Power Shutoff
Power Transformer
Smart Battery
Solar Panel
Steam Turbine
Switch
Wire
Wire Bridge
Wood Burner
see all buildings
