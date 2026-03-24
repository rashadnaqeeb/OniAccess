# Guide/Petroleum Boiler

A
Petroleum Boiler
is a device which is used to create
Petroleum
directly from
Crude Oil
, without the use of an
Oil Refinery
. It utilizes the fact that Crude Oil turns into Petroleum at a temperature of about
401.85 °C
/
755.33 °F
.
In relation
3
Oil Wells
running full time generate 10 kg of
Crude Oil
per second, which keeps 1
Oil Refinery
and therefore 1 Duplicant fully occupied, wasting 480 W of power and creating 5 kg of Petroleum per second, worth 5 kW of Power.
The theory
Crude oil, if heated up turns into Petroleum at a 1:1 ratio; 10 kg of Crude Oil therefore generate 10 kg of Petroleum for 10 kW power with no Duplicant errand generated.
Perfect mathematical model
Assuming an Oil Well ejects Crude Oil at
91.85 °C
/
197.33 °F
, it needs to rise in temperature for
310 °C
/
590 °F
, for 10 kg/s that is an amount 5,239 kDTU of heat per second (mass times specific heat capacity times temperature difference: 10*1.690*310). But since the generated Petroleum has a higher specific heat capacity it can flow against the new incoming oil, heating the crude oil up while cooling itself down by
297.67 °C
/
567.81 °F
(Heat divided by mass and specific heat capacity: 5239/(10*1.76)=
297.67 °C
/
567.81 °F
) from
401.85 °C
/
755.33 °F
to
104.18 °C
/
219.52 °F
.
In an imperfect world
Barely transformed Petroleum can, of course, not cause other crude oil to rise in temperature above, but it can come quite close. Therefore only a little bit more heat needs to be injected into the system through in order to push the crude oil over the edge to flash into petroleum.
Advantages
A Petroleum Boiler converts 100% of Crude Oil mass into Petroleum, while an Oil Refinery converts only 50% (+ 0.9% in
Natural Gas
).
It produces no troublesome to deal with Natural Gas.
Once it's set up, no
Duplicant
or Player interaction is required.
It is
Water
positive in conjunction with
Oil Wells
if all Petroleum is burnt in
Petroleum Generators
: 3000 g/s Water -> ~10000 g/s Crude Oil -> ~10000 g/s Petroleum -> ~3750 g/s
Polluted Water
. If burnt in four Petroleum Generators, it is (approximately) Water neutral and generates 2000 g/s of extra Petroleum.
Disadvantages
A Petroleum Boiler is resource- and time-intensive to set up; a lot of Raw Minerals will be needed for the Insulated Tiles.
A high output heat source is required; this is a serious hazard for the Duplicants building the Boiler.
Construction
The components of a Petroleum Boiler are as follows:
A heat source: Most often the Core or a Volcano with a
Magma
tank attached. The tank should be made from Obsidian
Insulated Tiles
to prevent heat leakage and melting of the tiles. When doing this, a device to regulate Magma flow to prevent tile formation and one to remove accumulating
Igneous Rock
. A
Robo-Miner
is only used in bad designs that do not prevent tile formation. Alternatively, a Thermo Aquatuner can be used. It will have to be made from Thermium to be able to reach the required
400 °C
/
752 °F
+ temperature without overheating. It also needs to be submerged in
Liquid
to facilitate sufficient heat transfer.
The boiler where Crude Oil is heated to turn into Petroleum. It should be made partly from
Metal Tiles
or
Diamond
Window Tiles
to let heat transfer inside. Diamond
Tempshift Plate
can then be used to more efficiently transfer heat to the Crude Oil.
A thermal coupling with a
Steel
Mechanized Airlock
to couple both heat source and boiler and to let heat inside the boiler in a regulated manner.
A
Plumbing
system which will let Crude Oil inside the boiler and remove Petroleum from it.
A heat exchanger, most often a so-called counter-flow heat exchanger, which will preheat the incoming Crude Oil and concurrently cool down the outbound Petroleum. By doing so, it decreases the heat required to power the Petroleum Boiler massively.
Volcano-powered Petroleum Boiler
Disclaimer: this build is considered outdated by the community, and has several flaws that will cause its failure in several possible ways. Use at your own risks.
This is an example of a Volcano powered Petroleum Boiler. It is designed to be able to use even the lowest output Volcanoes as a heat source. The Petroleum Output is 10 kg/s.
Note the long heat exchanger which allows sufficient heat transfer between liquids. The
Radiant Liquid Pipe
material is Gold.
Automation
settings:
FILTER Gate
: 20 s
Thermo Sensor
in Magma chamber: Below
435 °C
/
815 °F
Thermo Sensor in boiler: Above
405 °C
/
761 °F
Hydro Sensor
: Above 500 kg
This design was created by
Francis John
on YouTube.
The most notable factor in this design is the simultaneous pre-heating of incoming Crude Oil and cooling of outgoing Petroleum through the U-turns.
Ultimately though, the heat for the pre-heating still comes from the magma.
An example of a Petroleum boiler
The previous design in the Power, …
… Plumbing, …
… and Automation overlay.
Step by step construction:
Magma Blade
Catching Magma
Boiler Room
Seal Boiler
Heat Injector
Automation 1
Automation 2
Finish construction
Final Automation
Pressure Damage
Fill Slowly
Finished Boiler
Use Debris
Heat Damage
Clear a large area around and underneath a Volcano and vacuum it out.
Create a sizable Magma Tank, constructed from Obsidian Insulated Tiles, any other Material will eventually melt.
Build a 2 tiles deep hole with a
Steel
Mechanized Airlock
as a bottom, 9 tiles away from the tank.
Open the Magma Tank at the bottom, the Magma will leak out a span of 10 tiles and just barely slowly drip into the created hole.
Build another steel door on top of the hole so it can be conditionally opened.
Close the Magma Tank
Build the Boiler Room.
Its right walls can be Insulated Tiles made from Igneous Rock, since it won't experience the heat of the magma.
Its hot plate floor made from
Metal Tiles
can be made from
Gold
or
Copper
.
Aluminum
or
Steel
would actually inject heat too quickly for some weird results.
Place some
Tempshift Plates
in the boiler room. This does not only serve to effect to distribute heat, but also increases its mass and therefore cause the temperature not to fluctuate too rapidly.
Add a
Steel
Mechanized Airlock
as the thermal coupling. This door needs to be steel since it's temperature can soar rather high.
Add the boiler room Automation.
A Thermo Sensor set to "Above
406 °C
/
762.8 °F
"
At least the piece of
Automation Wire
on the Mechanized Airlock needs to be made of Steel, the others can be Copper or Gold, but mustn't be Aluminum or Lead.
Build the
Wires
to power the doors.
Seal up the Boiler Room
This side can be Insulated Tiles from Igneous Rock as well, since it will be 'cooled' by the
400 °C
/
752 °F
Petroleum inside the boiler.
Use
Diamond
Window Tiles
for the thermal coupling.
Place a
Steel
Mechanized Airlock
right underneath the place where the Magma will fall next to the Window tiles.
Build a Steel
Robo-Miner
on some Metal Tiles, where it has access to the two tiles in front of the Window Tiles.
Caution:
when building the Petroleum boiler in a mirrored fashion, the Robo-Miner will need to be placed differently.
the Robo-Miner can be placed further in, but sometimes the eventually falling lava might spread out two tiles wide, touch the metal tiles for a fraction of a second, and boil the coolant on them.
Hook up a new Thermo Sensor to the Robo-Miner.
Set the Thermo Sensor to "Below
435 °C
/
815 °F
"
Use
Steel
for all Automation Wires and Sensors
connect the Magma-dropper doors with an
NOT Gate
Place a
Memory Toggle
before the debris-dropper Door which
It should be set by the Thermo Sensor.
It should be reset by a
FILTER Gate
which is set to "25 s".
Use
Steel
for all Automation Wires and Sensors
Do not yet connect the Magma dropper-automation with the rest of the automation system as this would start the system and the Robo-Miner needs to be cooled first.
finish construction with a counter-flow system and possible maintenance entrance at the top
pour some Petroleum on the Robo-Miner for it to exchange heat properly
Buildings only exchange heat with the cells they occupy, not the ground or walls they are mounted to, and the Robo-Miner would operate in a vacuum, overheating itself without possibility to give off heat. By placing only a droplet of Petroleum there, it can exchange heat with it, and the Petroleum in turn with the Metal tiles around.
Mob up the excess Petroleum in the Heat Injector Room
Pour in barely enough Crude Oil (or Petroleum if you already have it) into the Boiler room for it to touch the Thermo Sensor.
check the Thermo Sensors and the Filter Gate for correct settings
build the
Automation Wire Bridge
to start the Boiler
It's likely that, upon startup, the Boiler Room experiences some Pressure Damage, but for this case we built a maintenance. With some ladders, we can access the broken tiles and continue.
Slowly Fill the Boiler, only a few 100 kg crude oil at a time
as time passes the incoming Crude Oil will exchange heat with the now in place Petroleum and will get hotter and hotter, while the outcoming Petroleum will get cooler and cooler, steadily increasing the Boilers efficiency.
Once the first scoop of Magma's heat is almost exhausted it can just drop down, but it still has some heat left. Having it drop on a Diamond Window Tile can help it heat up the incoming oil by about another degree.
Sometimes, when no new Crude Oil gets added to the Boiler, the Petroleum at the very top gets too hot without Crude Oil to counter-flow against, therefore, once new Crude Oil comes in, this new Oil gets too hot while already in the pipes, cracking them in the process. That's why the topmost Layer of the Boiler should have a maintenance entrance.
Tips
Aluminum
should definitely be used for a heat exchanger as its high
Thermal Conductivity
decreases its length significantly in comparison to
Gold
or
Copper
.
A double layer of Insulated Tiles should be used to prevent any heat leakage, especially around hot parts.
Everything in contact with Magma needs to be made from Steel or Obsidian to prevent (eventual) melting.
Drip Boiler Above Magma-Biome
This set-up is meant for mid-game segments, just after getting
Steel
and some
Plastic
, preferably from a
Glossy Drecko
ranch. It does not use any Volcano, but rather tapping into the
Volcanic Biome
. This is best done in
Spaced Out!
and with the default game world setting for the DLC, because the Biome is quicker to reach than in other types of playthroughs.
Petroleum Floats Above Crude Oil
The first critical factor of this set-up is that Petroleum is less dense than Crude Oil. When the Vent releases the packet of Crude Oil, it will automatically displace any Petroleum that is at the Vent - up into the waiting Liquid Pump.
Cooling the
Steel
Liquid Pump
The other critical factor of this set-up is keeping the pump cool; it will overheat without the heat transfer through the Metal Tile wall around it and the Tempshift Plates beneath its upper half.
The amount of water in the steam pockets next to the pump has to be more than 50 kg to keep the pump around
200 °C
/
392 °F
; more water makes the pump cooler.
Advantages
You don't need to deal with a Volcano; the
Volcanic Biome
can be safely reached with careful digging and diagonal tile-building.
Minimal space that has to be sealed in.
Of course, there is no need for
Thermium
.
The breach to the upper strata of the Biome can be sealed in with an Insulated Tile, preferably made of
Ceramic
.
Disadvantages
The most significant disadvantage is that higher Petroleum output means exposing the Pump to more just-converted Petroleum; it will be hotter, consequently. Any effective production rate above 5 kg/s of Petroluem will overheat the Pump.
Starting the drip is a headache; it has to be started just before the Metal Tile has been built over the breach. Doing so too late will have the Tile heating to over a thousand degrees and flash any drip that is below 5 kg.
Eventually, this will cool the upper substrate of the Volcanic Biome; this will happen in just 20 cycles. At equilibrium, the drip has to be gradually reduced such that output of Petroleum is just 650 g/s - this is slow, and has to be monitored every cycle.
You need at least three Steam Turbines; two adjacent to the pump, and one more to cool the first two. All of them will be underperforming in power generation.
Every once in a while - it is rare but of random frequency - a bit of Crude Oil gets pushed up into the Pump and gets sucked, thus introducing Crude Oil into the output line. This can be handled with a liquid filtering system (either with the Filter or a Shut-Off with Liquid Element Sensor), but this takes up space; this may not be feasible if the Magma Biome has inconvenient layouts.
This is the actual overlay.
This is the Liquid overlay.
External Links
A tutorial video on Petroleum Boilers, containing the above example and further ones for Minor Volcanoes and Aluminum piping.
