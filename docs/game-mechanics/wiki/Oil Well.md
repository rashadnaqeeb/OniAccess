# Oil Well

Oil Well
Extracts Crude Oil using clean Water.
Must be built atop an Oil Reservoir.
Water pumped into an oil reservoir cannot be recovered.
Research
Plastic Manufacturing
Dimensions
4×4 tiles
Category
Utilities
Power
-240 W
Heat
+
2 kDTU/s
Overheat
at
2000 °C
/
3632 °F
Decor
0
Piping
Input: Water
Requires
Water
: -1 kg/s
Storage Capacity
Natural Gas 80 kg
Water 10 kg
Effects
Crude Oil
: +3.33 kg/s @
90 °C
/
194 °F
+
Natural Gas
: +33.3 g/s @
300 °C
/
572 °F
Auto In
Green
: Enable
Red
: Disable
Materials
Refined Metal
200 kg
Oil Well
can be built atop an
Oil Reservoir
. It produces
Crude Oil
when supplied with water, however, water pumped into an oil reservoir cannot be recovered. Produced oil is at
90 °C
/
194 °F
, or higher if the input water was above
90 °C
/
194 °F
.
Oil Well also generates
Natural Gas
at a rate of 20 kg per cycle (33.3 g/s) and stores up to 80 kg of it (100% back-pressure). The gas is produced at temperatures of
300 °C
/
572 °F
and adds to the building's heat footprint. Both outputs are dumped into the environment at the bottom cells, gas at the second from the left, oil at the rightmost cell. It is a good idea to put
Tempshift Plates
on them.
Releasing Backpressure
When an oil well's backpressure reaches the player-set backpressure release threshold, It will require
Duplicant
operation
to release
Natural Gas
into the environment. The gas comes out at
300 °C
/
572 °F
, and can be released at any pressure, as wells cannot become overpressurized, so the duplicant releasing said gas should wear an
exosuit
to avoid getting scalded and
stressed
. Releasing pressure also pauses oil extraction and can turn water inside Oil Well into
Steam
which can contaminate the gas output, and potentially liquid output as well, if not performed
fast enough
. As Oil Wells do not mind being submerged, it is beneficial to keep some oil around around the well, 1-2 tiles vertically, so it can soak up the heat and prevent vaporization of the water inside the well.
The release rate is ~444.44g/s base. This is also affected by machining skill and lit workspace buff, however these stack additively rather then multiplicatively.
Alternatively, the gas can be sucked away with a
Gas Pump
; using an
Atmo Sensor
to automate the pump. This is recommended if the Oil Well is sealed in a pocket with
Tiles
and
Mechanized Airlocks
. The release of the gas will build up air pressure, thus triggering the pump to draw away the gas before they overheat things too much.
Note: if you plan to fuel
Natural Gas Generator
s with Natural Gas produced by Oil Wells, remember that those generators will release
Carbon Dioxide
at
110 °C
/
230 °F
+ and
Polluted Water
at
40 °C
/
104 °F
+, and that Natural Gas produced by Oil Wells is around
300 °C
/
572 °F
. That means that if the Natural Gas produced by the wells is allowed to heat up the generators above
122.35 °C
/
252.23 °F
(vaporization point of Polluted Water +3 °C), the outputted polluted water will immediately vaporize and turn itself into
Steam
(vaporization of Polluted Water also produces
Dirt
at 1% of the vaporized amount, but not if the amount of Dirt which would've been produced is under 1 g, and because there are only 67.5 grams of polluted water 67.5 / 100 = 0.675 g < 1 g, there won't be any Dirt produced in this case), and the generator may overheat if built from the wrong
Material
. You can easily cool down Natural Gas released by Oil Wells to temperatures
95 °C
/
203 °F
by making it exchange its heat with Crude Oil produced by the wells (which is emitted at
90 °C
/
194 °F
+), allowing it to be handled more easily.
Water Recovery
Usage of oil can indirectly lead to production of  water through several methods. Using
Oil Refinery
produces
Petroleum
and Natural Gas from oil, which then can be used in
Petroleum Generator
and
Natural Gas Generator
to produce
Polluted Water
. The Refinery converts only 50% of the mass of the oil into petroleum, and 9% to natural gas,  and the generators convert 37.5% of the mass of petroleum to polluted water (overall producing 62.5% the mass of the original water) and 75% of the mass of natural gas to water (producing 22.5% the mass of the original water) respectively. Even including the natural gas emitted by the oil well (which yields 2.25% the mass of the original water), this means a total loss of 12.75% of the water entering the oil well.
However, if the
Carbon Dioxide
output of the above system is used to ranch
Slicksters
, it increases the oil in the system by roughly 3.33% overall and reduces the overall water loss to approximately 7%. With molten slicksters, the amount of petroleum in the system can be increased by roughly 14.28%, reducing the loss to roughly 3.82%.
Oil can be directly converted to petroleum without loss of mass by heating it to
400 °C
/
752 °F
. This allows for a net increase in polluted water production. Using this method, Petroleum Generators will produce 125% as much polluted water as the water fed to the oil well (with an extra 2.25% from natural gas generators still). However, heat-producing machines can only withstand these heats if built with space materials, so before producing
rockets
, this rate of production can only be achieved using a natural heat source like
magma
or a
volcano
, or using a
Metal Refinery
.
An even more advanced method for maximizing water and energy efficiency is the conversion of oil into natural gas at 67% yield via the
Sour Gas
-
Methane
pathway. Oil is heated past
400 °C
/
752 °F
where it turns into petroleum, then to
539 °C
/
1002.2 °F
where it turns into sour gas. The sour gas is then cooled below
-162 °C
/
-259.6 °F
to turn it into 67% methane and 33% sulfur. The methane is then re-heated above
-161 °C
/
-257.8 °F
, turning into natural gas. Burning the natural gas yields 167% as much polluted water as the water fed to the oil well. If efficiently laid out, it also produces significantly more energy than a petroleum based system as the energy density of natural gas (8.89 J/g) is significantly higher than that of petroleum (1 J/g).
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
