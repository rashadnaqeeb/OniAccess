# Guide/Geyser and Vent Taming

Since
Geysers
are a renewable sources of
resources
, and putting a structure into place to harness these resources effectively is considered
taming
the geyser.
Some Geysers are more worth taming than others, and some are considerably harder to tame than others yet again.
The main goal of any Geyser Tamer is to get as many resources as possible, converting them to a usable form (or temperature), while using as little
Steel
or Space Age Materials, such as
Niobium
,
Thermium
, or
Insulation
, and as little
Power
, as possible.
Hydrogen Vent
Hydrogen Vent
Average Output
Temperature
P
max
70g - 140g
500 °C
/
932 °F
5 kg
Hydrogen Vent Tamer
Hydrogen Vents provide
Hydrogen
and
Heat
, both primarily used for
Power Generation
, and fortunately, it is easy to tame the Vent.
The first step to take, as with any Geyser, is to ensure that it does not reach its P
MAX
. That's point at which it over-pressurizes, and no longer yields any more Hydrogen.
An unpowered
Guide/Door Pump
can easily transport the Hydrogen Gas away from the Vent and into a
pressure chamber
in which the Hydrogen can collect.
Next, or simultaneously, the Hydrogen needs to reach a cold enough point at which it can be handled with Steel Equipment. If both the Vent and pressure chamber are connected to a steam room, which in turn is cooled by a
Steam Turbine
, the Hydrogen's Temperature will easily drop down to
125 °C
/
257 °F
.
Even if the Hydrogen Vent yields ~700g/s while active, this is only (700x2.4x375) 630kDTU/s, and approximately a third to half of that averaged out over its active period.
A Steam Turbine deletes about 250kDTU/s when working with
125 °C
/
257 °F
Steam, and 334kDTU/s when working with
135 °C
/
275 °F
(the approximate Temperature at which Steam Turbines can still cool themselves down). Since enough space is given, two Steam Turbines will easily manage the heat off the Vent.
Now the Hydrogen Gas needs to be distributed to
Hydrogen Generators
. To cool the Hydrogen down one last time one may want to snake it through the steam chamber before exiting to a
Power Plant
. Since Gold Amalgam Hydrogen Generators and
Power Transformers
, and Gold
Smart Batteries
have a maximum operating Temperature of
125 °C
/
257 °F
, we still need to cool the environment down, if ever so slightly, to... say
115 °C
/
239 °F
.
200g of Hydrogen from
125 °C
/
257 °F
to
115 °C
/
239 °F
hold 4.8kDTU, 2 Hydrogen Generators generate 4kDTU each, 4 Smart Batteries generate 500DTU each, and the Power Transformer generates 1kDTU; for a total of 15.8kDTU that need to be removed. This can easily be moved by a
Thermo Regulator
which can be placed in the Steam Room with which we cool the Hydrogen initially.
For larger Hydrogen Generator Arrays, a
Thermo Aquatuner
is recommended as it can move more heat per second and moves more DTU per Watt used.
Metal Volcanos
Copper Volcano
Average Output
Temperature
P
max
200g - 400g
2226.85 °C
/
4040.33 °F
150kg
Gold Volcano
Average Output
Temperature
P
max
200g - 400g
2626.85 °C
/
4760.33 °F
150kg
Iron Volcano
Average Output
Temperature
P
max
200g - 400g
2626.85 °C
/
4760.33 °F
150kg
Copper and Gold comes out HOT,
2226.85 °C
/
4040.33 °F
and
2626.85 °C
/
4760.33 °F
respectively, and they need to be cooled down to
275 °C
/
527 °F
to not destroy our Steel Equipment.
Material
<span title="Temperature difference to
275 °C
/
527 °F
or to SHC change" style="text-decoration: underline;text-decoration-style:dotted" class="help" tabindex="0">DeltaT
SHC
DTU per gram
Liquid Gold
2351.85 K
0.129
303.39DTU/g
Liquid Copper
1145.00 K
0.386
752.61DTU/g
806.85 K
0.385
Liquid Iron
1145.00 K
0.449
1055.98DTU/g
If cooled by its own output, the Maximum Cooling we can allow a
Steam Turbine
to do, is to cool down
135 °C
/
275 °F
Steam to
95 °C
/
203 °F
Water, or 37.43kDTU/s. If we subtract a tenth of which and its own heat production of 4kDTU, its net heat deletion is 296,888 DTU/s.
With this we can calculate how much a Metal Volcano is allowed to yield.
Heat Deletion
Gold
Copper
Iron
1 Steam Turbine
296.888 DTU/s
979 g/s
394 g/s
281 g/s
2 Steam Turbine
593.776 DTU/s
1957 g/s
788 g/s
562 g/s
3 Steam Turbine
890.664 DTU/s
2935 g/s
1183 g/s
843 g/s
4 Steam Turbine
1187.552 DTU/s
3914 g/s
1577 g/s
1124 g/s
However, the average yield cannot be taken, since the heat comes in spikes, therefore we need to use the average
during its Eruption Cycle, not over its lifetime
Example: 9.9kg/s for 53s over 913s are 574.7g/s (Average yield over eruption cycle range from about 400g/s to 700g/s (needs confirmation))
However, this still may not hold true if a Volcano yields 17.5kg/s for 20 seconds per 900seconds (583.33g/s), since heat that is this rapidly introduced.
With the facts above combined it is not feasible to have a one-size-fits-all-solution, rather two tiers with optional cooling each. Gold and Copper Volcanoes fall into Tier 1 indiscriminately, and Iron Volcanoes are split up. Those Iron Volcanoes with average yield above 500g/s (over active period) or 150g/s (over lifetime) fall into Tier 2. In addition, those Volcanoes with yield above 10kg/s (in activity) should be equipped with optional cooling. Cooling that should only be active if the Steam directly underneath the Steam Turbines rises above
135 °C
/
275 °F
, AND the cooling loop rises above
95 °C
/
203 °F
.
Tier 1
Blueprint
Tier 1 with cooling
Blueprint
Tier 2
Blueprint
Tier 2 with cooling
Blueprint
Natural Gas Vent
Natural Gas
's only uses are to create
Power
, and supply the
gas range
. The first few steps are very similar to the first steps for the Hydrogen Vent (see above). Since the Natural Gas comes out at
150 °C
/
302 °F
, one can reason that it's almost not worth initially cooling it down to
125 °C
/
257 °F
, but the Steam room will be needed to cool the Natural Gas Generator setup. Cooling 70 to 140 g of Natural Gas by
25 °C
/
77 °F
, with Natural Gas' considerable SHC of 2.191, is 3.8 kDTU to 7.7 kDTU that does not need to be cooled later (equating to roughly 7.86 W to 15.73 W)
As opposed to Hydrogen Vent Taming, the Natural Gas Generators have waste products in the form of CO2 and Polluted Water.
The CO2 can be Vented out into space, or be put into its own pressure chamber where it can be forgotten about without a pipe running through the entire base.
Tidy people can also run them through a
Carbon Skimmer
, which would double the amount of sand needed when Sieved, add 4.06W to 8.12W, and 68.83 DTU to 137.67 DTU.
Either way 70g to 140g Natural Gas yield 622.22W to 1244.44W, also yield 58.33g to 116.67g clean water. Electrolyzed, this yields 6.53 to 13.07 g Hydrogen for an additional 52,27W to 104.53 W. Taking Gas Pumps in the ideal situation into account, this would yield 604.63 W to 1209.26 W, and adding 3 smart batteries and one power transformer produces 10,392.69 DTU to 18,285.39 DTU. (or, for those who skimmed their CO2, 600.57 W to 121.14 W and 10,461 DTU to 18423.06 DTU)
Cooling should go to just below Water's boiling point, as such most heat mass will be removed when going into an electrolyzer, but some buffer should exist. As operating Temperature we should use
95 °C
/
203 °F
. Our two inputs are the 70g to 140g Natural Gas and from
125 °C
/
257 °F
for 4601.1 DTU to 9202.2 DTU, and 10.5 g to 21 g Regolith from
125 °C
/
257 °F
(also lead through the Steam room first) for 63 DTU to 126 DTU. (or, for those who skimmed their CO2, 22.17g to 44.33g for 133 DTU to 266 DTU)
In total these are 15.06kDTU to 27.61kDTU, or if carbon skimmed, 15.2kDTU to 27.89kDTU.
For a larger scale operation that produces power on demand with 11 Natural Gas Generators running at once, with 11 Smart Batteries, Heat production can spike up to 184.09kDTU (or 186.05 kDTU if skimmed). Fortunately, only a single piece of additional  equipment would need to be installed to keep up with the emergence of more by-products, a second Gas Pump for the Oxygen produced.
Blueprint
Steam Vent Taming
Preface: Why?
Let’s pose the stupid question first: can we not just slap a Steam Turbine on it? Is that not what they were literally made for? There are two main reasons.
First: Steam Turbines produce the most amount of power at a Steam Temperature of
200 °C
/
392 °F
, that means the same amount of power is produced if the Steam is
500 °C
/
932 °F
. That is 300 Degrees of wasted potential.
Second, and more importantly: Steam Turbines produce 4,000 DTU/s plus 10% of the heat they destroyed from the water. For
500 °C
/
932 °F
Steam to
95 °C
/
203 °F
Water (SHC: 4.179) at a rate of 2kg/s, that’s an astonishing 3,384,990 DTU/s, or what ~5.8 Thermo Aquatuners primed with Polluted Water can handle and which cost ~6960 W to cool, only to gain 850 W and 2kg of
95 °C
/
203 °F
Water per second.
But this is still not the only problem with Steam Vents. Since they over-pressurize at 5kg, and some very much able to spew forth more than 5kg/s the Steam needs to be removed as fast as possible to make the most of what it provides. Water and Power.
Useful Trivia
One may think that reintroducing water into the steam chamber would cool the steam down and therefore yield less power, but steam turbines produce power proportional to the amount of heat they delete according to following formula that depends on the difference between the steam temperature and the turbine's water output temperature (95 °C):
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
(with steam temperature in degrees Celsius).
For example, assume we have 10 kg steam at
200 °C
/
392 °F
. For 5 seconds, it will provide
850 W
→ a total of 8358000 kDTU deleted for 4250 J.
Then, let's compare that with the same 10 kg of steam at
200 °C
/
392 °F
but add the
95 °C
/
203 °F
water back in instantaneously, and assume that the steam turbine could suck up water at
95 °C
/
203 °F
. During the first second, we get
850 W
. During the second second, the steam will be at
179 °C
/
354.2 °F
for
680 W
, during the third second
162.2 °C
/
323.96 °F
for
544 W
, and so on. As time passes, the water approaches
95 °C
/
203 °F
, and a total of 8358000 kDTU is deleted for 4250 J electricity.
Of course, a steam turbine will stop at
125 °C
/
257 °F
, but the DTUs remain to be harnessed later.
As a matter of fact the opposite holds true since the more important limit is the
200 °C
/
392 °F
maximum Temperature, or rather the
850 W
power cap.
If we run the same thought experiment as before, 10kg of steam at
300 °C
/
572 °F
will still run for 5 seconds at
850 W
and produce 4250 J of power... however, the cooled steam can run the turbine for 9 seconds at ever decreasing power (see table below), but produce 5684.69 koule while doing so.
Steam
temp
temp
difference
Power
produced
300 °C
/
572 °F
205,00
850,00 W
259 °C
/
498.2 °F
164,00
850,00 W
226.2 °C
/
439.16 °F
131,20
850,00 W
199.96 °C
/
391.93 °F
104,96
849,68 W
178.97 °C
/
354.15 °F
83,97
679,74 W
162.17 °C
/
323.91 °F
67,17
543,79 W
148.74 °C
/
299.73 °F
53,74
435,03 W
137.99 °C
/
280.38 °F
42,99
348,03 W
129.39 °C
/
264.9 °F
34,39
278,42 W
122.51 °C
/
252.52 °F
temp too low
Therefore, we should always try to reintroduce water to keep the steam temperature between
125 °C
/
257 °F
and
200 °C
/
392 °F
.
The Design
under construction
other Geysers
under construction
