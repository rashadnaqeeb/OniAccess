# Guide/Water Production

Water
is one of the most valuable resources in Oxygen Not Included. This guide will explain how to make your own water when you run out.
Water-based Resources and Conversion
Water
is the most generally useful, and rarest in the general environment. It is usually found in the starting biome near the
Printing Pod
, and occasionally in small pockets or
Ruins
. Most water will have to be produced from other water-based resources.
Polluted Water
is produced by a variety of life support and industrial processes, as well as being found in a variety of biomes. In addition to water production, various agricultural processes use polluted water directly, making maintaining an additional stockpile can be useful - though care should be taken as in low-pressure environments it evaporates into
Polluted Oxygen
. It has the highest boiling point among water-based resources, and thus is often used as a coolant.
Salt Water
is found in some biomes, and cannot be produced by duplicants - the only renewable sources are space missions and the
Salt Water Geyser
. It can be used to grow domestic
Waterweed
, converted into water and salt by boiling or a
Desalinator
, or cooled to produce ice and brine.
Brine
is found in cooler salt biomes, and can be produced by cooling salt water. It has the lowest freezing point of the water-based resources, so it can see some use as a coolant, but its lower specific heat means polluted water is generally more useful unless the low-temperature performance is required. Other than as coolant, its only use is to be converted into salt and water.
Ice
,
Snow
,
Crushed Ice
,
Polluted Ice
, and
Brine Ice
all melt to their corresponding water type; an
Ice-E Fan
can be used to accelerate this process, transferring 32kDTU/s (roughly equivalent to the output 1.8
Space Heaters
) when operated in a warm environment.
Steam
is the result of boiling any water-based resource. Water converts 100% of its mass to steam, polluted water converts 99%, salt water converts 93%, and brine converts 70% - for brine and salt water this is the same ratio as using a
Desalinator
, but some water is lost when boiling polluted water compared to a
Water Sieve
. Steam will always condense to pure water, either by lowering its temperature or by feeding higher-temperature steam (above
125 °C
/
257 °F
) into a
Steam Turbine
, which will process 2kg/s of steam into
95 °C
/
203 °F
water as well as generate power.
Geyser Taming
Seven
Geyser
variants produce water or related resources on a regular schedule. In roughly increasing order of complexity:
A
Water Geyser
produces clean water at
95 °C
/
203 °F
. A
Gold Amalgam
Liquid Pump
(or possibly two, to avoid overpressurising) suffices to add it to your water system immediately.
A
Salt Water Geyser
produces salt water at
95 °C
/
203 °F
, which can be passed through a desalinator and into the network. (It can also be boiled, which is more energy-intensive but does not require duplicant maintenance.)
This is the only renewable source of salt water without rocketry, so it should prioritise any
Waterweed
farms before a
Desalinator
.
A
Polluted Water Vent
produces polluted water at
30 °C
/
86 °F
, infected by
Food Poisoning
germs. Germs are not destroyed by a
Water Sieve
, so duplicants can still be infected if they consume the water or food grown from it. See
Guide/Disinfect Germs
for an effective way to eliminate the germs before use.
A
Cool Salt Slush Geyser
produces brine at
-10 °C
/
14 °F
. This is too cold to run directly through a
Desalinator
without breaking the pipe, so it should be run through a heat exchanger or used to cool other parts of the colony before being moved into the water system.
A
Cool Slush Geyser
produces polluted water at
-10 °C
/
14 °F
. Many of the isssues of the salt slush geyser apply here, but polluted water has a higher specific heat than brine - making it a more effective coolant but needing more heat to reach safe temperatures.
A
Cool Steam Vent
produces steam at
110 °C
/
230 °F
. As this is too low-temperature for a
Steam Turbine
, it needs to either be heated (to
125 °C
/
257 °F
for turbine input) or cooled (to its condensation point for a liquid pump) to produce usable water. Cooling the steam requires slightly less heat transfer than heating it (-52,864 DTU/kg vs 62,685 DTU/kg), but heating the steam allows the system to serve as a heat sink for other machines.
A
Steam Vent
produces steam at
500 °C
/
932 °F
. The simplest way to extract water is to directly feed this to a turbine (a
Thermo Aquatuner
will still be needed to prevent it from overheating - due to the high temperatures, it must be placed outside the geyser room unless
Thermium
is used). More complex builds will cool the steam to
200 °C
/
392 °F
using heat exchangers which feed additional turbines, dramatically improving
Power
production.
Lavatory Loop
Things you'll need:
Lavatory
,
Water Sieve
,
Filtration Medium
Each use of a
Lavatory
will produce 11.7 kg of Polluted Water from 5 kg of Water, which essentially means each Duplicant will produce a net gain of 6.7 kg/cycle (or 13.4 kg/cycle with the Small Bladder trait) if lavatory output is run through a
Water Sieve
.
This water will contain food poisoning germs, which makes them unsuitable for direct consumption; see
Guide/Disinfect Germs
for eliminating germs before introducing lavatory water to the primary water supply. In order to stop lavatories from backing up with produced polluted water, it may be desirable to connect an overflow bridge to a
Thimble Reed
farm.
Pufts (Polluted Oxygen input)
Things you'll need:
Puft
,
Algae Distiller
,
Polluted Oxygen
source.
The puft will inhale polluted oxygen and defecate slime. The slime can be placed in an algae distiller and is turned into algae and polluted water, which can be used or filtered as needed.
Petroleum and Natural Gas Generators
Things you'll need:
Petroleum Generator
or
Natural Gas Generator
;
Crude Oil
and
Oil Refinery
or
Ethanol
or
Natural Gas
Burning hydrocarbon fuels (
Petroleum
,
Ethanol
, and
Natural Gas
) will produce polluted water in the surrounding environment at the temperature of the generator.
Note that
renewable
sources of these materials (
Oil Well
+
Oil Refinery
or
Arbor Trees
) will on their own result in a net
loss
of water even after fuel is burned. When environmental sources of crude oil and ethanol have been exhausted, additional systems are needed to obtain water renewably - see the zero-inputs setups below.
Arbor Tree Pip Farming (zero-inputs)
Things you'll need:
Pip
,
Arbor Tree
,
Ethanol Distiller
,
Petroleum Generator
Wild-growth Arbor Trees harvested by duplicants produce ~83.3kg/cycle of
Wood
at no resource cost. An Ethanol Distiller converts 50% of wood mass to ethanol, and a Petroleum Generator converts 67.5% ethanol mass to polluted water - each wild tree thus essentially produces ~26kg/cycle of germ-free polluted water at no cost, in addition to a small amount of power,
Carbon Dioxide
, and
Polluted Dirt
.
Pips
can be used to plant wild trees at any location. Deconstructing a
Manual Airlock
which is surrounded by tiles will produce natural tiles from the door's ore, which are suitable for pip planting.
Petroleum Boiling (zero-inputs)
Things you'll need: High-temperature heat source (
Volcano
,
Minor Volcano
,
Steam Vent
);
Oil Well
; large amounts of
Steel
Heating
Crude Oil
to its turning point will convert 100% of its mass into
Petroleum
, compared to only 50% conversion from an
Oil Refinery
- this is enough to make
Petroleum Generators
and
Oil Wells
water-positive. See
Guide/Petroleum Boiler
for details; once constructed, this will produce a net ~3.75kg/s of polluted water and ~10kW of power without duplicant labour or external input.
A
Thermium
Thermo Aquatuner
can also be used as a heat source, but with access to space materials it's more effective to produce
Sour Gas
(see below).
Pufts and Arbor Trees (Spaced Out! zero-inputs)
Things you'll need:
Puft
,
Algae Distiller
,
Arbor Tree
,
Ethanol Distiller
,
Petroleum Generator
,
Sublimation Station
,
Compost
The
Sublimation Station
's rapid conversion of
Polluted Dirt
allows for a combination of the puft cycle with ethanol generators which is completely resource-positive, requiring no external inputs other than duplicant labour.
A stable of six pufts will consume 500g/s of polluted oxygen (with a further 50g/s from a single puft prince to keep it stocked with eggs); this will in turn produce 480g/s of slime, which distills to 320g/s of polluted water - or 192 kg/cycle.
Eight Arbor Trees consume 560kg/cycle of polluted water and 80kg/cycle of dirt to produce 83.(3) kg/cycle of harvested
Wood
. This is enough to run (slightly more than) four ethanol distillers for 2kg/s of ethanol and 1.3kg/s of polluted dirt. Consuming 1 kg/s will fully run the
Sublimation Station
and supply the pufts with some polluted oxygen to spare; the remaining .3kg/s can be composted to supply 180 kg/cycle, more than enough dirt for trees. Burning the ethanol will produce 750g/s of polluted water, or 450 kg/cycle - added to the output from the algae distiller, this produces 642 kg/cycle for a net gain of 82 kg/cycle polluted water.
This loop also produces excess wood, dirt, algae, and carbon dioxide, as well as a net power gain of ~750W from the generators after powering all the other machines. Adding a fifth distiller to process the excess wood will increase the net power production to about 1000W and the net polluted water to 132 kg/cycle, and the resulting total carbon dioxide production is enough to feed about 40 slicksters, providing food and
Crude Oil
or
Petroleum
.
Sour Gas Boiling (space material zero-inputs)
Sour gas boiling is an upgrade to a petroleum boiler which needs late-game materials and significantly increases the net water and power production. The basic loop of feeding water back into a
Oil Well
is the same, but the processing apparatus is more complicated.
Crude Oil
is boiled into
Petroleum
and then
Sour Gas
, which is then cooled to yield
Sulfur
and
Methane
, the latter of which is heated up again to yield usable
Natural Gas
.
67% of sour gas converts to methane, while 75% of natural gas becomes polluted water; taken together, 50% of crude oil mass becomes water, compared to the 37.5% conversion via a petroleum boiler. Further, by exploiting rounding in phase transitions (boiling less than 100g of
Polluted Water
will not produce
Dirt
), a natural gas generator heated above polluted water's boiling point will essentially produce clean water directly without a water sieve or mass lost as
Dirt
.
It also produces 33% of input mass as
Sulfur
, which has no use in the base game but can be processed further in Spaced Out!.
This process needs enormous amounts of heat transfer at extreme temperatures, essentially requiring the use of
Super Coolant
and
Thermium
. It also produces considerably less carbon dioxide than petroleum boilers, which can be an issue if relying on
Slickster
ranching for food.
Sulfur Processing (Spaced Out!)
What you'll need:
Divergent
ranches;
Sludge Press
;
Sulfur
source
Sweetles consume sulfur and excrete 50% of its mass as
Sucrose
. Grubgrubs fed
Sucrose
will produce 100% of its mass as
Mud
(they can also be fed sulfur directly, but only excrete 10% mass - feeding it to sweetles first is thus 5x more efficient). This mud can then be run through a sludge press which will convert 60% of its mass to water and the rest to dirt. The final conversion rate of sulfur to water is thus 30% of sulfur mass.
Sulfur can be renewably obtained from a sour gas boiler or
Sulfur Geyser
. When combined with a sour gas boiler, this increases the oil to water ratio from 50% to ~60% mass conversion.
