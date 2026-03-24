# Rocket

This article may be
outdated
. It was last updated for
LU-356355
. These versions were tagged as updating the game mechanics discussed here:
EX1-444349
•
EX1-452242
•
U43-525812
This article is related to Base Game content.
There is a version of this article for the
DLC, see
Rocketry (Spaced Out)
.
Rockets
are multi-component constructs used to visit destinations in outer space found on the
Starmap
.
Construction
Each rocket must have an engine at its base, a
Command Capsule
at the top, and a
Gantry
providing access to the command capsule.
Rockets may optionally include other modules, such as
Cargo Bays
for carrying materials back from expeditions or
Research Modules
for
Data Banks
.
Players can safely deconstruct and replace modules in a rocket, but modules can only be built if connected vertically (possibly via other modules) to an engine.
While there is no limit to the number of modules in a rocket, the range of a rocket is dependent on its mass, so unnecessary modules are ill-advised.
As rockets typically produce a large amount of heat during take-off, building the rocket inside an insulated tower can help to prevent heat from leaching into other parts of the base. Building the rocket in space alleviates this problem, but meteors need to be considered.
Engines
There are 4 engines in the game:
Steam Engine
,
Petroleum Engine
,
Biodiesel Engine
, and
Hydrogen Engine
.
Solid Fuel Thrusters
can be used in conjunction with engines to provide extra range. Note that solid fuel thrusters require their own gantry to provide access for supply errands.
Steam Engines are the entry-level rocket engine that must be used to unlock the others. Without Solid Fuel Thrusters they are limited to a 10,000 km range, allowing access to only the closest destinations on the starmap. However, they only require
Steam
, do not require separate fuel storage tanks, and have significantly cooler rocket exhausts.
Petroleum Engines are used in intermediate-level space exploration. In addition to the engine itself, they require
Liquid Fuel Tanks
and
Solid
or
Liquid Oxidizer Tanks
to function. Although technically capable of reaching any destination on the starmap, they can only reach 110,000 km destinations with cargo bays and no solid fuel. Petroleum engines using exclusively solid oxidizers (
Oxylite
) are further limited to a 60000 km range with cargo. Nevertheless, the fuel required is relatively easy to obtain, allowing players to collect rare space materials in preparation for the Hydrogen Engine.
Hydrogen Engines are the most advanced type of rocket engine, working similarly to Petroleum Engines but offering better distance. They run off
Liquid Hydrogen
, which requires temperatures below
-250 °C
/
-418 °F
to create, possible through the use of
Super Coolant
and
Thermo Aquatuners
.
All engines have an exhaust, which is divided into 2 parts:
An element emitted at a specific temperature under the rocket and does not go through tiles. All rockets engine emits at
50 kg/s
, except for the
Biodiesel Engine
which emits at
100 kg/s
.
A heating factor which heats elements under the rocket up to
2926.9 °C
/
5300.42 °F
, at a rate depending on the engine type and go through tiles and has a 3x9 rectangular pattern.
Both are done during launch and landing of the rocket.
Exhaust Properties
Steam Engine
Petroleum Engine
Biodiesel Engine
Hydrogen Engine
Element Emitted
Steam
Carbon Dioxide
Carbon Dioxide
Steam
Element Temperature
150 °C
/
302 °F
1226.85 °C
/
2240.33 °F
1426.85 °C
/
2600.33 °F
1726.85 °C
/
3140.33 °F
The heat injected into tiles directly is dependent on the exhaust element temperature and the distance from the central output cell of the engine:
Heat Amount injected
50%
100%
50%
33%
50%
33%
25%
33%
25%
20%
25%
20%
17%
20%
17%
14%
17%
14%
13%
14%
13%
11%
13%
11%
10%
11%
10%
Cargo Bays
Cargo Bays are used to retrieve resources from destinations. There is no limit to the number of
Solid
,
Liquid
,
Gas
, or
Biological
cargo bay on a rocket, but bear in mind these add considerable weight to a rocket, reducing the distance it can travel.
All cargo modules have the same weight of 2 t and the same capacity of 1 t, with the exception of
Biological Cargo Bay
which has a capacity of 10 items.
The resources returned from a destination depends on the save file, but is the same every time for each type of container and each specific planet on a given save. Different destination types offer different typical resource distributions (listed below).
Solid Cargo Bays
bring back one ton of solid material from a planet.
Liquid Cargo Bays
bring back one ton of liquid material.
Gas Cargo Bays
bring back one ton of gaseous material.
Biological Cargo Bays
bring back a small number of seeds or critters, if the destination has them.
Utility Modules
Rockets may contain other modules for additional functions.
Research Modules
will unlock information about a destination, providing 50 data units in the process that can be used with a
Virtual Planetarium
. Future missions to a fully analysed destination will only yield 10.
Sight-Seeing Modules
will allow passengers to come along for the trip, reducing stress.
Command Capsule
Every rocket ends with a
Command Capsule
at the top, which is where the astronaut enters and a space suit is to be provided.
Rocket Launch
Temperature overlay during rocket launch using a Hydrogen Engine.
Prior to takeoff, the
Command Capsule
checks for several conditions:
The rocket must have a fuel tank (note that
Steam Engines
have their fuel tanks built-in).
A destination within the rocket's range must be selected in the
Starmap
.
A Duplicant with the Rocket Piloting skill must be assigned and manning the Command Capsule.
The Command Capsule must have an
Atmo Suit
.
All Cargo Bays must be empty.
The destination must have sufficient resources for the Cargo Bays installed in the rocket.
When all conditions are met, the Command Capsule will output a Green
Automation
signal. Sending a Green automation signal to the Command Capsule's automation input will start the launch.
In addition to the conditions above, the rocket will not take off if there are solid blocks or buildings obstructing its flight path, such as closed bunker doors, tiles or blocks. Some objects, such as extended
Gantries
, will not block the rocket, but be heavily damaged or destroyed. Other objects, such as background objects (wires, pipes, etc.) and
Ladders
are not directly damaged by rockets, but may liquefy on contact with the rocket's exhaust gases.
During takeoff, rocket engines (and solid fuel thrusters) expel large quantities of superheated Gas around the Engine.
Steam Engines
and
Hydrogen Engines
release
Steam
, while
Petroleum Engines
and
Solid Fuel Thrusters
release
Carbon Dioxide
. Additionaly the area below the engine is heated up in a 3x9 rectangle. The heat is directly added to the current Heat of whatever tile/gas/liquid is inside of the 3x9 rectangle. The heat applied decreases with distance from the engine, and is slightly higher in the middle column than the outer columns. Heat applied directly by the rocket engine cannot heat anything past
2926.85 °C
/
5300.33 °F
, although this is still sufficient to melt even tiles made of
Obsidian
or
Thermium
, given enough launches/landings and inadequate cooling.
Rockets slowly accelerate to reach a top speed of 10 cells per second when launching, and decelerate from that speed when landing.
The more height a rocket has, the slower it accelerates.
Rockets do not require full fuel tanks to launch, but all fuel and oxidizers will be consumed during launch. For this reason, it is optimal to match the fuel and oxidizer loaded into the rocket to the distance you wish to achieve. A tool like the
Rocket Calculator
can be useful here.
Rocket Return
After an expedition, rockets will return to their initial construction location in their entirety, carrying cargo and
Data Banks
according to the modules included in the rocket.
Space Scanners
can be set to detect a returning rocket, which can be used to open
Bunker Doors
in preparation for landing.
Similarly to take-off, rockets produce a large amount of heat during landing, and can damage certain buildings in the way (most notably extended
Gantries
and closed bunker doors).
Tips
Rocket modules are calculated from top to bottom, this means you can use
Research Modules
to discover
Rare Resources
and return them on the same launch if you build the
Cargo Bays
below the
Research Modules
.
Engines must be built on solid ground, but the ground can be removed afterwards with no consequence; they will take off as normal and "land" in the air when returning.
It is possible to build multiple rockets on top of each other in a single vertical shaft. Launching lower rockets will not damage the ones above.
All engines and thrusters have a width of 7, while all other modules have a width of 5. However, building tiles on the sides will prevent the rocket from launching.
Decreasing the maximum storage capacity of
Steam Engine
or fuel tanks will cause duplicants to remove the extra. Items will be dropped on the ground (liquid fuel in canisters).
Both
Steam Engine
and
Liquid Fuel Tank
can fit 900 kg of fuel, however due to a single input pipe, the latter can be filled with a single liquid pump in 90 seconds, while the former requires two gas pumps and 900 seconds. When using
Liquid Oxygen
for
Liquid Oxidizer Tank
(2.7 t capacity) and 3
Liquid Fuel Tanks
(900 kg capacity each), rocket can be refueled in 270 seconds.
The Heat added below an Engine is quite useful in reaching higher Temperatures. With the proper insulation the area can reach melting Temperatures for most Materials and therefore be used for generating Power with
Steam Turbines
, losslessly refining
Crude Oil
, or even melting
Regolith
.
Setups
Note that cargo modules that allow to bring back resources all weigh 2 tons, while the
Research Modules
and
Sight-Seeing Modules
weight only 200 kg. These are referred to below as "small modules" and you can replace 10 of them with 1 cargo module.
Using
Steam Engine
with no extra thrust
9 small modules to first line of asteroids
with
Solid Fuel Thruster
12 small modules to first line
6 small modules to second line
with 2
Solid Fuel Thrusters
2 small modules to third line
Using
Petroleum Engine
(which requires
Liquid Fuel Tank
and
Solid Oxidizer Tank
filled to 1/3 maximum - to match the amount of fuel)
with no extra thrust and fuel tanks
23 small modules to first line
18 small modules to second line
10 small modules to third line
with 1 extra
Liquid Fuel Tank
you can bring large container module to as far as 50000 km
with 2 extra
Liquid Fuel Tanks
you can bring large container module to 60000 km
Fuel Quick Lookup Tables
Data is calculated with
oni-assistant rocket calculator
.
A Solid Booster requires additional 400 kg Iron and 400 kg Oxylite, which is not listed in the table.
Steam Engine
Destination Distance
[km]
1 Steam Engine
5 Research Modules
1 Command Capsule
1 Steam Engine
1 Solid Booster
5 Research Modules
1 Command Capsule
1 Steam Engine
1 Solid Booster
1 Cargo Bay
1 Command Capsule
10000
695 kg Steam
163 kg Steam
528 kg Steam
20000
Unreachable
809 kg Steam
Unreachable
30000
Unreachable
Petroleum Engine
Oxylite, Research Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Petroleum Engine
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
10000
306 kg Petroleum
306 kg Oxylite
16 kg Petroleum
16 kg Oxylite
20000
569 kg Petroleum
569 kg Oxylite
279 kg Petroleum
279 kg Oxylite
30000
832 kg Petroleum
832 kg Oxylite
543 kg Petroleum
543 kg Oxylite
40000
Unreachable
1098 kg Petroleum
1098 kg Oxylite
820 kg Petroleum
820 kg Oxylite
50000
1394 kg Petroleum
1394 kg Oxylite
Unreachable
1155 kg Petroleum
1155 kg Oxylite
60000
1723 kg Petroleum
1723 kg Oxylite
1514 kg Petroleum
1514 kg Oxylite
70000
Unreachable
2124 kg Petroleum
2124 kg Oxylite
Unreachable
1987 kg Petroleum
1987 kg Oxylite
80000
2592 kg Petroleum
2592 kg Oxylite
Unreachable
2656 kg Petroleum
2656 kg Oxylite
90000
Unreachable
Unreachable
Liquid Oxygen, Research Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Petroleum Engine
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
4 Liquid Fuel Tanks
2 Liquid Oxidizer Tanks
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
4 Liquid Fuel Tanks
2 Liquid Oxidizer Tanks
5 Research Modules
1 Command Capsule
10000
227 kg Petroleum
227 kg Liquid Oxygen
12 kg Petroleum
12 kg Liquid Oxygen
20000
422 kg Petroleum
422 kg Liquid Oxygen
208 kg Petroleum
208 kg Liquid Oxygen
30000
618 kg Petroleum
618 kg Liquid Oxygen
403 kg Petroleum
403 kg Liquid Oxygen
40000
813 kg Petroleum
813 kg Liquid Oxygen
598 kg Petroleum
598 kg Liquid Oxygen
50000
Unreachable
1010 kg Petroleum
1010 kg Liquid Oxygen
802 kg Petroleum
802 kg Liquid Oxygen
60000
1211 kg Petroleum
1211 kg Liquid Oxygen
Unreachable
1034 kg Petroleum
1034 kg Liquid Oxygen
70000
1430 kg Petroleum
1430 kg Liquid Oxygen
1268 kg Petroleum
1268 kg Liquid Oxygen
80000
1659 kg Petroleum
1659 kg Liquid Oxygen
1515 kg Petroleum
1515 kg Liquid Oxygen
90000
Unreachable
1916 kg Petroleum
1916 kg Liquid Oxygen
1780 kg Petroleum
1780 kg Liquid Oxygen
100000
2177 kg Petroleum
2177 kg Liquid Oxygen
Unreachable
2105 kg Petroleum
2105 kg Liquid Oxygen
110000
2462 kg Petroleum
2462 kg Liquid Oxygen
2451 kg Petroleum
2451 kg Liquid Oxygen
120000
Unreachable
2879 kg Petroleum
2879 kg Liquid Oxygen
Unreachable
3121 kg Petroleum
3121 kg Liquid Oxygen
130000
3329 kg Petroleum
3329 kg Liquid Oxygen
Unreachable
140000
Unreachable
Oxylite, Cargo Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Petroleum Engine
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
10000
332 kg Petroleum
332 kg Oxylite
43 kg Petroleum
43 kg Oxylite
20000
595 kg Petroleum
595 kg Oxylite
320 kg Petroleum
320 kg Oxylite
30000
882 kg Petroleum
882 kg Oxylite
638 kg Petroleum
638 kg Oxylite
40000
Unreachable
1223 kg Petroleum
1223 kg Oxylite
Unreachable
1014 kg Petroleum
1014 kg Oxylite
50000
1593 kg Petroleum
1593 kg Oxylite
1439 kg Petroleum
1439 kg Oxylite
60000
Unreachable
2092 kg Petroleum
2092 kg Oxylite
Unreachable
2156 kg Petroleum
2156 kg Oxylite
70000
Unreachable
Unreachable
Liquid Oxygen, Cargo Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Petroleum Engine
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
4 Liquid Fuel Tanks
2 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Petroleum Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
10000
247 kg Petroleum
247 kg Liquid Oxygen
32 kg Petroleum
32 kg Liquid Oxygen
20000
442 kg Petroleum
442 kg Liquid Oxygen
229 kg Petroleum
229 kg Liquid Oxygen
30000
637 kg Petroleum
637 kg Liquid Oxygen
447 kg Petroleum
447 kg Liquid Oxygen
40000
846 kg Petroleum
846 kg Liquid Oxygen
675 kg Petroleum
675 kg Liquid Oxygen
50000
Unreachable
1080 kg Petroleum
1080 kg Liquid Oxygen
Unreachable
929 kg Petroleum
929 kg Liquid Oxygen
60000
1316 kg Petroleum
1316 kg Liquid Oxygen
1187 kg Petroleum
1187 kg Liquid Oxygen
70000
1566 kg Petroleum
1566 kg Liquid Oxygen
1469 kg Petroleum
1469 kg Liquid Oxygen
80000
Unreachable
1862 kg Petroleum
1862 kg Liquid Oxygen
1784 kg Petroleum
1784 kg Liquid Oxygen
90000
2169 kg Petroleum
2169 kg Liquid Oxygen
Unreachable
2225 kg Petroleum
2225 kg Liquid Oxygen
100000
2529 kg Petroleum
2529 kg Liquid Oxygen
Unreachable
110000
Unreachable
3298 kg Petroleum
3298 kg Liquid Oxygen
120000
Unreachable
Hydrogen Engine
Oxylite, Research Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Hydrogen Engine
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
4 Liquid Fuel Tanks
2 Solid Oxidizer Tanks
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
4 Liquid Fuel Tanks
2 Solid Oxidizer Tanks
5 Research Modules
1 Command Capsule
10000
206 kg Liquid Hydrogen
206 kg Oxylite
16 kg Liquid Hydrogen
16 kg Oxylite
20000
378 kg Liquid Hydrogen
378 kg Oxylite
188 kg Liquid Hydrogen
188 kg Oxylite
30000
550 kg Liquid Hydrogen
550 kg Oxylite
361 kg Liquid Hydrogen
361 kg Oxylite
40000
723 kg Liquid Hydrogen
723 kg Oxylite
533 kg Liquid Hydrogen
533 kg Oxylite
50000
895 kg Liquid Hydrogen
895 kg Oxylite
720 kg Liquid Hydrogen
720 kg Oxylite
60000
Unreachable
1075 kg Liquid Hydrogen
1075 kg Oxylite
Unreachable
923 kg Liquid Hydrogen
923 kg Oxylite
70000
1266 kg Liquid Hydrogen
1266 kg Oxylite
1125 kg Liquid Hydrogen
1125 kg Oxylite
80000
1463 kg Liquid Hydrogen
1463 kg Oxylite
1337 kg Liquid Hydrogen
1337 kg Oxylite
90000
1667 kg Liquid Hydrogen
1667 kg Oxylite
1559 kg Liquid Hydrogen
1559 kg Oxylite
100000
Unreachable
1896 kg Liquid Hydrogen
1896 kg Oxylite
1795 kg Liquid Hydrogen
1795 kg Oxylite
110000
2125 kg Liquid Hydrogen
2125 kg Oxylite
Unreachable
2081 kg Liquid Hydrogen
2081 kg Oxylite
120000
2369 kg Liquid Hydrogen
2369 kg Oxylite
2373 kg Liquid Hydrogen
2373 kg Oxylite
130000
2636 kg Liquid Hydrogen
2636 kg Oxylite
Unreachable
2864 kg Liquid Hydrogen
2864 kg Oxylite
140000
Unreachable
3041 kg Liquid Hydrogen
3041 kg Oxylite
3511 kg Liquid Hydrogen
3511 kg Oxylite
150000
3460 kg Liquid Hydrogen
3460 kg Oxylite
Unreachable
160000
Unreachable
Liquid Oxygen, Research Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Hydrogen Engine
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
5 Research Modules
1 Command Capsule
10000
153 kg Liquid Hydrogen
153 kg Liquid Oxygen
12 kg Liquid Hydrogen
12 kg Liquid Oxygen
20000
282 kg Liquid Hydrogen
282 kg Liquid Oxygen
141 kg Liquid Hydrogen
141 kg Liquid Oxygen
30000
411 kg Liquid Hydrogen
411 kg Liquid Oxygen
269 kg Liquid Hydrogen
269 kg Liquid Oxygen
40000
539 kg Liquid Hydrogen
539 kg Liquid Oxygen
398 kg Liquid Hydrogen
398 kg Liquid Oxygen
50000
668 kg Liquid Hydrogen
668 kg Liquid Oxygen
526 kg Liquid Hydrogen
526 kg Liquid Oxygen
60000
796 kg Liquid Hydrogen
796 kg Liquid Oxygen
661 kg Liquid Hydrogen
661 kg Liquid Oxygen
70000
Unreachable
926 kg Liquid Hydrogen
926 kg Liquid Oxygen
800 kg Liquid Hydrogen
800 kg Liquid Oxygen
80000
1058 kg Liquid Hydrogen
1058 kg Liquid Oxygen
948 kg Liquid Hydrogen
948 kg Liquid Oxygen
90000
1195 kg Liquid Hydrogen
1195 kg Liquid Oxygen
Unreachable
1092 kg Liquid Hydrogen
1092 kg Liquid Oxygen
100000
1335 kg Liquid Hydrogen
1335 kg Liquid Oxygen
1240 kg Liquid Hydrogen
1240 kg Liquid Oxygen
110000
1478 kg Liquid Hydrogen
1478 kg Liquid Oxygen
1391 kg Liquid Hydrogen
1391 kg Liquid Oxygen
120000
1623 kg Liquid Hydrogen
1623 kg Liquid Oxygen
1545 kg Liquid Hydrogen
1545 kg Liquid Oxygen
130000
1771 kg Liquid Hydrogen
1771 kg Liquid Oxygen
1704 kg Liquid Hydrogen
1704 kg Liquid Oxygen
140000
Unreachable
1934 kg Liquid Hydrogen
1934 kg Liquid Oxygen
Unreachable
1886 kg Liquid Hydrogen
1886 kg Liquid Oxygen
150000
2092 kg Liquid Hydrogen
2092 kg Liquid Oxygen
2060 kg Liquid Hydrogen
2060 kg Liquid Oxygen
160000
2254 kg Liquid Hydrogen
2254 kg Liquid Oxygen
2241 kg Liquid Hydrogen
2241 kg Liquid Oxygen
170000
2422 kg Liquid Hydrogen
2422 kg Liquid Oxygen
2432 kg Liquid Hydrogen
2432 kg Liquid Oxygen
180000
(max distance in game)
2597 kg Liquid Hydrogen
2597 kg Liquid Oxygen
2635 kg Liquid Hydrogen
2635 kg Liquid Oxygen
Oxylite, Cargo Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Hydrogen Engine
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
4 Liquid Fuel Tanks
2 Solid Oxidizer Tanks
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Solid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
10000
223 kg Liquid Hydrogen
223 kg Oxylite
33 kg Liquid Hydrogen
33 kg Oxylite
20000
395 kg Liquid Hydrogen
395 kg Oxylite
220 kg Liquid Hydrogen
220 kg Oxylite
30000
569 kg Liquid Hydrogen
569 kg Oxylite
414 kg Liquid Hydrogen
414 kg Oxylite
40000
758 kg Liquid Hydrogen
758 kg Oxylite
614 kg Liquid Hydrogen
614 kg Oxylite
50000
963 kg Liquid Hydrogen
963 kg Oxylite
822 kg Liquid Hydrogen
822 kg Oxylite
60000
Unreachable
1167 kg Liquid Hydrogen
1167 kg Oxylite
Unreachable
1059 kg Liquid Hydrogen
1059 kg Oxylite
70000
1380 kg Liquid Hydrogen
1380 kg Oxylite
1295 kg Liquid Hydrogen
1295 kg Oxylite
80000
1605 kg Liquid Hydrogen
1605 kg Oxylite
1549 kg Liquid Hydrogen
1549 kg Oxylite
90000
Unreachable
1869 kg Liquid Hydrogen
1869 kg Oxylite
Unreachable
1873 kg Liquid Hydrogen
1873 kg Oxylite
100000
2136 kg Liquid Hydrogen
2136 kg Oxylite
2215 kg Liquid Hydrogen
2215 kg Oxylite
110000
2436 kg Liquid Hydrogen
2436 kg Oxylite
2665 kg Liquid Hydrogen
2665 kg Oxylite
120000
Unreachable
2960 kg Liquid Hydrogen
2960 kg Oxylite
Unreachable
130000
Unreachable
Liquid Oxygen, Cargo Vehicle
Expand/Collapse table
Destination Distance
[km]
1 Hydrogen Engine
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
4 Liquid Fuel Tanks
2 Liquid Oxidizer Tanks
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
1 Liquid Fuel Tank
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
2 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
3 Liquid Fuel Tanks
1 Liquid Oxidizer Tank
1 Cargo Bay
1 Command Capsule
1 Hydrogen Engine
1 Solid Booster
4 Liquid Fuel Tanks
2 Liquid Oxidizer Tanks
1 Cargo Bay
1 Command Capsule
10000
166 kg Liquid Hydrogen
166 kg Liquid Oxygen
25 kg Liquid Hydrogen
25 kg Liquid Oxygen
20000
295 kg Liquid Hydrogen
295 kg Liquid Oxygen
160 kg Liquid Hydrogen
160 kg Liquid Oxygen
30000
423 kg Liquid Hydrogen
423 kg Liquid Oxygen
299 kg Liquid Hydrogen
299 kg Liquid Oxygen
40000
552 kg Liquid Hydrogen
552 kg Liquid Oxygen
439 kg Liquid Hydrogen
439 kg Liquid Oxygen
50000
689 kg Liquid Hydrogen
689 kg Liquid Oxygen
583 kg Liquid Hydrogen
583 kg Liquid Oxygen
60000
828 kg Liquid Hydrogen
828 kg Liquid Oxygen
729 kg Liquid Hydrogen
729 kg Liquid Oxygen
70000
976 kg Liquid Hydrogen
976 kg Liquid Oxygen
878 kg Liquid Hydrogen
878 kg Liquid Oxygen
80000
Unreachable
1121 kg Liquid Hydrogen
1121 kg Liquid Oxygen
Unreachable
1044 kg Liquid Hydrogen
1044 kg Liquid Oxygen
90000
1270 kg Liquid Hydrogen
1270 kg Liquid Oxygen
1203 kg Liquid Hydrogen
1203 kg Liquid Oxygen
100000
1421 kg Liquid Hydrogen
1421 kg Liquid Oxygen
1367 kg Liquid Hydrogen
1367 kg Liquid Oxygen
110000
1577 kg Liquid Hydrogen
1577 kg Liquid Oxygen
1538 kg Liquid Hydrogen
1538 kg Liquid Oxygen
120000
1737 kg Liquid Hydrogen
1737 kg Liquid Oxygen
1716 kg Liquid Hydrogen
1716 kg Liquid Oxygen
130000
Unreachable
1920 kg Liquid Hydrogen
1920 kg Liquid Oxygen
Unreachable
1930 kg Liquid Hydrogen
1930 kg Liquid Oxygen
140000
2095 kg Liquid Hydrogen
2095 kg Liquid Oxygen
2133 kg Liquid Hydrogen
2133 kg Liquid Oxygen
150000
2278 kg Liquid Hydrogen
2278 kg Liquid Oxygen
2352 kg Liquid Hydrogen
2352 kg Liquid Oxygen
160000
2471 kg Liquid Hydrogen
2471 kg Liquid Oxygen
2593 kg Liquid Hydrogen
2593 kg Liquid Oxygen
170000
2678 kg Liquid Hydrogen
2678 kg Liquid Oxygen
Unreachable
3025 kg Liquid Hydrogen
3025 kg Liquid Oxygen
180000
(max distance in game)
Unreachable
2995 kg Liquid Hydrogen
2995 kg Liquid Oxygen
3451 kg Liquid Hydrogen
3451 kg Liquid Oxygen
