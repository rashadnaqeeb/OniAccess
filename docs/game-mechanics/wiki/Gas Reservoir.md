# Gas Reservoir

Gas Reservoir
Stores any Gas resources piped into it.
Reservoirs cannot receive manually delivered resources.
Research
HVAC
Dimensions
5×3 tiles
Category
Base
Overheat
at
75 °C
/
167 °F
Decor
-10 (Radius: 2 tiles)
Ventilation
Input: Gas Intake Pipe
Output: Gas Output Pipe
Storage Capacity
1000
kg
Auto Out
Green
: Emptied below low threshold
Red
: Filled above high threshold
Materials
Metal Ore
400 kg
Gas Reservoir
is a building that allows for 1000 kilograms of
Gas
storage.
It can connect to a Gas Intake Pipe and a Gas Output Pipe, and is useful as a
power
-free in-line buffer, since it also eliminates need for intermediary
Gas Pumps
. At least one segment of Gas Intake Pipe must be connected for Gas Reservoir to work. Gas Output Pipe is optional.
Be aware the Reservoir can overheat depending on the environment it is sitting in, and has a low initial overheat temperature of
75 °C
/
167 °F
. It is best to build them out of
Gold Amalgam
or
steel
when dealing with high temperature environments.
When a gas reservoir is deconstructed, it releases it contents freely into its surroundings.
Disabling the Reservoir
Reservoir can be disabled by
Duplicant
, when disabled it only stops outputting its content, while still accepting the input.
This can also be achieved by destroying the base the Reservoir is built upon, which can be automated by building it on top of a
Mechanized Airlock
and open and closing it via automation. Only a single tile of the Reservoir must be on top of an open Mechanized Airlock for it to be disabled.
Liquid Reservoirs
Versus Gas Reservoirs
Resources are best kept in the form of solids, because
Storage Bins
remain the most economical and compact means of storage. However, if resources cannot be feasibly kept in solid forms due to volatile thermal properties (e.g. narrow temperature ranges in between their forms), then they have to be kept in fluid form.
In this case, the
Liquid Reservoir
is a better choice, if the resource can be held in liquid form prior to any downstream processing. For example, if the player's intention is to hold fuel for
Hydrogen Generators
, it is better to keep
Water
in Liquid Reservoirs (for piping into
Electrolyzers
later) than keeping
Hydrogen
in Gas Reservoirs: A single Liquid Reservoir can hold 5000 kg of water which can be electrolyzed to produce 560 kg of Hydrogen, and one Liquid Reservoir occupies 6 tiles of space for a density of about 833.33 kg/tile of water, or 93.33 kg/tile of Hydrogen. In comparison, one Gas Reservoir can hold up to 1000 kg of Hydrogen, but occupies 15 tiles, giving a density of only 66.67 kg/tile.
However, the Gas Reservoir can allow for easier pipe-laying (due to its larger footprint) than Liquid Reservoirs, especially if the Liquid Reservoirs are to be placed next to each other.
Moreover, having a Gas Reservoir for holding the results of downstream processing, e.g. the
Hydrogen
from
Electrolyzers
and their attendant
Gas Pumps
, allows for considerable reserves in the case of disruptions and for easier monitoring.
Tips
It is generally best not to expose Gas Reservoirs of any material to extreme environments. They release their contents when they are completely damaged which can be a major problem since, unlike buildings which only store solid or liquid contents, escaping gasses are not easily contained and will quickly spread far and wide.
Contents inside the reservoir
only
exchange
heat
with two tiles: the tile containing the reservoir's output port
and
the tile right below the output port (i.e. the reservoir's bottom-middle tile and the tile directly below that). A reservoir's contents will
not
heat up or cool down the reservoir itself
directly
. Thus, if both of these two tiles are in a
vacuum
(e.g. by placing an
Airflow Tile
,
Mesh Tile
, or
Insulated Tile
below the output port and keeping those tiles in a vacuum), then there will be no heat exchange at all - i.e. you can store very hot or very cold gases in a
Copper
reservoir without them heating up or cooling down their surroundings.
The reservoir itself will exchange heat with all 15 of its tiles, but no surrounding tiles. If the reservoir is not kept in a vacuum, then its contents will exchange heat with whatever substance occupies its bottom-middle tile (e.g. a gas or liquid), and
that
substance will exchange heat with the reservoir itself.
Depending on the type of gas, a Gas Reservoir may be a significantly more space-efficient means of storing gases than leaving them in the world. For example,
gas vents
overpressurize at 5 kg per tile, and one Gas Reservoir holds 1000 kg - the equivalent of 200 max-pressure tiles of a vent's output, contained in a building which only occupies 15 tiles. Even if one were to pump the same 1000 kg of gas into a room at 20 kg/tile using
High Pressure Gas Vents
, it would require 50 tiles of space.
However, there may still be more space-efficient ways of storing the same materials, such as in liquid or solid form. See the above comparison with Liquid Reservoirs for more details.
Gas Reservoirs are more energy efficient and more convenient than storing gases externally, due to their inclusion of output and input ports (which can already drive fluid flow on their own). More importantly, Gas Reservoirs, like Liquid Reservoirs, do not block Duplicant access.
If a reservoir is submerged in
Chlorine
, any
germs
on its contents will be quickly killed.
Gas Reservoirs average out the temperature of each gas pumped into it (thus smoothing out temperature spikes/differences from the input gas). This allows, for example, for much easier temperature control if a
Gas Pipe Thermo Sensor
is placed on the output pipe.
Gas Reservoirs automation outputs can be used to control filters with a
NOT Gate
, to ensure that the filter only operate when the tank has filled up to a certain level, and fully take advantage of the buffering capabilities of the reservoir.
If multiple gases are stored(H2,O2,Cl,CO2 from lighter to heavier), output cycles through all stored gases, but heavier gases get slight priority and the lightest gas(Hydrogen) will remain if multiple different gases of the same amount are stored. This is probably intended and due to the output tile being at the bottom of the reservoir, hence output prioritizes heavy gases.
Blueprints
Available blueprints
Bluemoon Gas Reservoir
Dartle Gas Reservoir
Faint Purple Gas Reservoir
Golden Gas Reservoir
Greenpea Gas Reservoir
Ick Yellow Gas Reservoir
Lumb Gas Reservoir
Mod Dot Gas Reservoir
Mush Green Gas Reservoir
Party Dot Gas Reservoir
Puce Pink Gas Reservoir
Weepy Blue Gas Reservoir
See also
Liquid Reservoir
History
RU-284571
: Introduced.
AP-395113
:
Gas Reservoir
and
Liquid Reservoir
have smart storage functionality
AP-397125
: More buildings are brought forward into Overlay space in the
Gas
/
Liquid conduit overlay
U51-596100
: Increased Gas Reservoir storage capacity from 150kg to 1000kg. Slight adjustments to building art.
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
