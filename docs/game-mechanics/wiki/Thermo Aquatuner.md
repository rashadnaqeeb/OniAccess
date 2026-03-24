# Thermo Aquatuner

Thermo Aquatuner
Cools the Liquid piped through it, but outputs Heat in its immediate vicinity.
A thermo aquatuner cools liquid and outputs the heat elsewhere.
Research
Liquid Tuning
Dimensions
2×2 tiles
Rotation
Mirrored
Category
Utilities
Power
-1.2 kW
Overheat
at
125 °C
/
257 °F
Decor
0
Piping
Input: Liquid
Output: Liquid
Effects
Heat: Varies
Cooling factor: -14 °C
Auto In
Green
: Enable
Red
: Disable
Materials
Metal Ore
1,200 kg
Thermo Aquatuner
cools the
liquid
piped through it and absorbs the
Heat
itself.
If the environment around the Thermo Aquatuner cannot absorb its heat, it will quickly overheat. Due to its high heat output it is recommended to submerge it in a liquid, as gasses cannot exchange temperatures with it fast enough when the machine is working full time.
Use of
Steel
or
Thermium
Due to the massive amounts of heat that this device can remove from its input fluid and transfer to itself, the choices of materials for this device is very limited if
Overheating
is to be avoided. In the early and mid-game, the most reliable material is
Steel
or
Iridium
. In the late game,
Thermium
is the best choice.
Use of
Thermo Sensor
A
Thermo Sensor
is also very much recommended - indeed,
required
- for efficient use and control of this device. Setting the sensor close or next to the Aquatuner is wise; this can give a measure of both the temperature of the Aquatuner and the fluids that it is immersed in. If the Aquatuner is made of
Steel
,
Iridium
, or
Thermium
, the heat transfer will be fast enough to reliably control the temperature of the Aquatuner and the fluids that are soaking heat from it.
Mechanics
The Thermo Aquatuner (as well as the
Thermo Regulator
) works by taking heat from anything piped into it and transferring that heat to itself, and does not produce its own heat. As this is heat-neutral, cooling it with its own output will cause it to maintain its temperature, assuming no other heat sources or sinks are present.
The amount of heat moved depends on the type and amount of liquid piped in. Each packet of liquid has 14 °C removed from it, regardless of the Specific Heat Capacity (SHC) of the fluid or the amount. It is therefore best to use liquids with a high SHC (
Water
,
Polluted Water
,
Super Coolant
) and to ensure all packets sent in are 10 kg (it consumes 1.2 kJ per packet, not per 10 kg), in order to make the most of the 1.2 kW power requirement.
The Aquatuner does not respect freezing temperatures of liquids, and will damage the output pipe if liquids are cooled below their freezing temperature.
The Aquatuner is the only building capable of reliably producing liquid oxygen and hydrogen as it has a minimum output temperature of
-272 °C
/
-457.6 °F
. The Thermo Regulator also has the same minimum temperature, but there is no gas suitable for cooling hydrogen to the necessary temperatures.
Common liquids used as coolant (assumes 10 kg packets)
Name
SHC
(DTU/°C)/g
TC
DTU/m/s/°C
Heat moved
DTU/s
Energy Efficiency
DTU/W
Self heating
°C/s for
Steel
Freezing
point
Vaporization
point
Crude Oil
1.690
2.000
236,600
197.17
2.012
-40.15 °C
/
-40.27 °F
399.85 °C
/
751.73 °F
Petroleum
1.760
2.000
246,400
205.33
2.095
-57.15 °C
/
-70.87 °F
538.85 °C
/
1001.93 °F
Naphtha
2.191
0.200
306,740
255.62
2.608
-50.15 °C
/
-58.27 °F
538.85 °C
/
1001.93 °F
Ethanol
2.460
0.171
344,400
287.00
2.928
-114.05 °C
/
-173.29 °F
78.35 °C
/
173.03 °F
Brine
3.400
0.609
476,000
396.67
4.048
-22.5 °C
/
-8.5 °F
102.75 °C
/
216.95 °F
Salt Water
4.100
0.609
574,000
478.33
4.881
-7.5 °C
/
18.5 °F
99.69 °C
/
211.44 °F
Nectar
4.100
0.609
574,000
478.33
4.881
-82.5 °C
/
-116.5 °F
160 °C
/
320 °F
Water
4.179
0.609
585,060
487.55
4.975
-0.65 °C
/
30.83 °F
99.35 °C
/
210.83 °F
Polluted Water
4.179
0.580
585,060
487.55
4.975
-20.65 °C
/
-5.17 °F
119.35 °C
/
246.83 °F
Nuclear Waste
7.440
6.000
1,041,600
868.00
8.857
26.85 °C
/
80.33 °F
526.85 °C
/
980.33 °F
Super Coolant
8.440
9.460
1,181,600
984.67
10.048
-271.15 °C
/
-456.07 °F
436.85 °C
/
818.33 °F
Formula for Heat Moved (assuming 10kg packets of liquid) is Heat Moved = SHC × 14°C × 10,000g. For example, for
Nuclear Waste
, Heat Moved = 7.440 × 14 × 10,000 = 1,041,600 DTU/s.
Heat removed from the liquid will heat up the Aquatuner itself, and if it is not sufficiently cooled down, it will overheat and take damage. The amount of self heat will depend on the liquid used and the metal used to build the Aquatuner. The formula for Self Heating = Heat Moved / (mass of Aquatuner × SHC of metal used / 5). Note the mass of the Aquatuner is fixed at 1,200,000g and as a building it has its thermal mass divided by 5. For example, the formula for Self Heating °C/s for a steel Aquatuner using water: Self Heating = Heat Moved (585,060) / (Mass of Aquatuner (1,200,000) × SHC of steel (0.49) / 5) = 585,060 / 117,600 = 4.975 °C/s.
Any liquid can be used, but these are most common as they have desirable properties;
Super Coolant
has the largest heat capacity. Thermal conductivity is listed above, but has no effect on the operation of the Thermo Aquatuner; it is important only when the coolant exchanges heat with the environment. Notably, buildings that dump heat into a coolant piped into them, such as the
Metal Refinery
, ignore thermal conductivity.
Note: There is a lower temperature limit of
-272.1 °C
/
-457.78 °F
.
Super Coolant
is the only liquid capable of reaching such a temperature without freezing. However, the Thermo Aquatuner will continue to operate, consuming 1200 W and generating no heat in the process.
Interaction with Steam Turbine
Steam Turbines
produce approximately 0.969W of power per raw kDTU/s of heat deleted, and approximately 1.082W per real kDTU/s of heat deleted if running at 200 °C (factoring in the heat transferred to the turbine itself, assuming this heat is subsequently reintroduced to the steam chamber either via an aquatuner or self-cooling with the turbine's output).  In order for an aquatuner in a steam chamber to fund its own power consumption, it needs to transfer at least enough heat from the fluid it is cooling to the surrounding steam to match this power to heat ratio.  As an aquatuner consumes 1200W, this means it needs to transfer at least 1109.41 kDTU/s.  If the aquatuner is cooling a full pipe of 10 kg/s of liquid, this means it needs to be extracting roughly 110.41 kDTU/kg of liquid.  As it is reducing the temperature of the liquid by 14 °C, this is only possible with a fluid with a Specific Heat Capacity (SHC) of at least 7.92.
The only fluid for which this is true is
Super Coolant
, with an SHC of 8.44.
Liquid Nuclear Waste
falls barely under that threshold, at 7.44 (and generally shouldn't be run through aquatuners anyway, since nuclear waste often leaks and damages the aquatuner).  If using
Water
, with an SHC of 4.179, as the fluid being cooled by the aquatuner, the expected heat transfer to the aquatuner would be 585.06 kDTU/s, meaning a steam turbine would be expected to recoup slightly more than half (~633W) of the 1200W power consumption of the aquatuner.  The practical effect of this is that any aquatuner + steam turbine(s) setup that is cooling anything except Super Coolant will require either external power input (~567W if cooling water) or an alternate heat source (such as a
Steam Vent
or
Metal Volcano
) adding at least enough additional heat to the steam chamber to offset the remaining power consumption of the aquatuner (~524 kDTU/s if cooling water).  As an aquatuner consumes 1200W, versus the maximum 850W output of the steam turbine, at least two turbines are needed to power the aquatuner with its own heat output even if using Super Coolant.
Engie's Tune-Up
changes this math considerably, however.  It increases the power output of the Steam Turbine to 1275W without changing its steam consumption or heat deletion behavior, allowing it to generate up to 1.622W per kDTU/s real heat deleted.  This lowers the required SHC of the cooled fluid for power-neutrality to only 5.283.  While this is still not enough for water to be power-neutral, it does reduce the net power deficit when cooling water from ~567W to ~251W (or alternatively, ~155 kDTU/s additional heat input to the steam chamber).  It also means that only a single steam turbine is required to fully power the aquatuner's 1200W input.
Caution on the Use of
Thermium
and
Super Coolant
Both high-end manufactured materials vastly increases the Aquatuner's ability and efficiency at exchanging heat - but also vastly changes the amounts of heat involved. Using these materials require more careful control of not just the temperatures of the Aquatuner and the fluids that it is immersed in, but also what the fluids and Aquatuner is drawing heat from. It is very easy to excessively cool a region without this additional care.
Use to melt metal
The Aquatuner takes about 1.25 cycles (750 seconds) to completely become disabled from overheating.
Therefore, if an Aquatuner is used to cool a full pipe of
Water
or
Polluted Water
continuously while sitting in
Vacuum
, it will melt before the damage from overheating breaks it. It is recommended to disable auto-repair to avoid wasting metal. Note that since buildings have 1/5 the Specific Heat Capacity of the material they are made of, the heat in the resulting liquid metal will be 5 times what was displaced by the Aquatuner. This trick is particularly useful to melt
Steel
, since
Liquid Steel
is used for a wide array of advanced industrial processes.
Tips
Despite the similar SHC,
Nectar
and
Polluted Water
should be picked over
Water
for the
-82.5 °C
/
-116.5 °F
and
-20.65 °C
/
-5.17 °F
freezing points, respectively. Maintenance on burst pipes tends to be very tricky, so the low freezing point provides much more wiggle room when trying to balance the coolant receiver.
While
Nuclear Waste
can be used to move very large amounts of heat, care must be taken as to not let it go below the relatively high freezing point of
26.85 °C
/
80.33 °F
, and the pipe network doesn't stop. When stops, the nuclear waste becomes "contained" in the Aquatuner, which will make it leak from the Aquatuner.
Compared to the
Thermo Regulator
, the Aquatuner is more power-efficient. However it has a higher power draw, overloading early power grids (making the regulator a highly niche early game cooler)
It uses 120W per kg of material  (compared to Regulator's 240W per kg)
Larger Packet sizes: Liquids(10kg) vs Gases (1kg)
It cools the packet
14 °C
/
57.2 °F
regardless of packet density
Liquids have higher
SHC
then Gases
Water
:
4.179
DTU
g
∘
C
/
2.32
DTU
g
∘
F
Hydrogen
:
2.4
DTU
g
∘
C
/
1.33
DTU
g
∘
F
Blueprints
Available blueprints
Regal Neutronium Thermo Aquatuner
See also
Liquid Tepidizer
Steam Turbine
Thermo Regulator
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
Ice Liquefier
Ice Maker
Ice-E Fan
Liquid Tepidizer
Oil Well
Ore Scrubber
Space Heater
Sweepy's Dock
Tempshift Plate
Thermo Aquatuner
Thermo Regulator
Wood Heater
see all buildings
