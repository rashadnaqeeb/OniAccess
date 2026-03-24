# Guide/Temperature Management

Temperature management
is one of the key challenges of the game. Your
Duplicants
, your
plants
, your
critters
, and your
buildings
will suffer and break if their temperature is too high or too low. The environment can get warmer through buildings that produce heat, it can creep through walls and resources, even your Duplicants produce and spread some heat by mere existence. Similarly, Chilliness can spread by air exchange or heat spreading towards colder regions. Controlling the environment is one of the key challenges in this game.
Formally,
Heat
is the amount of energy transferred to equalise thermal energy of two bodies. The game uses
DTU per second
to display heat output, it is equivalent to
Joule per second
or
Watt
in proportion 800 DTU/s = 1 J/s = 1 W . Although it should not be confused with electric watt, since the game has no friction heat or electric heating in the general sense. Conservation of heat energy tend to vary depending on specific buildings.
It Is Too Cold
This problem is the easiest to handle.  Many buildings, your Duplicants, and certain special features of your colony environment produce heat.
Heat Sources
Space Heater
produces 18 kDTU/s heat at 120 W (150 DTU/s per watt), but it is generally not worth using, as its only other effect is a meager 10 decor.
Liquid Tepidizer
produces 4064 kDTU/s at 960 W (~4200 DTU/s per watt). While it is slower, it is way more power-efficient and can be used as a core of a central heating system.
Charged
Batteries
and
Jumbo Batteries
produce 1.25 kDTU/s (375 DTU/s per watt for jumbo 750 DTU/s per watt for small batteries based on self discharge rate) of heat when charged. They're small, short, clean and useful in other ways and so a good way to provide extra heat.
Charged Smart Batteries produce only 0.5 kDTU/s  (750 DTU/s per watt due to self discharge) but at greater electric efficiency then anything but the Liquid Tepidizer.
Power Transformer
produces 1 kDTU/s of heat.
Ceiling Lamp
produces 0.5 kDTU/s of heat at 10 W (50 DTU/s per watt), but it fits right above
Sculpting Block
.
Oxygen Diffuser
produces 1.5 kDTU/s of heat  at 120 W (12.5 DTU/s per watt).
Metal Refinery
produces between 264 and 2,340 kDTU/s of heat at 1,200 W (220 - 1,950 DTU/s per watt).
Coal Generator
produces 9 kDTU/s of heat while producing 600 W (15 DTU/s per watt), but utilising it can be tricky, due to the large amount of
Carbon Dioxide
in its exhaust.
Cool Steam Vent
outputs steam at
110 °C
/
230 °F
. It can be cooled down into
90 °C
/
194 °F
Water
, which can then be piped around the colony, for example to
Shower
(Duplicants won't mind near-boiling shower). Heat will then radiate from the pipes.
Glass Forge
outputs molten glass at very high temperatures.
Nearby
Biomes
can provide large quantities of heat (or cold). Biomes do not keep their own temperature after map generation, so this is a temporary source.
It Is Too Warm
Buildings' output temperatures
Output temperatures
Buildings
Input temperature
Carbon Skimmer
Deodorizer
Shower
Sink
Wash Basin
Water Sieve
Fully consumes input
Hydrogen Generator
Gas Range
Ore Scrubber
(???)
Fixed 20
Plants
and
Critter
produced items based on
Genetic Ooze
.
30+
Oxygen Diffuser
Algae Terrarium
Algae Distiller
Oxylite Refinery
37
Outhouse
Lavatory
Input
Desalinator
40+
40+
Molecular Forge
(???)
70+
Electrolyzer
75+
Rust Deoxidizer
Compost
Oil Refinery
80
Kiln
90+
Oil Well
110+
Coal Generator
Wood Burner
50+
Fertilizer Synthesizer
76+
40+
Petroleum Generator
Natural Gas Generator
110+
73.4+
Ethanol Distiller
93.4+
75+
Polymer Press
150+
200+
~1750
Glass Forge
Varies by metal/coolant
Metal Refinery
Unknown and/or irrelevant
Rock Crusher
Microbe Musher
Electric Grill
Apothecary
Power Control Station
Farm Station
Textile Loom
Exosuit Forge
This is the tricky bit. There are six straightforward ways to destroy heat:
Steam Turbine
will convert hot steam above
125 °C
/
257 °F
into electricity, while cooling the steam into
95 °C
/
203 °F
water.
Thermo Aquatuner
will remove 14 °C from its input liquid and apply the heat to itself, pair this with Steam Turbine to convert its own heat output to energy. The Steam Turbine needs be cooled with this as well since they also leak massive amounts of heat.
Ice-E Fan
occupies a duplicant and uses up ice. It cools the gas at 32 kDTU/s.
Ice Maker
, which deletes 20% of the heat in the water it cools (and releases the rest into its surroundings), and further reheating of ice will consume some more heat.
Wheezewort
works rather slowly in most gases and natural setups and can not be mass-produced, but does not use any power. Its cooling is equivalent to 12 kDTU/s in the best circumstances.
Anti Entropy Thermo-Nullifier
is able to provide around 80 kDTU/s of cooling by consuming
hydrogen
.
Avoiding Heat
In early game, it's better to move the heat to where it won't cause trouble or avoid generating the heat in the first place, rather than trying to truly destroy it.
Dump excess heat in a cold biome.
Acquire cold gases and cold water from cold biomes.
Plant irrigation is one of the worst places to misplace extra heat - do not use hot water to irrigate cold-loving plants (
Bristle Blossom
). If you have no choice, use valves to avoid storing excess hot water in farms (water being consumed does not heat the plant up, standing water does), use insulated pipes in the sections that need to stay cool, and pre-cool the water by winding pipes carrying it through cool areas.
Avoid creating machines like
Polymer Press
or
Metal Refinery
, but Ranch
critters
instead. This is really important in early game, when you do not have much time or resources to create proper
rooms
, gadgets and other things you may need to avoid spreading heat. Most materials can be "fabricated" by critters.
Use
Igneous Rock
pipes for hot fluids/gases in early-to-mid game. At late game use
Ceramic
and
Insulite
when managing really hot fluids/gases.
Use
Heavi-Watt Wire
to power things in an insulated hot area, then use a Gas Pump to create a vacuum chamber between the two
Heavi-Watt Joint Plates
to avoid the need for Power Transformers.
Use
Steel
to build non-emitting generators in steam rooms, or organize all heat producers in one area then collect the heat with conduction panels leading back to a steam room.
The void can also be used to store generators this way.
THC Differences
In the
Launch Update
, the majority of buildings were given temperature floors for their outputs, instead of a fixed temperature or one set by the temperature of the building itself. This significantly complicated heat deletion tricks, and made them more reliant on inadequacies in mass conservation, which happen to also erase the carried heat energy alongside the mass.
The buildings that allow for that can be found by differences in
Total Heat Capacity
of their inputs and outputs.
The heat energy erasure happens in cases where inputs' THC is above the outputs' THC, and tends to get more potent with higher temperatures. For instance,
Water
has a Heat Capacity of 4.179 DTU each gram per 1 °C, however
Oxygen
has only 1.005. It incentivize running some machinery with inputs as hot as possible to leech heat from other places, like using
Electrolyzer
which outputs
75 °C
/
167 °F
Oxygen
, if we assume the input of
Water
is also
75 °C
/
167 °F
, only a quarter of heat will be contained in the
Oxygen
, the rest goes into the
Hydrogen
and the
Electrolyzer
produces 1.25 kDTU per second to make up for the rest, heat deletion occurs when the input of
Water
is hotter than
75 °C
/
167 °F
, as the output temperature cannot go above
75 °C
/
167 °F
. However, the extra-hot output materials still need to be cooled down themselves. Crops also delete heat from its fertilizer, for instance, if
28 °C
/
82.4 °F
Water
goes into a pipe of a
Hydroponic Farm
all at
28 °C
/
82.4 °F
and surrounded by Atmosphere at same temperature, no heat transfer can happen in this scenario, but the plant will delete the water outright, without the heat going anywhere else, all the heat temperature will be lost from the water when consumed.
In other cases, the THC difference in materials makes the machinery inherently heat multiplying, this effect will increases the further the inputs' temperatures deviate from the floors (in either direction, but stronger in up). Penalizing running them outside the optimal temperature.
It also important to notice that some heat deletion is necessary for the long-term survival of your colony, since the game constantly adds more heat,
one way
or
another
.
However, just deleting heat comes with its own drawbacks, as over deletion of heat (removing more than producing) could result in the base going into freezing temperatures if left unchecked. To avoid this, either use sensors to regulate your cooling systems, calculate how much heat is produced/removed or find synergies between heat requesters like crops or
Slicksters
(they delete carbon dioxide and the heat contained within) that both need heat, and remove heat. Putting carbon dioxide generators on top of Slickster ranches separated by airflow tiles both produces heat and co2, and deletes them as well. Use insulated tiles to keep heat trapped, use more conductive tiles to direct heat to them (into
Balm Lily Flowers
or
Pincha Pepperplants
for instance). When planning the flow of heat, carefully selecting the materials based on heat capacity and mass will allow you to dictate precisely where the heat will travel.
Specific Heat Capacity
This property quantifies how much an object's temperature changes if one adds or removes an amount of heat energy, per unit mass. Its unit is
J
o
u
l
e
s
/
(
G
r
a
m
*
D
e
g
r
e
e
C
e
l
s
i
u
s
)
. Objects with larger specific heat capacity can hold more heat (or coldness—the lack of heat). Therefore, objects that are more massive or are hotter hold more heat than objects that are less massive or are colder.
Sometimes people talk about the "heat capacity" or "
total
heat capacity" of an object. An object's "heat capacity" is the
specific
heat capacity times the mass of the object. Conversely, the "
specific
heat capacity" of an object is equal to its
total
heat capacity divided by its mass. Note that buildings inherently have 1/5 the total heat capacity from what the straightforward formula would suggest.
Many calculations also talk about "total heat" or "heat energy". This is simply the total heat capacity multiplied by the temperature. However, there is a catch: This quantity depends on the units you use for temperature! The most physically "proper" way would be to use Kelvin, such that absolute zero would mean zero total heat. But if we measured this way, it would result in some buildings being incredibly good at heat deletion, and others staggeringly heat-generating. Instead, by convention we use Celsius, with a zero point that is much closer to temperatures that dupes actually experience.
How much heat transfer is required ...
... to cool down one Tile of Water with
80 °C
/
176 °F
to
20 °C
/
68 °F
in a
Cycle
? A tile full of Water contains 1 tonne (1,000,000 grams) of Water. The Temperature difference is
60 °C
/
140 °F
, the cycle is 600 seconds and the Specific Heat Capacity of Water is:
4
.
1
7
9
D
T
U
/
g
∘
C
H
e
a
t
(
D
T
U
/
s
)
=
4
.
1
7
9
D
T
U
/
g
∘
C
×
1
,
0
0
0
,
0
0
0
g
×
6
0
∘
C
/
6
0
0
s
=
4
1
7
,
9
0
0
D
T
U
/
s
Thermal Conductivity
This property defines how
quickly
heat can be exchanged between two objects (where walls, resources, gas, liquids, plants and items all are objects). A lower value means heat is transferred slower, a higher value means heat is transferred faster. The rate at which two objects exchange heat is defined by the
lower
thermal conductivity value of both objects (except for non-insulated pipes and buildings). For detailed formula see
Thermal Conductivity
.
Its unit is
D
T
U
/
(
M
e
t
e
r
*
S
e
c
o
n
d
*
D
e
g
r
e
e
C
e
l
s
i
u
s
)
. In this game, a Tile is considered to be one meter high and wide.
The larger the temperature difference between two objects, the more quickly heat will be transferred between them, but the longer overall it will take for them to equilibrate (come to the same temperature). Because of this, materials with high thermal conductivities are useful in situations where one wants to transfer (or conduct) heat quickly, and materials with low thermal conductivities are useful in situations where you want to prevent the transfer of heat via insulation.
For more information on how Energy and Wattage (Power) are related, read the
Power guide
.
Here
is a nice video explaining it.
Choosing Coolant
Thermal Conductivity, counterintuitively, is the least important parameter, it has to be higher than other recipients to collect heat, but only as much, since the system will be inevitably bottlenecked by the material with the lowest Thermal Conductivity.
Mass and Specific Heat Capacity, on the other hand are extremely important, since they will define the maximum size of "heat packet" that can be transferred. For example
Super Coolant
limited to 1 kg will perform almost as well as 20 kg packets of
Thermium
, despite their vast difference in conductivity. This is further capped by contact zone / radiator size, which might not be enough to fill or free the full packet.
Temperature range is a matter of convenience and/or limiting factor.
Petroleum
and
Crude
have great temperature range, but are the poorer mediums, that are competitive due to ability to maintain mass concentration.
Hydrogen
has good SHC, but mostly exist as a low-mass gas which limits its throughput.
Water
has good mass
and
SHC, but limited by its narrow state transition temperatures. As such
Polluted Water
should be used as an alternative for dealing with freezing temperatures.
Heat Movement Directions
Assuming the same materials, on solids and all materials heat disperses evenly, however, gases such as oxygen will rise if they are hot, the oxygen will be sorted by its heat where the hottest is at the top, water is affected the same.
