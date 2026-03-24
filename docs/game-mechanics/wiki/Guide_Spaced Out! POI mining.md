# Guide/Spaced Out! POI mining

The
Starmap in Spaced Out
contains numerous
space Points Of Interest
(POI), which act as a renewable supply of various resources. A rocket with Cargo Module(s) can be sent to a POI to collect those resources. Each POI has a maximum mass available as indicated by "Total Mass Remaining", and it will slowly renew itself after being mined. Each POI also shows each available type of resource and its percentage.
Resource Collection
Resources can be collected by sending a rocket with a
Drillcone
(filled with 1t of
Diamond
) and the appropriate
Cargo Module(s)
(Solid, Liquid or Gas) to the POI. The Cargo Module(s) will be filled with the corresponding resource in the percentage ratio shown on the POI, up to the max storage capacity and limited to 20t of mass in total across all resource types (Solid, Liquid or Gas).
To compute how much of a given resource can be obtained from a POI, use the following logic:
If storage is not available for a type (Solid, Liquid or Gas):
No resources will be collected for that type
If storage is available for a type (Solid, Liquid or Gas) and storage capacity is less than 20t:
Multiply the storage capacity by the percentage displayed on the POI for that resource
If storage is available for a type (Solid, Liquid or Gas) and storage capacity is greater than 20t:
Multiply 20t by the percentage displayed on the POI for that resource
For example, a
Rocky Asteroid Field
has 40% Sedimentary Rock, 40% Igneous Rock, 20% Copper Ore.  If a
Large Cargo Bay
is used, then up to 27t  of cargo space is available for solids, therefore it will yield 8t of Sedimentary Rock, 8t Igneous Rock and 4t of Copper Ore, for a total of 20t. However, if a Small Cargo Bay is used, then cargo space is limited to 12t, therefore it will yield  4.8t of Sedimentary Rock, 4.8t Igneous Rock and 2.4t of Copper Ore.
Note that each POI also has a Total Mass Remaining indicator. Every time the POI is mined, the mass remaining will be reduced by 20t (or up to the max storage). If there is less than 20t of mass (or less than the max storage), then it is not possible to extract the optimal full amount. Over time, the POI will slowly increase back to its maximum. Therefore, it is advised to wait until at least 20t (or up to the max storage) is available to optimize rocket fuel usage.
NOTE: Resources are extracted from the POI even if its storage type is not available!
For example, if a POI has both Solid and Liquid, but only Solid storage is available, both Liquid and Solid are extracted from the Total Mass Remaining even if no storage is available on the rocket.
Rocket Design
For an infinitely sized rocket, the large cargo modules (
Large Cargo Bay
,
Large Liquid Cargo Tank
,
Large Gas Cargo Canister
) are strictly better than their smaller counterparts for moving bulk cargo. However, rockets are significantly height-limited, especially due to mandatory modules.
Mandatory Modules
4 units of height are reserved for the
Drillcone
.
4 units of height are reserved for the
Spacefarer Module
(note that the
Solo Spacefarer Nosecone
cannot be used because the Drillcone occupies that space).
5 units of height are required by any endgame engine (
Petroleum Engine
,
Radbolt Engine
,
Hydrogen Engine
).
Before fuel/oxidizer and cargo storage is added, any rocket with a drillcone and a high-performance engine thus has a minimum height of 13.
Engine Selection
The following table shows the round trip range of different engine types and various configurations.
In order to reach any destination on the
Starmap
, a round trip range of 22 is required. Configuring a rocket for a round trip range of 20 is a good strategy as it allows a rocket to reach every destination except for the very last ring of hex tiles. There is normally at least one POI with rare resources (
Niobium
and
Fullerene
) within the range of 20. A common strategy is to initially use a
Radbolt Engine
to collect rare resources and get over the hump to create
Liquid Hydrogen
for the Hydrogen Engine that can then reach any destination on the map.
Note that when travelling 10 hexes or fewer, a
Steam Engine
's 12 units of available height surpasses every other option.
Engine Build
Fuel Tanks
Max Height
Height w/o Cargo
Free Height
Round Trip Range
Radbolt Engine
N/A
20
13
7
20
Hydrogen Engine
1
LOx Tank
1
Fuel
35
20
15
16
1
LOx Tank
2
Fuel
35
25
10
32
Petroleum Engine
1
LOx Tank
1
Fuel
35
20
15
10
1
LOx Tank
2
Fuel
35
25
10
20
100% Mass Capture
As a full drillcone load produces 20t of mass according to the POI's composition ratios, the limits of each kind of storage module can be found by simply dividing the module's capacity by 20t.
To capture all solid output:
A small cargo bay is sufficient if the POI is at most 60% solid.
A large cargo bay is sufficient for all POIs.
To capture all liquid output:
A small liquid tank is sufficient if the POI is at most 45% liquid.
A large liquid tank is sufficient for all POIs.
To capture all gas output:
A small gas canister is sufficient if the POI is at most 18% gas.
A large gas canister is sufficient if the POI is at most 55% gas.
A small and large gas canister are sufficient if the POI is at most 73% gas.
Two large gas canisters are sufficient for all POIs.
From this we can deduce some implicit facts about rocket cargo modules:
A rocket with a large gas canister never needs a large cargo bay or liquid tank.
No rocket needs both a large cargo bay and large liquid tank.
The largest possible necessary cargo bay arrangement is two large gas canisters, a small cargo bay, and a small liquid tank for a total height of 16 units. This would describe a POI which contains all three resources, with more than 73% being gas. This rocket is also unlaunchable - the most free space available on a drillcone-equipped rocket is 15 (hydrogen/petroleum engine with single fuel tank).
The Exploded Gas Giant POI meets these characteristics, so a fully-loaded drillcone cannot mine it without losing some materials.
POI List
Solid Cargo
Liquid Cargo
Gas Cargo
Height
Carbon Asteroid Field
Frozen Ore Asteroid Field
Gilded Asteroid Field
Ice Asteroid Field
Oxidized Asteroid Field
Radioactive Asteroid Field
Rocky Asteroid Field
Sandy Ore Field
Swampy Ore Field
1 Large
0
0
5
Chlorine Cloud
Metallic Asteroid Field
Oily Asteroid Field
Space Debris
1 Large
1 Small
0
8
Exploded Ice Giant
Organic Mass Field
1 Large
0
1 Small
8
Interstellar Ocean
Salty Asteroid Field
1 Small
1 Large
0
8
Forested Ore Field
1 Large
0
1 Large
10
Glimmering Asteroid Field
1 Large
1 Small
1 Small
11
Oxygen Rich Asteroid Field
1 Small
1 Small
1 Large
11
Radioactive Gas Cloud
1 Small
1 Small
1 Small 1 Large
14
Exploded Gas Giant
1 Small
1 Small
2 Large
16 (unlaunchable)
Cargo Unloading
Temperature Management
Most cargo is retrieved at
26.85 °C
/
80.33 °F
, with the following exceptions:
Element
Temperature
Solid Oxygen
-243.15 °C
/
-405.67 °F
Solid Methane
-223.15 °C
/
-369.67 °F
Methane
-173.15 °C
/
-279.67 °F
Solid Carbon Dioxide
-81 °C
/
-113.8 °F
Liquid Chlorine
-73.15 °C
/
-99.67 °F
Polluted Ice
-43.15 °C
/
-45.67 °F
Ice
-41 °C
/
-41.8 °F
Fullerene
Snow
-31 °C
/
-23.8 °F
Brine
9 °C
/
48.2 °F
Algae
Dirt
Salt
Slime
Sulfur
16.85 °C
/
62.33 °F
Crude Oil
76.85 °C
/
170.33 °F
Liquid Copper
1526.85 °C
/
2780.33 °F
Liquid Iron
2226.85 °C
/
4040.33 °F
Liquid Tungsten
3726.85 °C
/
6740.33 °F
As rocket ports must be in direct contact with the
Rocket Platform
, heat from rocket landings can pose a risk to low-temperature cargo (
Liquefiable
solids,
Methane
, and
Liquid Chlorine
) if cargo is unloaded before exhaust has dissipated. (Care should also be taken that unloaders are made of materials with high melting points, especially when dealing with
Hydrogen Engines
.)
The molten metals can also potentially freeze inside pipes, and
Liquid Tungsten
in particular is hot enough to melt any
Raw Mineral
.
Insulated Liquid Pipes
made of
Ceramic
or ideally
Insulation
can carry these liquids directly to a
Steam
chamber to freeze them and run turbines. To ensure that pipes handling iron and tungsten remain cool, it can be useful to run water through them when not in use.
