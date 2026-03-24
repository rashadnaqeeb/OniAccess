# Insulated Tile

Insulated Tile
Used to build the walls and floors of rooms. Reduces
heat
transfer between walls, retaining ambient heat in an area.
The low thermal conductivity of insulated tiles slows any heat passing through them.
Research
Temperature Modulation
Dimensions
1×1 tiles
Category
Base
Decor
-5 (Radius: 1 tile)
Materials
Raw Mineral
400 kg
Insulated Tiles
(not to be confused with the resource
Insulite
) are used as wall and floor tile to build rooms and manage
heat
at the cost of -5 Decor and double the materials compared to normal Tiles. Specifically, it reduces the
Thermal Conductivity
of the material used in its construction to 1/16256th (e.g. Igneous Rock has thermal conductivity of 2, but 0.000123 when built as an Insulated Tile), however for some reason the game displays the thermal conductivity as being reduced to 1/100th.
Usage
Reduces
Heat
transfer with adjacent tiles,
liquids
, and
gas
. Use it to keep heat from leaking in/out.
You'll want to cover the near or outer edges of the
Temperate Biome
to keep away heat from nearby biomes.
Generally, the material with the
lowest
Thermal Conductivity
(TC) and the
highest
Specific Heat Capacity (SHC) possible is the most preferable. TC takes care of heat transfer, while SHC prevents sudden temperature shifts.
Using
Igneous Rock
is a staple due to abundance and the highest SHC among
Raw Minerals
.
Alternatively,
Mafic Rock
can be sourced from
Rust Biomes
or
Space Biomes
, it has a half of TC of
Igneous Rock
, but only fifth of SHC.
Ceramic
is the more advanced material, made in the
Kiln
using
Coal
and
Clay
. It has TC of 0.62 and SHC of 0.84.
Space materials
allow to produce
the best possible solid insulator
in the
Molecular Forge
.
Insulated Tile Materials
Material
Thermal Conductivity
Specific Heat Capacity
Melting Point
Ceramic
0.620
0.840
1849.85 °C
/
3361.73 °F
Fossil
2.000
0.910
1338.85 °C
/
2441.93 °F
Granite
3.390
0.790
668.85 °C
/
1235.93 °F
Graphite
8.000
0.710
276.9 °C
/
530.42 °F
Igneous Rock
2.000
1.000
1409.85 °C
/
2569.73 °F
Insulite
0.00001
5.570
3621.85 °C
/
6551.33 °F
Isosap
0.170
1.300
200 °C
/
392 °F
Mafic Rock
1.000
0.200
1409.85 °C
/
2569.73 °F
Obsidian
2.000
0.200
2726.85 °C
/
4940.33 °F
Sandstone
2.900
0.800
926.85 °C
/
1700.33 °F
Sedimentary Rock
2.000
0.200
926.85 °C
/
1700.33 °F
Shale
1.800
0.250
826.85 °C
/
1520.33 °F
Minimum temperature delta for heat exchange
Temperatures in Oxygen Not Included are represented using floating-point numbers, which have limited precision. As a result, some ingame objects will require a minimum difference in temperature for actual temperature change — and thus, heat exchange — to occur
at all
. This isn't a programmed game mechanic, but can have important implications: for example, a difference of roughly
250 °C
/
482 °F
is required for solids or liquids to exchange heat with an Igneous Rock Insulated Tile, while gases require a much lower
10 °C
/
50 °F
due to gas:solid heat exchange's 25x multiplier.
In this table, ΔT represents the minimum temperature difference in °C for heat exchange with an Insulated Tile at
20 °C
/
68 °F
.
Material
ΔT (solid/liquid)
ΔT (gas)
Ceramic
672
26.9
Fossil
226
9.0
Granite
116
4.6
Graphite
44.0
1.8
Igneous Rock
248
9.9
Insulite
∞
∞
Isosap
3791
152
Mafic Rock
99.2
4.0
Obsidian
49.6
2.0
Sandstone
137
5.5
Sedimentary Rock
49.6
2.0
Shale
68.9
2.8
Advanced details of required ΔT
The details in the table are pretty good as a rule of thumb, for example remembering roughly
250 °C
/
482 °F
(
10 °C
/
50 °F
) for Igneous,
100 °C
/
212 °F
(
4 °C
/
39.2 °F
) for Mafic and
670 °C
/
1238 °F
(
27 °C
/
80.6 °F
) for Ceramic, but sometimes there will be meaningful deviations
The ΔT is directly proportional to the thermal mass of the tile, and inversely proportional to the Thermal Conductivity of the Insulated Tile. If the other cell has a higher thermal mass than the Insulated Tile, then the thermal mass of the other cell determines the required ΔT. For example a Mafic Insulated Tile has a thermal mass of 80 (400 x 0.2), while a full Magma tile has a thermal mass of 1840 (1840 x 1), so the required ΔT increases 23x, between
99.3 °C ↔ 2283.9 °C
/
210.74 °F ↔ 4143.02 °F
. Paradoxically this means that Mafic Insulated Tile will not exchange heat with full magma tiles, however half full magma tiles or obsidian tiles at magma temperature have a lower thermal mass and thus can exchange heat.
The required ΔT is also directly proportional to the temperature of the Insulated Tile in kelvin, the table gives the ΔT for 293.15 K, if for instance an Igneous Insulated Tile was only 90 K - the boiling point of liquid oxygen - the ΔT would be only
76.1 °C
/
168.98 °F
. Given that liquid oxygen will aggressively exchange heat with Insulated Tiles via the partial evaporation mechanism, this will cool down the Insulated Tile to such a temperature that it readily exchanges heat with the warm outside environment. However if the liquid oxygen were isolated using a lining of metal tiles, then the warm Insulated Tiles would maintain sufficient ΔT to not exchange heat at all with the cryogenic inner lining.
Tips
Although
Natural Abyssalite Tiles
separating biomes have a
Thermal Conductivity
of 0.00001 (DTU/(m*s))/°C, which is lower than most Insulated Tiles, in most cases Insulated Tiles provide better insulation due to their property of using the "lowest thermal conductivity" rather than the geometric mean of the two thermal conductivities. Abyssalite tiles exchange less heat with other Abyssalite Tiles, Insulated Tiles, Debris and in some cases Buildings (practically only the Steam Turbine and Tempshift Plate can overlap Abyssalite), but Insulated Tiles exchange less heat with gas, liquid, solid (excluding Insulated, Abyssalite) tiles, for example Abyssalite exchanges roughly 11x more heat with Steam than an Igneous Rock Insulated Tile, and 36x more than a Ceramic Insulated Tile would.
Incidentally,
Insulite
is made from
Abyssalite
in the
Molecular Forge
.
See Also
Insulite
Insulated Liquid Pipe
Insulated Gas Pipe
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
Airflow Tile
Automatic Dispenser
Bunker Door
Bunker Tile
Carpeted Tile
Drywall
Fire Pole
Gas Reservoir
Insulated Door
Insulated Tile
Ladder
Liquid Reservoir
Manual Airlock
Mechanized Airlock
Mesh Tile
Metal Tile
Mini-Pod
Plastic Ladder
Plastic Tile
Pneumatic Door
Smart Storage Bin
Snow Tile
Storage Bin
Storage Tile
Tile
Transit Tube
Transit Tube Access
Transit Tube Crossing
Wicker Door
Window Tile
Wood Tile
see all buildings
