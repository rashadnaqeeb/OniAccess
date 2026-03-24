# Guide/Base Cooling

Base cooling is a pressing issue at all times. Difficult to manage, temperature might be the biggest challenge you will be facing in Oxygen Not Included.
The main reasons for high temperatures are
geyser
water and machine heat, as you will need both through your game to keep your dupes alive.
Rule 0: Don't Heat Up In The First Place
The first rule of base cooling is to not heat your base in the first place. Heat-sensitive objects like
Duplicant
living quarters,
Plants
, and
Critter
ranches should be built far away from high-temperature machines like the
Glass Forge
or
Metal Refinery
. Avoid digging through
Abyssalite
veins blocking off high-temperature biomes from cooler regions.
Insulation
Insulated Tiles
can be used to slow heat transfer between two areas.
Mafic Rock
is the best early-game material for temperatures below its melting point, while
Igneous Rock
and
Obsidian
can handle higher temperatures at the cost of faster heat transfer.
Ceramic
and
Insulation
should be used in the mid- and late-game as available.
Vacuum
is the most effective insulator in the game - heat will not transfer over a vacuum at all. A "box base" surrounded by a layer of vacuum can be completely insulated from surrounding heat, meaning cooling is only necessary for handling heat produced inside. To preserve a vacuum along a high-traffic area in minimal space,
Liquid Airlocks
can be built with a shared
Airflow Tile
between them, which prevents heat moving from one airlock to the other as long as the rest of the chamber remains a vacuum.
Specific Heat Deletion
Most machines, including plants and critters, don't actually care about the temperature of their inputs - output temperatures, plants wilting, and critter death is instead determined by the temperature of the entity itself, which exchanges heat with its surroundings. This means that cooling materials which are about to be consumed is often highly counterproductive - in order to minimise base heating, it's generally better to have machine and plant inputs as hot as possible.
A
Sleet Wheat
plant, for instance, can survive just as well on
95 °C
/
203 °F
water as
5 °C
/
41 °F
water - the plant's body temperature is determined by the temperature of its surroundings, not the temperature of the water it consumes. Pre-cooling water before sending it to your farms would require moving
9
0
∘
C
×
2
0
kg
×
4
.
1
8
4
=
7
5
3
1
2
0
0
DTU
per plant per cycle; but if the
95 °C
/
203 °F
water is kept in an
Insulated Liquid Pipe
made of
Igneous Rock
, only
9
0
∘
C
×
0
.
0
3
1
2
5
×
1
.
0
0
0
×
5
0
×
6
0
0
=
8
4
3
7
5
DTU
are transferred from the liquid to the pipe (and then to the environment) each cycle. The rest of the heat is deleted as the water is consumed. In this simplified case, the same amount of cooling can support almost ninety times as many plants when applied to the environment rather than the liquid input; in practice, the change is not nearly this dramatic, as the small internal buffer of a
Hydroponic Farm
still causes a large amount of heat to leak into the environment - however, the smaller amounts involved still mean less heat needs to be moved compared to pre-cooling inputs.
Heat Pumps
The primary means of heat transfer is through the use of heat pumps - the
Thermo Regulator
and
Thermo Aquatuner
. Both machines reduce the temperature of an input packet by a fixed
14 °C
/
57.2 °F
, and transfer the corresponding amount of heat to the machine itself. The Regulator uses less power and processes smaller packets with generally lower SHC; while this is a downside for large-scale cooling, it also means that storing the output heat is easier - as is running it.
Starting Out: Heat Sinks and Radiators
Cooling the base early using less power, by sinking the heat to a random biome
The basic core of a heat pump-based cooling system is a heat sink and a radiator, connected by a system of pipes containing a working fluid (or coolant). The radiator exchanges heat with its surroundings, which is transferred through the working fluid to a heat pump and pushed into a heat sink. The working fluid, cooled by the heat pump, then returns to the radiator to exchange more heat, until some equilibrium is reached.
A
heat sink
is a structure designed to store heat, either for deletion or just to keep it away from the rest of the base. Heat sinks are usually built as a room of some medium, usually
Hydrogen
or
Water
/
Steam
but sometimes
Petroleum
, containing a
Thermo Aquatuner
or
Thermo Regulator
to "load" the sink, and surrounded by
Insulated Tiles
and/or a vacuum layer. Heat is transferred into the sink by the Aquatuner or Regulator, stored in the medium, and kept isolated by the insulation/vacuum.
A
Thermo Regulator
made of
Gold Amalgam
in a room full of
Hydrogen
serves as a functional early-game heat sink. With access to
Steel
, a
Thermo Aquatuner
placed in
Water
that is later boiled to
Steam
is the primary heat sink design through endgame; once these are developed and
Renewable Energy
is researched, a colony has moved properly into the midgame of heat management.
This self-powering
Metal Refinery
uses a radiator made of
Metal Tiles
to cool the
Petroleum
it uses as a working fluid more quickly. The steam room acts as a heat sink for both the refinery and the integrated cooling loop, which uses a simple radiant pipe-based radiator.
Radiators
are designed to have pipes exchange heat with the environment passively. Due to how
Thermal Conductivity
works, these are usually comprised of
Radiant Gas Pipes
or
Radiant Liquid Pipes
(or both) snaking through a mass of solid tiles - ideally
Metal Tiles
or
Window Tiles
made of
Diamond
, but even regular
Tile
can suffice (with
Granite
being the best option). The tiles may themselves be adjacent to
Tempshift Plates
with high thermal conductivity, if faster exchange over an area is desired - Diamond is again an ideal material. Vacuum can also be used to control which cells the radiator exchanges heat with.
Early radiators can simply run radiant pipes through gas - while slower than using solid tiles, it also allows for
Duplicant
movement and building construction. For cooling some structures such as generators,
Heavi-Watt Conductive Joint Plate
can also be used as part of a radiator. Buildings can be placed on top of a radiator, but will normally not exchange heat with the blocks below them. A thin layer of
Water
or
Petroleum
can rapidly increase heat transfer, as long as it remains under the amount needed to flood the building.
It is also possible to build "smart" radiators by using
Mechanized Airlocks
in a sealed chamber. While the airlock is closed, it is a solid block and exchanges heat with the pipe passing through it; when it is open, it becomes replaced by
Vacuum
and no heat is transferred. This can be connected to e.g. a
Thermo Sensor
to precisely control the temperature of the environment.
Midgame: Turbines
The
Steam Turbine
converts heat energy to electrical energy by converting
Steam
to
Water
. Building a turbine on top of a steam-based heat sink converts it into a powerful heat deletion device - though the turbine itself leaks some heat and must be cooled.
A turbine can handle steam temperatures of up to around
135 °C
/
275 °F
(in theory
140 °C
/
284 °F
, but this is unreliable in practice) by simply being placed in a
Hydrogen
atmosphere, and snaking a
Radiant Liquid Pipe
containing its own output back and forth across it. These "self-cooling" turbines are the most efficient way of converting heat into energy, but are beaten out by actively cooled turbines (linked to an aquatuner, usually the one in the heat sink beneath it) to maximise per-second heat deletion and energy output.
The
Steam Turbine
page covers turbine constructions in significant detail and will not be repeated here.
Endgame: Super Cooling
Super Coolant
is frankly overkill for cooling a well-designed base, and the first few batches should instead go towards
Liquid Oxygen
and
Liquid Hydrogen
production (or even condensation of
Sour Gas
). Once a large reserve is produced, and other industrial uses well in hand, it is substantially more energy-efficient than water when used with turbines; per the Steam Turbine page's calculations, a super coolant-based heat pump system optimised for heat deletion is 23 times as efficient as a water-based equivalent.
Generating Low Temperatures
The
Hydrogen Engine
and its
Spaced Out! counterpart
both require
Liquid Hydrogen
, making extreme low temperatures a core part of endgame progression. The
Not 0K, But Pretty Cool achievement
additionally requires cooling any building below
-267.15 °C
/
-448.87 °F
. As no natural environment is cold enough to produce liquid hydrogen, an Aquatuner or Regulator is needed.
A
Thermo Aquatuner
with
Super Coolant
will never freeze pipes, as the aquatuner has an absolute lower limit of
-272.1 °C
/
-457.78 °F
, just above the freezing point of
-272.15 °C
/
-457.87 °F
. Due to the mechanics of freezing, even without this limit, super coolant cannot freeze - it would need to reach the impossible temperature of
-274.15 °C
/
-461.47 °F
.
Notable Temperatures
The following table lists a variety of significant environmental temperatures and suggested coolants to reach them, subject to the following conditions:
Fluids must be able to guarantee they are
not above
that temperature (i.e., activating the Aquatuner on an "Above [X]"
Liquid Pipe Thermo Sensor
).
Fluids are selected according to the highest specific heat capacity, which determines Aquatuner energy efficiency.
Super Coolant
is only listed when no options without space materials are available.
Temperature
Significance
Aquatuner Coolant
Regulator Coolant
4 °C
/
39.2 °F
Refrigerated
Food
threshold
Polluted Water
Hydrogen
-10 °C
/
14 °F
Concentration of
Salt Water
to
Brine
Ethanol
-18 °C
/
-0.4 °F
Deep Freeze
Food
threshold
-34.6 °C
/
-30.28 °F
Condensation of
Chlorine
for
Gas Grass
-164.5 °C
/
-264.1 °F
Condensation of
Sour Gas
Methane
-182.96 °C
/
-297.33 °F
Condensation of
Liquid Oxygen
Liquid Oxygen
-255.15 °C
/
-427.27 °F
Condensation of
Liquid Hydrogen
Super Coolant
None
Coolant Comparison
The following comparison is made under these assumptions:
A coolant is
ideal
on a range if it has the highest SHC of any element which is liquid in that range, other than
Super Coolant
.
SHC is the primary factor for efficiency of an Aquatuner, but lower SHC may be preferable if exchanging heat directly with the environment (whether a steam room,
Anti Entropy Thermo-Nullifier
, or
Wheezewort
farm) via a radiator.
Fluids must be able to be directly cooled to the target temperature by a
Thermo Aquatuner
(i.e., there is at least
14 °C
/
57.2 °F
of separation between the freezing point and the target temperature).
Due to the relative contribution of
Radiant Liquid Pipes
to heat exchange, thermal conductivity is only considered in cases where specific heat capacity is equal (i.e.,
Water
versus
Polluted Water
).
Fluids are only listed if they are ideal for some temperature below
125 °C
/
257 °F
.
Hydrogen
is not considered due to the dramatically decreased performance of the
Thermo Regulator
and
Radiant Gas Pipes
compared to their liquid counterparts. It should only be used if a liquid coolant is not available.
Coolant
SHC
Thermal Range
Ideal Range (DLC)
Ideal Range (Base Game)
Nuclear Waste
7.440
26.9 °C
/
80.42 °F
to
526.9 °C
/
980.42 °F
Above
37.9 °C
/
100.22 °F
Unavailable
Water
4.179
-0.65 °C
/
30.83 °F
to
99.35 °C
/
210.83 °F
10.45 °C
/
50.81 °F
to
37.9 °C
/
100.22 °F
10.45 °C
/
50.81 °F
to
99.35 °C
/
210.83 °F
Polluted Water
4.179
-20.65 °C
/
-5.17 °F
to
119.35 °C
/
246.83 °F
-9.65 °C
/
14.63 °F
to
10.45 °C
/
50.81 °F
-9.65 °C
/
14.63 °F
to
10.45 °C
/
50.81 °F
and
99.35 °C
/
210.83 °F
to
119.35 °C
/
246.83 °F
Nectar
4.1
-82.5 °C
/
-116.5 °F
to
160 °C
/
320 °F
-68.5 °C
/
-91.3 °F
to
-9.65 °C
/
14.63 °F
Unavailable
Brine
3.400
-22.5 °C
/
-8.5 °F
to
102.75 °C
/
216.95 °F
None
-11.5 °C
/
11.3 °F
to
-9.65 °C
/
14.63 °F
Petroleum
1.760
-57.15 °C
/
-70.87 °F
to
539.85 °C
/
1003.73 °F
None
Above
119.35 °C
/
246.83 °F
Ethanol
2.460
-114.05 °C
/
-173.29 °F
to
78.35 °C
/
173.03 °F
-103.05 °C
/
-153.49 °F
to
-68.5 °C
/
-91.3 °F
-103.05 °C
/
-153.49 °F
to
-11.65 °C
/
11.03 °F
Methane
2.191
-182.6 °C
/
-296.68 °F
to
-161.5 °C
/
-258.7 °F
-171.6 °C
/
-276.88 °F
to
-161.5 °C
/
-258.7 °F
Liquid Oxygen
1.010
-218.79 °C
/
-361.82 °F
to
-182.96 °C
/
-297.33 °F
-209.79 °C
/
-345.62 °F
to
-182.96 °C
/
-297.33 °F
Super Coolant
8.440
-271.15 °C
/
-456.07 °F
to
436.85 °C
/
818.33 °F
-271.15 °C
/
-456.07 °F
to
436.85 °C
/
818.33 °F
Note that
Super Coolant
is the only liquid option for the ranges from
-182.6 °C
/
-296.68 °F
to
-171.16 °C
/
-276.09 °F
and
-161.5 °C
/
-258.7 °F
to
-103.05 °C
/
-153.49 °F
. As such,
Hydrogen
in a
Thermo Regulator
must be used to cool
Natural Gas
and
Oxygen
enough to condense them in order to use
Methane
or
Liquid Oxygen
as a coolant - though once the first batch is produced, it can be used to cool future supplies as long as input heat is controlled carefully.
Alternatives
Anti Entropy Thermo-Nullifier
(AETN)
The Thermo Nullifier is a structure which is found in some
Ruins
, and can be used as an early-game alternative to turbine-based heat sinks. The AETN takes in 10g/s of
Hydrogen
gas to remove 80kDTU/s of heat from itself. As 10g/s of Hydrogen is equivalent to 80W in a
Hydrogen Generator
, this has an effective energy efficiency of 1000 DTU/J - superior to the ~923.78 DTU/J achieved by a
Thermo Aquatuner
with
Water
as coolant, and by extension most other pre-endgame coolant options.
It is not recommended to produce hydrogen specifically for an AETN, and instead use byproduct hydrogen from
Electrolyzer
-based
Oxygen
production, or even a
Hydrogen Vent
. If including the costs of a dedicated electrolyzer, the energy value of 10g/s of hydrogen increases by ~10.71W, which makes the efficiency of the AETN drop to 881.93 DTU/J - at which point a water-based aquatuner/turbine system is superior, while also not consuming valuable
Water
.
Passive Heat Sinks and Steam Batteries
Temperatures which are above
125 °C
/
257 °F
can be connected directly to a
Steam
-based heat sink without use of an Aquatuner, running a turbine and bringing the temperature down to below
125 °C
/
257 °F
by a standard self-cooling turbine. The primary source of these temperatures naturally is from
Geysers
- particularly
Metal Volcanoes
.
Buildings which do not require duplicant maintenance, such as
Batteries
and
Power Transformers
, can also be built out of
Steel
and placed inside heat sink steam rooms, essentially eliminating the need to cool them in a base.
Auto-Sweepers
and
Conveyor Rail
systems expand this to a large number of low-maintenance industrial machines.
Steam can also be kept at higher temperatures (usually
200 °C
/
392 °F
) - while this requires an actively cooled turbine, it has its advantages. Heat stored in
Steam
loses energy far more slowly than any type of battery, especially with well-insulated walls. Coupled with appropriate automation (a
Smart Battery
to keep the lights on, and a
Thermo Sensor
to establish an upper limit on temperature), this allows the steam to essentially act as a massive battery, powering nearby machines as needed. Various geyser tamers and
Metal Refinery
designs can use this method to become essentially indefinitely self-powering, ensuring that interruptions to grid power do not stop production.
Low-Temperature Geysers
Several types of
Geyser
produce materials at low temperatures, allowing their output to serve as a rate-limited heat sink.
The
Carbon Dioxide Geyser
is largely useless for cooling due to the low SHC and thermal conductivity of
Carbon Dioxide
in both liquid and gaseous states. Heating the averaged output of 150g/s of CO2 from
-55.15 °C
/
-67.27 °F
to
30 °C
/
86 °F
only takes ~10806 DTU - which would cool a 10kg packet of water by about
0.26 °C
/
32.47 °F
. It would take 54 geysers to match the throughput of a single water-based Aquatuner. That said, this geyser can be useful for
Food
storage, as it combines a low temperature with the Sterile Atmosphere trait.
The
Cool Salt Slush Geyser
and
Cool Slush Geyser
have greater cooling potential, as
Brine
and
Polluted Water
have relatively high specific heat for low-temperature fluids.
Start by insulating your geyser and putting in some extra storage for when the geyser goes dormant.
Then attach the output pipe from that into the cooling loop you build into your base. this should be done with a
liquid bridge
similar to those that you would use to fill a cooling loop for an
aquatuner
.
And you're pretty much done. Using very cold water like this may have some risks but as long as heat is being produced by your base or being absorbed from the rest of the map this method will last you for your playthrough. Some points to note is that you should make the pipes out of
granite
and use normal
pipes
and not
radiant
ones. Regular granite or
igneous
pipes will release the 'chill' slowly enough into your base as to not cause Chilly Surroundings. If an area is particularly hot, then use radiant pipes or pipes made with thermally conductive material to remove more heat from that area.
Overall pipe work (it's a mess, it was actually meant to be temporary but i needed to keep it in to remove heat as well as expanded it overtime as my base cooling needs grew. It can be used to restore nature reserves)
Cooling machinery spaces with the cold water.
I used reed fiber killing several birds with one stone as the excess
fiber
they produce is always good for later on in the game, this same fiber bed is the dump for my germy polluted water from the bathrooms.
To remove the warm water use a liquid bridge that feeds into a
liquid shutoff
. A pipe
thermal sensor
is used to activate the switch this signal should be feed though a
filter gate
and set the filter gate to 10 seconds, this is done to prevent the switch from cycling too much. Using a liquid bridge like this will make the fluid "prefer"  to flow off the loop because of how the game mechanics work until that backs up. (see
bridge tricks
for more information) In addition another liquid bridge should be used to feed onto the pipe that will go to the reed bed. this is done to give preference to the polluted water coming from the bathrooms and keep them from backing up.
The cooling pipes can also be installed into the floor and roof of rooms instead of in the middle of them.  Bathrooms are especially problematic to cool this way since they typically already have pipes running in the floor and roof of the room as well as the interior. These spaces can be cooled by changing the tiles to metal and the temperature will eventually equalize out. Granite tiles also have a decent enough thermal conductivity for this application.
Partial Passive Cooling
Passive cooling means the system is independent and doesn't need additional power/cooling. This method is partial because it is not confirmed practically if it CAN be made fully independent.(in theory it should be able to)
In Oxygen Not Included are a few substances which have different SHC and the change is reversible. The difference between 2 substance states to adds/deletes heat from the system. The amount of heat you want to remove depends on the difference in Specific Heat capacity(lower energy state(colder) should have a higher SHC than the higher energy state).
Solid/Liquid
Liquid/Gas
SHC Difference
((DTU/g)/°C Removed)
Temperature
State Border
Nuclear Waste
Nuclear Fallout
7.125
526.9 °C
/
980.42 °F
Glass
Molten Glass
0.640
1126.85 °C
/
2060.33 °F
Ethanol
Gas Ethanol
0.312
78.35 °C
/
173.03 °F
Steel
Liquid Steel
0.104
1083.85 °C
/
1982.93 °F
Liquid Oxygen
Oxygen
0.005
-182.96 °C
/
-297.33 °F
NOTE: Nuclear Fallout condensation point is lower (
66.9 °C
/
152.42 °F
) than Nuclear waste vaporization point (
526.9 °C
/
980.42 °F
), so even if the substance deletes a very large amount of heat it requires some additional cooling.
