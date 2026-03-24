# Hidden Mechanics

Due to the nature of the game and its high complexity, there are many mechanics that were not intended by the developers but may be used by the player. Many of these behaviors are debated as to how much is considered "fair game", but it is ultimately up to the player to decide.
Generally, these mechanics are broken down into two main categories: Unusual Behaviors and were put there intentionally by developers for simplicity or easy-of-use, and the other is
Exploits
, which utilize a quirk or bug in a system (or interactions between systems).
Unusual Behaviors
Many systems in the game are designed with certain simplifications, either for the player or the developers. Some of these simplifications can be practical if intentionally utilized.
Heat Deletion/Creation
Many buildings in game have either a minimum or fixed temperature for outputs, meaning the heat energy of the inputs can be changed without other outside energy.
Some examples include:
Composts
output
Dirt
at a fixed
75 °C
/
167 °F
, no matter the temperature of the
Polluted Dirt
that was used for the composting process. Since Polluted Dirt can go up to
1712.85 °C
/
3115.13 °F
, the compost can theoretically delete 135.9 kDTU/s. Like
Refrigerators
, their internal temperature is at a fixed value that exchanges heat with the stored items to help them destroy germs.
Buildings like the
Water Sieve
and
Electrolyzer
(and many others) have a minimum output temperature and will add free heat if inputs are below it.
Germy Water in Sinks
Sinks
only need non-polluted
Water
to rid a passing
Duplicant
of
Germs
, but the water used can be contaminated with germs and still function. This allows for the germy
Polluted Water
of their outputs to be directly fed into a
Water Sieve
and back into the sinks without the water being treated for germs.
Self-Sustained Latrine
10% of pipe capacity avoids state change
An example of a super-cooled 1kg water packet in a pipe.
Packets of up to 10% of a pipe's capacity (1kg for liquids, 100g for gases) will not change state regardless of temperature.
This can be used to transport liquid and gas without worrying about temperature, and super-cool or super-heat them in pipes. When exiting the pipe, the liquid or gas will change state according to its temperature without breaking anything. Special care should be taken not to merge super-cooled or super-heated packets packets with others, as this can break the pipes.
This is an intended albeit surprising mechanic that was specifically coded.
Petroleum/Sour Gas from an Oil Well
Original thread
•
Petroleum Well setup
Oil Wells
have a minimum output temperature of
89 °C
/
192.2 °F
, but no maximum output temperature.
Common sense would dictate that you can only have up to
100 °C
/
212 °F
Water
temperature, but by
restricting pipes to 1kg/s
, you can super-heat the input water to as high a temperature as you want. The 10% pipe limit of 1kg/s exactly matches the Oil Well requirement.
When fed super-heated water, the Oil Well's outputted
Crude Oil
will match its temperature. If it is hot enough, it will immediately change into
Petroleum
, then into
Sour Gas
, creating heat in the process due to the SHC difference between
Water
and
Crude Oil
.
Steam Turbine cooling via its foundation tiles
A Water cooler via a
Steam Turbine
cooled with its foundation tiles.
Original thread
A
Steam Turbines
exchanges its building temperature with its foundation tiles (the tiles it's sitting on).
Most of the time, those tiles should be
Insulated Tiles
to avoid exchanging heat with the
Steam
below it.
On the other hand, the left-most and/or right-most tiles can be made out of heat-conductive material (
Diamond
Window Tile
or
Metal Tile
), because heat does not conduct diagonally.
Since those materials
Thermal Conductivity
is much higher than any gas, one tile can be enough to cool a Turbine when using active cooling.
Building construction temperature
With a few exceptions, buildings temperature is capped  at
45 °C
/
113 °F
when constructed.
This means massive amounts of heat can be deleted by using materials well over
45 °C
/
113 °F
at the cost of player interaction and Duplicant labor.
Building Heat Capacity
Buildings, referring here to all buildings that are not some form of
Tile
or
Steam Turbine
, have only 1/5th the heat capacity of their primary material. This is not indicated anywhere in-game. Effectively, this means 1 DTU will change the temperature of a building 5x more than it would the same mass of tile or debris.
This has numerous ramifications. For example, an 800 kg
Tempshift Plate
effectively only offers 160 kg of thermal mass, less than that of a
Tile
. It also means that buildings that produce large amounts of heat such as Aquatuners and Polymer Presses heat up very quickly.
This can be deliberately exploited by melting buildings or deconstructing them after they have changed temperature, as the resulting liquid or debris will retain the original building temperature. This effectively multiplies all heat inputted to the building by 5x. This is why
Ice Tempshift Plates
are a good way to melt ice, but also a poor way to cool down an area. This also makes makes
building melting exploits
dramatically more effective.
Debris formation manipulation
Debris formation cells priority
When a liquid freezes into a debris, the debris will try to form in the cell the liquid froze in.
By placing a
Mesh Tile
at that position, the debris will to try form on all 8 tiles around that tile, following the order: Up, Down, Right, Left, Up-right, Down-right, Up-left, Down-left. If none of those is available, the debris will be trapped into the Mesh Tile. Any solid tile, or other mesh tiles, can be used to prevent a debris from forming in those 8 locations.
This allows separating liquid and debris thermally, as well as having the debris form in a different room than the liquid, making, for example,
Magma
and solidified
Igneous Rock
much easier to handle.
When a debris is melted into another solid, it does not follow the same rule and instead try to form a tile at the position it's melted, then try all cells upwards until the top of the map, without trying any other adjacent cell.
AI speed limit
When the simulation speed drops below desired value, AI of duplicants and critters is throttled by staggering the activity in time. For example, in a small, early game colony, attacking a
Hatch
will typically result in it retaliating nearly immediately, while in a "heavy" late game colony, a hatch can be killed and then stand for a couple of seconds at 0 hp before it "notices" that it's dead.
This has some important implications: duplicants with high digging wasting most of their time "wobbling" and doing nothing when digging soft materials (
Regolith
), low input-per-feeding critters like
Pufts
and
Slicksters
reaching starvation timer between feeding attempts even in highly pressurized gas of their choice, duplicants wasting a lot of time "clapping" at a
Grooming Station
or
Shearing Station
before the critter notices and approaches the duplicant.
There are no proper solutions to those problems, but four workarounds exist: buy a better PC, lower the game speed, design your playstyle to minimize lag (avoid uncovering new planetoids, organize all debris, minimize unused pipes etc.), design your playstyle to be more lag-resistant (limit reliance on critters, dupes (and rovers, auto-diggers etc.), and instead focus on plants with long cycles, immobile machines).
Technical Exploits
Many mechanics are derived from how systems in the game are implemented in a technical sense. These are generally stable and not likely to change.
Doors as floors
Debris falls through the door, but Duplicants and Critters can walk on it.
Pneumatic Doors
can serve as floors that
Duplicants
and
Critters
are able to walk on, but let items and debris fall through, unlike
Airflow
or
Mesh Tiles
. However, buildings that require a foundation cannot be built on top of them.
This is an unintended feature that arose through the related mechanics.
Airlock doors as foundation
Manual Airlock
and
Mechanized Airlock
can be used as a foundation for a building when closed, and disable it when open.
This can be used to automate enabling and disabling a building with no automation input port by placing it on top of a Mechanized Airlock and controlling it via automation.
For example:
A
Wheezewort
in a
Planter Box
can be controlled via a
Thermo Sensor
.
An
Anti Entropy Thermo-Nullifier
can be controlled via a
Thermo Sensor
for finer control than is possible by regulating incoming fuel.
A
Liquid Reservoir
output can be automated.
Automatic Duplicant Regolith clearing
Regolith-covered Crown Moulding being excavated
Placing build orders for
Crown Moulding
on the surface allows to order Duplicants to dig
Regolith
when the tile is covered, instead of re-ordering whenever new regolith is created.
Since it is an invalid building location, the Crown Mouldings will never be built, but Duplicants will still dig anything that falls on those cells.
Natural tile heat sinks
Most natural tiles have far more mass than built Tile, and far more thermal mass than any other building which makes them very effective and readily available heat sinks in the early game. Furthermore, they lose half their mass, and therefore half of their stored heat, when mined. This can be leveraged to keep your base cool in the early game and buy more time until you need to build an active cooling system.
By building a heat-producing building such as a
Coal Generator
in a location surrounded by natural tiles, you can effectively contain almost all of the heat the building produces to the surrounding area, rather than primarily warming the air around the building which will then heat up your entire base. When you later mine out the natural tile after it has been heated significantly, almost half the heat produced by that building is deleted.
For the same reasons, natural tile is also very effective as a temporary insulator and heat sink to keep temperature-sensitive areas cool. Each Farm Tile in a Mealwood farm is made of 100kg of dirt, and might contain another 10kg of dirt on average during operation, for a total of 147 kDTU/°C heat capacity. By contrast, a 1200kg natural dirt tile has 1776 kDTU/°C heat capacity, and a 950kg natural sandstone tile has 760 kDTU/°C heat capacity. Leaving just one or two natural tiles per Farm Tile near a Mealwood farm can make it heat up several times more slowly than it would if all the surrounding natural tile were dug out immediately.
Similarly, you may want to place a polluted water cistern/heat sink somewhere that it can have a wall of natural tile (ideally at least 2 cells thick or with an outer layer of constructed tile so it's easy to replace the natural tile without spilling), only digging out the natural tile after you have an active cooling system ready.
Kiln melting to refine Metal Ore
Since
Kilns
have no overheat temperature and can be built with
Metal Ore
, operating them in vacuum will make them heat up with no limit, eventually reaching their material's melting point.
When that point is reached, the Kiln will melt, and the resulting liquid metal will instantly freeze into a
Refined Metal
because of the hysteresis temperature loss.
It will take a few cycles and a few tons of
Ceramic
or
Refined Carbon
production to reach the melting point depending on the material SHC. Since
Gold Amalgam
has the lowest SHC, it is the fastest to melt.
This converts Metal Ore to Refined Metal 200kg at a time at a 1:1 ratio.
Aquatuner melting to refine Metal Ore & produce Liquid Steel
Similar to Kilns,
Thermo Aquatuners
can be melted to refine
Metal Ore
by operating them in vacuum: when running constantly, they will reach the material melting point long before breaking from overheating.
The process is much faster due to the massive amount of heat Aquatuners produce, but uses power and cools the input liquid, which can be counteracted with a
Liquid Tepidizer
or just used elsewhere.
How much time and power is needed depends on the material SHC. Since
Gold Amalgam
has the lowest SHC, it is the fastest to melt.
This converts Metal Ore to Refined Metal 1200kg at a time at a 1:1 ratio. It's also a very convenient way to produce
Liquid Steel
for very high temperature builds.
One element per cell rule applications
The
One element per cell rule
has several applications that are considered exploits by part of the playerbase, despite being an intended mechanic, like
Infinite Storage
or
Hydra
.
Arbor Tree auto-harvesting
Original thread
Arbor Trees
branches cannot grow in liquids and will break after being submerged for too long.
This can be used by making a Waterfall fall on branches once they are fully matured to harvest branches without Duplicant interaction.
When doing so,
Wood
will fall on the floor, ready to be swept by an
Auto-Sweeper
for a fully automated farm.
Boosted Arbor Acorns production
Original thread
Arbor Acorns
have a chance to be spawned when a new branch starts growing and there is a
Pip
in the same room.
By submerging branches in liquid, either with waterfalls or liquid stacking, branches will automatically be destroyed a short time after starting growing, cycling through branches at a fast rate.
As a consequence, there will be a much faster
Arbor Acorn
production, but this won't produce any
Wood
because branches are destroyed before being fully matured.
Beads upward teleportation
A gas condensing in
Airflow Tile
being teleported upwards.
Pumping setup
•
Setup using condensation teleportation
When a bead is trapped in a tile it shouldn't be in and it cannot be moved to any adjacent cell, it will be teleported upwards, in the first valid cell found.
This can be used in two ways:
By making liquids form a bead when falling and closing a
Mechanized Airlock
over that bead, it will be teleported upwards. Since airlocks closing timing is not exact, it's advised to have a way out for liquids that isn't trapped in time: having airlocks close on liquids that have no way to move can destroy them instantly.
By making a
Gas
condense in an
Airflow Tile
, the resulting liquid forms as a bead and is forced to teleport upwards.
It's useful to teleport liquids in
Infinite Storage
without using a
Liquid Pump
, saving on power as well as being much faster. When using the condensation variant, this also means you can insulate the gas from the resulting liquid, only cooling the gas just enough to condense it while not cooling the resulting liquid.
This does not work when melting debris, as debris melting form droplets, not beads. The liquid will only attempt to form in directly adjacent cells (excluding diagonal cells) before being deleted.
Liquid/gas diagonal swapping
A Liquid-gas bypass design.
Original thread
Under some circumstances detailed in the original thread, a liquid and a gas can exchange places diagonally, with the gas going upward and the liquid downward.
This teleportation happens through a drip
Liquid Airlock
and as such can be used in various ways, such as high pressure gas storage, making vacuums, deleting some liquid or gas without Space Exposure.
Pumping superhot liquids
Pump ranges. Red=pumping; Blue=detection
Original thread
•
Setup example
Liquid Pump
and
Mini Liquid Pump
have different pumping range and liquid detection range, based on its foundation tile F. Two cells P1 and P2 that are in pumping range are not in direct contact with the Liquid Pump, and more on the Mini Liquid Pump depending on its orientation. This can be used to pump very hot liquids like
Magma
that would otherwise overheat or melt pumps without any thermal exchange. Since those two cells are not in the detection range, Pumps must be tricked into pumping by providing a liquid in its detection range. Since F is in direct contact of P1 and P2, it cannot be used, and the cell above cannot either because the liquid cannot be blocked above. This means T1 or T2 needs to be used to trick Pumps.
To trick the pump:
A droplet can be dropped on T2, which will be pumped by a Liquid Pump and re-ejected at the same place. This halves the hot liquid output to 5kg/s. Show in example (2)
3 examples of Magma Pumps
With 3 of the 4 orientations of a Mini Liquid Pump, a tile can be placed on T2 and a droplet left on T1, which will not be pumped, keeping the full 1kg/s hot liquid output. Shown in example (3)
A
Waterfall
can be dropped on T1, and broken just before it reaches T2 (by having an empty cell right of T1), allowing to get the full 10kg/s output out of a Liquid Pump. Shown in example (1)
To pump the hot liquid:
It can be pumped from P1 by ensuring the liquid doesn't get above that level. Shown in example (2) and (3)
A
Liquid Blade
can be used to have a liquid reach P2 but never exceed it, especially with a high viscosity liquid like
Magma
. Shown in example (1), which also allows dripping some of that into P1 for added safety.
A
Waterfall
can be dropped on P2.
Note that example (3) isn't cooled: the Mini Liquid Pump will eventually overheat from its own heat creation and will need to be cooled by a
Conduction Panel
, and when it is deconstructed it will drop Plastic in the hot liquid pool and creating Sour Gas. The other 2 examples can be cooled by the liquid used to trick the pump as depicted by
Wheezeworts
on the examples.
Rover workforce
After achieving space flight and the necessary technology, you can drop rovers on your home colony, increasing the workforce. Rovers are most useful for working in inhospitable areas. The technique is lossless because rovers and rover modules can be deconstructed, and the rocket exhaust recycled.
Starving farming (ranching)
Certain critters can lay an egg before starving when Tame, such as
Pacu
and
Shine Bugs
. As long as these critters are not overcrowded, they can provide infinite food.
Quick Sink use
Sink designed to be used nearly instantly
Bug report
•
Original video
The
Sink
performs the germ removal part of its action even if the action bar doesn't complete because of an interruption.
By removing a Sink's foundation when a Dupe is using it, the Dupe will be cleared of germs in a fraction of the time. The amount of time required varies depending on the number of germs on the Dupe.
Refrigerator as a cooling device
Minimal setup to turn a Refrigerator into a cooling device.
Bug report
•
Exploit example
•
Original video (russian)
A
Refrigerator
can be used as a cooling device because it outputs much less heat than it removes from its contents, and those contents are not insulated when the refrigerator is unpowered.
This can be automated with a
Timer Sensor
and a
Power Shutoff
. The Timer Sensor optimal values depend on the atmosphere temperature, but does not need to be pulsed very fast. 2s green / 2s red works reasonably well.
To maximize the cooling potential, use as many different items as possible. These items are the most convenient:
Muckroot
,
Nutrient Bar
,
Allergy Medication
,
Curative Tablet
,
Immuno Booster
,
Vitamin Chews
. More items means more cooling, but are either more difficult to produce (
Berry Sludge
,
Serum Vial
) or can spoil and need to be replaced.
The cooling is more effective at higher temperatures, so
Ceramic
is recommended to raise the overheat temperature to
275 °C
/
527 °F
.
This can be exploited to cool a
Steam Turbine
or condense the Steam from a
Cool Steam Vent
.
Bug-Driven Exploits
Some mechanics derive from a bug or similar in systems: they are generally unstable, may break games, and are liable to be fixed in future updates.
When the underlying bugs are fixed, they will be moved to the
Obsolete Exploits
page.
Instant bottle/canister emptying
Left: the full duration of the emptying errand. Right: the emptying errand is canceled as soon as it's started, completing it instantly.
When emptying a bottle or canister, a Dupe is locked into an errand of a few seconds. If you cancel the emptying errand during that invisible animation, instead of dropping on the floor, the bottle or canister is instantly emptied.
This allows saving a few seconds for each emptying errand.
Natural Tile from Airlocks
When deconstructed while the bottom cell is surrounded by tiles (for a total of 7 tiles as shown in the example), a
Manual Airlock
or
Mechanized Airlock
which element is of sufficient max mass will produce a natural tile rather than debris.
Uranium Ore
does not have a high enough max mass and cannot be used.
Aluminum Ore
can only be used for
Manual Airlock
and not
Mechanized Airlock
.  Those tiles can be used for
Pip
planting.
Build the Airlock surrounded by tiles.
Deconstruct it.
There is a natural tile with the Airlock where its bottom cell was.
Diagonal access
Duplicants can construct tiles and conduit background buildings like pipes, wires, and bridges diagonally. It has been confirmed as an intended mechanism by
Game Update 463874
in
Spaced Out DLC
.
This allows deleting gas/liquid by trapping them into four tiles, constructing a fifth one in the center then deconstructing it, easily creating a vacuum.
Auto-sweepers
can access items diagonally, which can be used to have them access items that are in a different room.
Duplicants can enter rooms under a locked door built at a diagonal, while the room retains liquid/gas contents, performing a climb animation
Plastic to Metal conversion
Some items are built from several materials, but when melted only converts to their main material for the sum of all their materials mass.
The most interesting example is the
Liquid Pipe Germ Sensor
, which is made out of 25kg of
Refined Metal
and 50kg of
Plastic
. When melted, it will produce 75kg of
Refined Metal
.
This can be used to convert Plastic into Refined Metal using Magma to melt arrays of sensors, making Refined Metal renewable even without a Metal Volcano.
Buildings in tiles
Example of exploits using building in tiles
Bug report
•
Original video
By replacing a
Tile
with a tile of the same type but a different material just before it's deconstructed, you can build another building inside. Once built, the tile and the building will co-exist in the same cell, leading to some unusual behavior that can be exploited to build a few contraptions:
A
Manual Airlock
or
Mechanized Airlock
set to "Open" inside tiles will create a perfect airlock that doesn't leak gases. A save and reload is needed after opening the door
Wall Pots
can be embedded in tiles to work correctly without the need for a floor compared to a
Flower Pot
. This is especially useful when combined with the Pip planting in Flower pots exploit.
It can be used to put a liquid over the trick cell of a
Liquid Pump
to
pump superhot liquids
The same technique cannot be used with
Gas Pumps
, as putting an insulated tile over its cell of interest (bottom left one) prevent heat transfer and allow it to start the pumping animation, but prevents it from actually pumping any gas.
Pip planting in Flower Pots
A
Pip
-planted
Dusk Cap
in a
Flower Pot
, growing at Domesticated rate without fertilizer.
Bug report
•
Original thread
Pips
planting can be abused to have
Plants
in
Flower Pots
and
Wall Pots
.  This also applies to
Hanging Pots
and
Aero Pots
, but it's not useful. Those plants will grow at the Domesticated rate, without the need for solid fertilizer (but will still need irrigation, making this exploit a poor match for plants that requires irrigation).
To use this exploit:
Build a
Flower Pot
. If you want to use
Sand
in the next step, do so under Sand tiles.
Entomb those flower pots in a natural tile. There are several ways, for example letting
Sand
fall over pots or cooking
Algae
debris into
Dirt
.
Make a
Pip
plant seeds.
Once you have a flower pot with a plant, you can copy its settings to other flower pots to propagate the exploit without repeating those steps. Digging up the natural tile is not required.
After copying the settings, you can remove the seed and plant another one.
Good candidates for this exploit are
Mealwood
,
Dusk Cap
,
Wheezewort
,
Grubfruit Plant
(Spaced Out!) and
Pikeapple Bush
(Frosty Planet Pack) because they don't require irrigation, require solid fertilizer and have a normal growth direction. Plants requiring irrigation can be planted using this exploit to eliminate solid fertilizer requirement, but they will have to be irrigated by Duplicant. Disabling the Flower Pot does not negate the plant's requirement of irrigation.
To make a plant accessible to critters, so that
Dreckos
can eat Mealwood and
Divergents
can rub plants, a tile must be constructed next to the Flower Pot, just like when using a
Planter Box
.
If using Grubfruit Plants, make sure to copy from a Spindly Grubfruit in a flower pot, not a Divergent-rubbed one. As there are no seeds for a non-spindly Grubfruit, those are unable to be copied into flower pots properly.
Pip unlimited planting
Original thread
Pip
planting calculation occurs when one is taking a seed, and doesn't re-check all conditions when it is actually planting the seed.
This can be abused to force them to plant in illegal tiles, or make several Pips each plant a seed on the same tile, causing multiple to overlap.
Liquid germ deletion
Liquid Scrubber
Bug report
•
Exploit example
By creating beads out of germy liquid (using a
Liquid Vent
with a
Mesh Tile
just below it), a significant amount of germs will be deleted from it. Germy liquid can quickly be sanitized by repeating this process.
For practical purposes, there is little advantage to this exploit compared to the intended method of letting liquids sit in
Reservoirs
in a
Chlorine
environment.
Dupes recovering breath without consuming Oxygen
A duplicant recovering breath without consuming any Oxygen
Bug report
By making a Duplicant wear an
Oxygen Mask
or
Atmo Suit
without refilling its
Oxygen
, they will stop to recover their breath but not actually consume any Oxygen.
This can be exploited to remove the need to provide Oxygen to Duplicants at the cost of frequent gasping interruptions.
Buildings internal storage heat manipulation
Water
temperature manipulation exploited on the
Water Sieve
.
Bug report
•
Original thread
A building's internal storage temperature sometimes uses the wrong value for output elements, using the element's previous temperature rather than current temperature of the element being converted.
For example, this can be exploited on the
Water Sieve
by feeding it a small amount of cold
Water
, then switching to hot
Polluted Water
. The output water for the converted polluted water will sometime have the previous cold water temperature, deleting large amounts of heat.
Other buildings known for exhibiting this issue are the
Desalinator
and
Oil Refinery
.
Thermo Aquatuner/Regulator pulsing power-saving
Liquid/gas based timer sensor
Thermo Aquatuner
and
Thermo Regulator
only need to be enabled and consume power when a liquid/gas packet pass through them to enable their cooling effect.
By designing a special timer sensor that's based on liquid/gas flow rather than actual time (since liquid and gas flow change when building pipes), it's possible to enable both buildings only when a packet pass through, to save on power. Using this, it's possible to use only a third of their normal power, about 400W for the Aquatuner (versus 1200W in normal operation) and about 80W for the Regulator (versus 240W in normal operation).
This special timer sensor is designed to work against a single liquid/gas packet being looped in a pipe with 3 pipe sections linking input and output of that pipe, with an Element or Temperature Sensor to detect when the packet is passing through, and only activate the Thermo Aquatuner/Regulator for a short time based on that.
Since this exploit is dependent on precise timing, it might not work fully as expected depending on game performance.
Power supply pulsing
A simpler variant that saves more power but reduces the throughput is to pulse the power supply (rather than the building itself) of a
Thermo Aquatuner
or certain other buildings using a
Power Shutoff
and a
Timer Sensor
set to 0.2s/0.2s.
Using that method, you can expect to see a 95-100% reduced power consumption, while still achieving 50% throughput.
An Aquatuner on a 0.2s/0.2s pulsed power supply will rarely consume power, around 5% of the time. It is unclear why this happens, as it can go for long series of ticks without consuming any power at all.
Most buildings can be power-pulsed in this manner to eliminate nearly all their power consumption, however their throughput will always be reduced. Most buildings that don't require Duplicant operation will maintain 50% throughput: a non-exhaustive list includes
Thermo Aquatuner
,
Thermo Regulator
,
Liquid Tepidizer
,
Electrolyzer
,
Oxylite Refinery
, Filters and Lights (if you don't mind epilepsy).
The main exceptions are Pumps,
Water Sieve
and
Desalinator
, which lose dramatically more throughput or even produce nothing, possibly due to long "wind up" animations or complicated multi-tick sequences: these buildings still consume nearly no power, they just perform little to no useful work, possibly to the extent of making them less power efficient when 0.2s/0.2s power-pulsed.
This exploit can come with downsides like intense rapid hammering sounds and blinking.
Left-to-right liquid/gas flow heat transfer
An example of left-to-right staircase exploit applied to a Petroleum Boiler, compared to a right-to-left staircase that behaves normally.
Bug report
Liquids
and
Gases
flowing from left to right don't properly calculate heat.
This can be exploited with liquids with a staircase going from left to right- any heat difference gets amplified. This is less easy to exploit for gases.
This can be exploited for both cooling or heating:
For heating, input cold liquid at the top, and counter-flow something hot.
For cooling, input hot liquid at the top, and counter-flow something cold.
It can be exploited in a late-game
Petroleum Boiler
using
Thermium
: by making
Crude Oil
flow in a staircase counterflow heat exchanger, Crude Oil heating is amplified, which allows massive savings in heat required to boil the Crude into
Petroleum
.
Due to the nature of the bug, the effect drops off exponentially as the staircaise gets longer.
A high difference in temperature at each step is the key to exploit this bug efficiently.
Tepidizer target temperature bypass
An example of a minimal power-positive exploit using the target temperature zone check.
Bug report
•
Exploit example
There are two ways to trick a
Liquid Tepidizer
into working above its target temperature:
Rapidly pulse it on and off with a
Timer Sensor
or looped
NOT_Gate
.
Add a 400kg+ cell of liquid inside its target temperature zone check.
By using either method, you can get a Liquid Tepidizer to heat up liquid to very high temperatures before a game reload.
After a game reload, there is a soft limit of
125 °C
/
257 °F
after which the Tepidizer will only heat at 64kDTU/s. The full 4064kDTU/s under
125 °C
/
257 °F
can still be used to boil
Water
and extract a very small amount of power using a
Steam Turbine.
Liquid duplication
This article is a
stub
. You can help Oxygen Not Included Wiki by
expanding it
.
While some
liquid duplication techniques
have been fixed, it is still possible to generate large amounts of liquid.
Enclosed Telescope Oxygen rationing
The Enclosed Telescope requires a supply of oxygen in order to be used by a Duplicant. However, once they are inside the Enclosed Telescope, no oxygen is required, and the Duplicant's breath will not deplete even if it runs out. This exploit is most useful inside rockets, where oxygen supply may be more limited. An easy way to do this is to use a Gas Valve set to the smallest value to trickle oxygen into the Enclosed Telescope, keeping it active without consuming much oxygen.
Infinite copper ore farming
Building then deconstructing a
Solo Spacefarer Nosecone
drops an extra 100 kg of copper ore from the free Rocket control station inside. This can be repeated as a free source.
Skipping repair costs
Repairing damaged wires costs 2.5 kg of metal. Replacing the damaged wire with one of a different metal type skips the 2.5Kg metal cost. Of course deconstructing and reconstructing would work too but requires two actions instead and interrupts the power flowing. Should work with buildings also by deconstructing / reconstructing, and is useful on low metal starts
Freshener Spice returns food to 100%
Example
If food spiced with
Spice Grinder
manages positive decay, it can go back up to 100% freshness. For example, spiced food under
-18 °C
/
-0.4 °F
and in a sterile atmosphere will gain 0.9% freshness per cycle.
Disabled building room requirement
Example
Disabled, floating or otherwise non-functional buildings still count towards room requirements. This is useful to save on resources or floor space.
