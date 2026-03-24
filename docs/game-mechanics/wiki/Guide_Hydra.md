# Guide/Hydra

A large hydra array.
A "
Hydra"
, as the community colloquially refers to it, is a scalable
Electrolyzer
array, similar in functionality to a
SPOM
but designed to quickly convert large amounts of water into oxygen and hydrogen.  The large quantities of generated Hydrogen can then be used to power an entire colony, not just the Hydra itself.
One of the earliest designs you can find on the Klei forums by user "PVD" on April 11, 2017 :
https://forums.kleientertainment.com/forums/topic/77566-hydra-big-electrolyzer/
The design requires a fair bit of pre-planning to build in addition to phases of preparation before it can be implemented and used.  Primary designs use "flooded"
Electrolyzers
in conjunction with gas flow structures like the
Airflow Tile
to channel
Oxygen
and
Hydrogen
into specific areas for storage and use.
Benefits
A Hydra array is capable of quickly converting mass quantities of
Water
into
Hydrogen
and
Oxygen
.
Even a basic 9x Electrolyzer Hydra can supply enough Hydrogen to generate in excess of 7200 Watts of power.
Capability and Efficiency are only limited by the design and the availability of Water.
This method allows infinite storage of the output gases in structures made of basic
Tiles
.
Drawbacks
The gas storage areas need to be vacuumed out and primed before the Hydra can be utilized.
Should the stored gas in either area fall too low, the
Electrolyzer
Array could start mixing the output gas in that area.
Heat
could be an issue depending on how the gas is used.
Oxygen
may require cooling before pumping it into a
Duplicant's
living area.
There is a chance when loading into a save that an Electrolyzer will output gas into the wrong storage. (Special thanks to -Hotep Thunderbolt- of the Oni Discord for helping to investigate this glitch.  ~Possibly patched out of the game now as no new instances of it have been observed since 2023)
Design and Implementation
In order for a Hydra to function you will need enough
Power
to run all associated
Machinery
, this includes the pumping of
Water
and
Gas
into and out of your Hydra as well as powering each
Electrolyzer
in your array.  This can make a Hydra a tricky structure to use should your design eventually require the
Hydrogen
it generates to keep its production powered.  It is recommended to have an alternative or backup source of
Power
in case of
Water
shortages or other unforeseen failures.
In addition you will want to have a positive enough flow of
Water
to supply the Hydra as well as some safety implementations to keep it from malfunctioning, (see Drawbacks - issue with gas falling too low).  A simple safety feature would be to integrate an
Atmo Sensor
into your design that will use an
Automation Wire
to shutoff any logistics pumping gas out of your Hydra or associated
Gas Reservoir
when
Gas
Pressure
is too low.  (recommend keeping tile pressure above 5,000g for this purpose)
The chance of displaced gas mixing in the wrong storage area can cause your array to eventually malfunction, as this would enable Electrolyzers to freely deposit more of the wrong gas in the wrong area, which would require maintenance on the part of the player to fix.  You can mitigate this issue through extra design, such as implementing a
Gas Element Sensor
+
Gas Pump
in the top of your Oxygen Storage, and/or the bottom of your Hydrogen storage to detect and remove any possible buildup of the incorrect gas.
Recommended design approaches include finding an existing, proven to function, design then studying and building it yourself.  This will hopefully give you enough familiarity to begin your own prototypes, designs and integrations.
How does it work!?
Essentially when an
Electrolyzer
converts
Water
into
Gas
it outputs this
Gas
into it's upper left square, both
Oxygen
and
Hydrogen
basically fart into the air there and the game physics govern their movement after.  Hydrogen being lighter floats up and Oxygen floats down.  By flooding the Electrolyzer it forces the Gas to go into another tile and other game mechanics then affect where the gas will go.
To flood an electrolyzer you stack two different fluids inside the 2x2 tile area containing the electrolyzer itself, but the amount of fluid can't be too high or the game will consider the electrolyzer as actually flooded which will render it non-functional, but also can't be too low either.
One method for flooding an electrolyzer is building the enclosing tiles on every side but the top and then setting up a
Bottle Emptier
above it, set the emptier to dispense the first liquid, which can be just about anything other than water, I recommend Salt Water, some guides use polluted water, but should you need to do maintenance on your hydra later it could release polluted oxygen into it, which could complicate it working correctly. water is typically used second.
Typically a duplicant will deliver 200Kg of fluid if Auto-Bottle is enabled, which works perfectly fine as the liquid will split into 2 tiles of 100Kg each, after which you switch the emptier to water which will dump 200Kg and then stack on top of the first liquid, again splitting evenly to about 100Kg per tile above the first two tiles and properly flooding your electrolyzer for use in a hydra.
When a flooded electrolyzer outputs gas the game then has its own rules for where this displaced gas will go, in this case if you have adjacent tiles primed with the two gases already, the game will place the additional gas into the tile with the same gas, thus if you have two different tiles each separately primed either with Hydrogen or Oxygen, and the other tile primed with the opposite, the matching output gas will displace and split into the primed tiles.
Once you've got a handle on this game mechanic you can design an array to exploit it.
In this example you can see how the oxygen and hydrogen automatically sorts into areas already containing their respective gases.  The key to making this happen is using a flooded electrolyzer and having tile/s in the upper left dividing the flow of oxygen and hydrogen.
Above details how if you enclose a flooded electrolyzer with tiles the gas will want to escape through the airflow tiles attached off the upper left corner.
You can also enclose your electrolyzer this way to help with alternative array designs.
Other things to take into account
Since a Hydra is an electrolyzer array you should take into account the limit on pumping gas through a pipe and plan around this bottle neck in your design.  A
Gas Pipe
has a throughput of 1000 g/s of Hydrogen, one electrolyzer can put out 112 g/s of Hydrogen, a
Hydrogen Generator
consumes -100 g/s of
Hydrogen
, so from these details  you can additionally plan on some sort of
Hydrogen Generator
Array to turn that Hydrogen into
Power
.
With these limitations in mind you can plan a first basic Hydra, maybe with 9x Electrolyzers outputting 112 x 9 g/s (1008 g/s) of Hydrogen into storage, and then a
Gas Pipe
moving 1000 g/s to an array of 9 ~ 10 Hydrogen Generators.  Once completed your first design could potentially generate 7200 ~ 8000 Watts of Power per basic Hydra.
Making a Baby Hydra
So now that we have a Basic design in Mind, I'll walk you through making a Basic Baby Hydra, it won't be perfect, and it'll be bigger than it needs to be just so you'll have something to fiddle with.  Later when you feel confident in your familiarity with the design to make your own, you can miniaturize and/or expand your own prototype and maybe even make a permanently enclosed Hydra that you'll never need to mess with again.
When you design a Hydra you can make the Hydrogen and Oxygen pool in specific areas, typically on opposite sides of the Hydra structure, wherever would be the easiest for you to utilize, or for other design reasons.  For me I like to make it so I can later expand my Hydra, typically further left or right of the original structure, in case I need or want to later on, because of this I make my Hydrogen pool at the top of the structure, and my Oxygen pool at the bottom.
With my gas storage always on the bottom and top I can easily expand left or right, You could also easily design your Hydra to pool gas on the left and right sides to allow for expanding up or down.
First we'll dig out an area so we can begin our initial design.
We're going to create two entrances into each gas area so we can adjust / fiddle with the build later, for this purpose we'll use liquid step airlocks to save on space and materials.  (but strongly consider using a standard liquid lock, as the step lock is more fragile)  Additionally we'll make the outside walls of the structure insulated just to cut down on temperature bleed in/out, your typical hydra will semi-self maintain it's temperature between
40 °C ↔ 60 °C
/
104 °F ↔ 140 °F
because of the temperature of the gas it is storing, so you shouldn't have to worry about cooling the structure etc.
You can use pretty much any mineral for the insulated and regular tiles, personally I'll just use Igneous if available in large quantities, Mafic Rock is pretty great as well, and you can even use Granite if you want to use up a large supply.  Just bear in mind you really shouldn't require Ceramic or Insulation for this unless you are building it somewhere that extreme temperatures may affect it, like in a Magma biome or something.... in which case use Obsidian I guess...
For the electrolyzers I like to use Gold Amalgam, not because it is required, but just because it's pretty common and the bonus to overheat is nice just in case, but you could probably get away with other common ores, most likely you will never need Steel for a Hydra, but hey it's your Hydra, do as you please!
For the insulated Piping you may as well use the same mineral as your insulated / regular Tiles, although I'll recommend against Granite for this part, but it probably wouldn't matter in the end.
So, since we decided to make an array of 9x Electrolyzers for this Hydra we'll align the array in a 3 x 3 pattern, 3 rows or columns of 3 electrolyzers each, this will help us make a quick and easy design and easily allow for further expansion later on should the need arise.
At this point we pick how the gas will channel from the array by figuring out which ones go up / down, since an array generates 8+ x the oxygen the first channel should be for oxygen, this way if you expand you'll either have the same number of gas channels or +1 for the oxygen, but ultimately this detail won't affect your Hydra because of how the gas pools, so I'll chock this decision up to better design methodology I suppose.
Examples
[
"Hydra (big electrolyzer)"
] Klei Forums Post by: "PVD" on April 11, 2017
[
"100% Uptime Electrolyzer No Gas Filter No Gas Element Sensor"
] Klei Forums Post by: "EnergeticSecret" on January 11, 2019
[
"Compendium of amazing designs"
] Steam Community Post by: "Kharnath" on July 6, 2020
A More Modern Heavy Industry, Scalable, Self-Regulating, Maintenance-capable Hydra.
