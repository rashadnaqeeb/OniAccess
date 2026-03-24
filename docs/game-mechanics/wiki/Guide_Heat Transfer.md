# Guide/Heat Transfer

Heat transfer
is an important mechanic in the game, but one that is not well understood by most players. Every object in contact with another object of a different temperature will either transfer some heat to it or accept some of its heat. Amount of heat transferred depends on temperature difference between the objects, heat conductivity of materials that make up the objects in contact, but also heat capacity of both objects, and thickness of the more isolated object.
Thermal Conductivity
Thermal conductivity is a material property measured in (DTU/m)/K, that is: DTUs per meter per Kelvin.
Heat transfer is multiplied by the minimum of thermal conductivities of both objects (except for non-insulated pipes and buildings). That is,
Liquid Chlorine
traveling through a
Granite
Insulated Liquid Pipe
will not transfer heat with the pipe much faster than if the pipe was made of
Igneous Rock
, as conductivity of Liquid Chlorine is well below that of Granite. For more detailed formula see
Thermal Conductivity
.
Since all
Raw Minerals
have higher heat conductivity than all commonly piped
Resources
, using Wolframite or
Granite
over
Sandstone
will not result in faster heat transfer from the piped resources to the surroundings of the pipe. One might think that pipes with higher conductivites would exchange heat better between pipe segments, but as of AU-221295, no heat is exchanged between pipe segments; heat is only exchanged between the contents, the individual pipe segment the contents are in, and the contents of the tile the pipe is in.
The
Raw Mineral
with highest thermal conductivity is
Granite
. The
Metal Ore
with the highest thermal conductivity is
Aluminum Ore
.
Heat Capacity
Heat capacity -- or specific heat -- is a material property measured in (DTU/g)/K, that is: DTUs per gram per Kelvin.
Heat capacity determines how much heat energy must be transferred for the object to change temperature. For example, if one object has twice the heat capacity of another, but is otherwise identical, it will take twice as much heat energy to heat it up by 1 Kelvin.
Objects with high heat capacity may take a long time to heat up or cool down, but the actual amount of energy transferred is higher than for objects with low capacities.
The
Raw Mineral
with the highest heat capacity is
Igneous Rock
. The
Metal Ore
with the highest heat capacity is
Aluminum Ore
.
Insulation Thickness
A
Duplicant
can change their own heat transfer thickness by wearing clothing created in
Textile Factory
. Doubling thickness halves heat transfer (and vice versa).
Insulated Tile
,
Insulated Gas Pipe
, and
Insulated Liquid Pipe
use a mechanic similar to thickness, where their heat transfers are divided by 20.
Surface area
Surface area within a tile seems to have a negligible - if any - effect on heat transfer. For example, a small puddle transfers heat just as fast as a whole tile full of same liquid.
Multiple tile buildings transfer heat on every single one of their tiles.
Fluid Ports
Heat transfer does not occur for fluids at fluid ports. The entry and exit of fluids at the ports appear to override scripts for heat transfer. This is true as of version
EX1-456169
.
Object Layers
A more thorough description of the different layers are in
Thermal_Conductivity#Equations
, in the section named
Thermal Element Categories
.
On 1 tile, there exists multiple layers.
The main/primary layer of the tile can contain only 1 of a solid (as in, a solid tile), liquid, gas, or neither (vacuum). This excludes contents of pipes/the conveyer/etc.
The secondary layers of this tile include the item layer, building layer, background building, liquid pipe, gas pipe, automation, conveyer, etc, all of which can exist on top of each other in 1 tile.
What is a tile in the main layer can transfer heat to the tiles directly touching the sides of the tile and not to the diagonal tiles.
Generally, objects in the secondary layers can only transfer heat if there is something in the primary layer.
Items (or Entities) in the item layer can transfer heat to solid layers below them in a vacuum but not to tiles to the sides and above them.
Implications
Aluminum Ore
is not necessary for most heat transfer applications, as in most cases it is barely more effective at it than
Wolframite
Use
Aluminum Ore
and
Igneous Rock
for heat transfer with liquids and gases
Small puddles of cold Water can cool down Duplicants who stand in them just as fast as full immersion
Small amounts of liquid will change phase faster than large amounts. Boiling a quantity of water, or freezing ice out of salt or pure water will take the same amount of time to convert the total whether it is done all at once, or using a slow drip. The slow drip, however, will provide a steady small supply of clean steam or ice, while the wait will be significantly longer if the volume is converted at once.
