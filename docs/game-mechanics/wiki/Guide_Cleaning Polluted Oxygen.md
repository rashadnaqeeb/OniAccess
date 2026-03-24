# Guide/Cleaning Polluted Oxygen

Polluted Oxygen can be converted to
Liquid Oxygen
by cooling it down to its condensation point using an installation involving
Thermo Regulators
, and then reheating it. This guide describes an example system.
System described below has following characteristics:
Uses 2650 W of energy (though it scales up and down by adding/removing
Thermo Regulators
)
Can convert ~500 g/s of
Polluted Oxygen
(assuming
17 °C
/
62.6 °F
) into
Oxygen
Schematics
Polluted Oxygen Cleaning station
Important details:
The room on the left contains 50 kg/tile (350 kg total) of
Petroleum
. Because it has a high thermal conductivity (2 (W/m)/K) it ensures that
Thermo Regulators
will not overheat and nicely smooths out temperature differences. It also has a very low freezing point (
-57.1 °C
/
-70.78 °F
) which ensures that
Liquid Oxygen
of temperature
-183 °C
/
-297.4 °F
will not freeze it.
The room with
Hydrogen
should have slightly less pressure than 20 kg (I would suggest ~18 kg) - otherwise if it's too close to 20 kg, the vents will block and the system will not perform at full efficiency.
Use gold for most of the buildings (especially
Thermo Regulator
and
Liquid Pump
).
I built the metal wall from
Tungsten
and used
Diamond
Tempshift Plates
to ensure fast transfer of energy.
It is important to only input
Polluted Oxygen
to the system - putting other gasses might break the system.
Automation
The main rule is to enable
Thermo Regulators
when temperature of
Petroleum
in the left room is lower than
-45 °C
/
-49 °F
(to heat it and protect against freezing) OR when temperature of
Hydrogen
in the middle room got higher than
-200 °C
/
-328 °F
(to keep it at stable temperature that can condensate
Polluted Oxygen
).
There is also a safety part that completely shuts off the
Polluted Oxygen
input when:
Temperature of the left room is not between
-45 °C ↔ 30 °C
/
-49 °F ↔ 86 °F
Temperature of the middle room is lower than
-190 °C
/
-310 °F
All settings, assuming that the order is from left to right, and then from up to down:
Thermo Sensor
- (Above
-45 °C
/
-49 °F
)
Thermo Sensor
- (Below
30 °C
/
86 °F
)
Atmo Sensor
- (Above 2000 g)
Thermo Sensor
- (Above
-190 °C
/
-310 °F
)
Thermo Sensor
- (Above
-200 °C
/
-328 °F
)
Other settings:
Liquid Valve
- (1000 g) (it is probably not needed)
Gas Valve
- (500 g) - set depending on the expected temperature of the input gas and number of
Thermo Regulators
Building tips
Note: I haven't built it yet in normal mode, only debug, but this is how I plan to execute building it:
Build the middle and right rooms.
Fill the middle room with just hydrogen, and the room on the right should be empty
Place thermo regulators in cold environment - I would suggest
Icy Biomes
or in the room with
Anti Entropy Thermo-Nullifier
Cool down the hydrogen to ~
-200 °C
/
-328 °F
)
Build a left room, pump 50 kg/tile of
Petroleum
(ensure that
Thermo Regulators
are not flooded) and close it
And you are done, pump
Polluted Oxygen
into the system and enjoy clean
Oxygen
as output.
Math
TODO: explain the math behind the system
The
Thermo Regulators
, via the hydrogen, are responsible for removing energy from and condensing the
Polluted Oxygen
. If we are given the rate in which we want to condense
Polluted Oxygen
(ie, the clean
Oxygen
rate), then the number of
Thermo Regulators
can be calculated.
We do this by equating the energy rate required to cool the oxygen to the energy rate required to cool the hydrogen.
Polluted Oxygen:
Rate of Polluted Oxygen: 500 g/s
Temperature change of Polluted Oxygen: 17-(-183) = 203 K
Specific Heat of Polluted Oxygen: 1.01 = (W*s)/(g*K) or W = 1.01 * K * (g/s)
Watts from Polluted Oxygen: W = 1.01 * 203 * 500 = 102515
Hydrogen:
Temperature change of Hydrogen: 14 K
Specific Heat of Hydrogen: 2.4 = (W*s)/(g*K) or (g/s) = W/(2.4*K)
Rate of Hydrogen: (g/s) = 102515/(2.4*14) = 3051.04
Thermo Regulator:
Thermo Regulator = 1000 g/s
3051.04 g/s * (1 Thermo Regulator)/(1000 g/s) = 3.05 Thermo Regulators
