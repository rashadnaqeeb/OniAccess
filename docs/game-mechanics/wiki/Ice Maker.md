# Ice Maker

Ice Maker
Converts Water into Ice or Snow.
Ice makers can be used as a small renewable source of ice and snow.
Research
Temperature Modulation
Dimensions
2×3 tiles
Category
Utilities
Power
-240 W
Heat
+
16 kDTU/s
Overheat
at
75 °C
/
167 °F
Decor
0
Requires
Water
: 60 kg per use
Storage Capacity
Water
: 60 kg
Ice
or
Snow
: 300+ kg
Effects
Ice
or
Snow
: 60 kg per use @
-20 °C
/
-4 °F
Auto In
Green
: Enable
Red
: Disable
Other Properties
Susceptible to
flooding
Materials
Metal Ore
400 kg
Ice Maker
is a machine that freezes
water
into
ice
or
snow
. It will relocate the heat energy from the water until the water is
-20 °C
/
-4 °F
ice/snow. The ice maker heats up significantly but the total heat output is negative ~60 to ~64 kDTU/s.
This machine has 2 phases when working. At first it takes 3.5 seconds to spin up. After that, begins the main cooling process which takes however long is needed for water to be frozen to the target temperature. The cooling rate of the second stage is constant at
0.32 °C
/
32.58 °F
/s and does not depend on water's initial temperature. However, warmer water will take longer to cool down. Power is drawn during both phases.
Therefore, overall cooling efficiency will be different on case by case basis as a direct result of the machine using power and de facto idling but not cooling during its first phase of operation.
It will start new production cycles as long as there is less than 300 kg of ice or snow in storage.
Heat deletion
The Ice Maker cools 60 kg of Water at a rate of 0.32 K/s (°C/s).
We calculate the heat change in water as SHC · ΔT · mass:
4
.
1
7
9
D
T
U
g
⋅
∘
C
⋅
−
0
.
3
2
∘
C
s
⋅
6
0
k
g
=
8
0
.
2
4
k
D
T
U
s
The Ice Maker produces +16 kDTU/s this means it deletes ~64.24 kDTU/s while it's running.
Accounting for the heat it releases, the Ice Maker net cooling per watt is ~267.65 DTU/Ws.
Real efficiency
Let's consider 2 fringe cases:
Water is 100°C. Cooling that water will take 3.5 s + (120°C / 0.32°C/s) = 378.5 s.
During that time the machine removed 30 088.8 kDTU. Thus, real heat deletion is ~79.48 kDTU/s, accounting for machine's heat generation ~63.48 kDTU/s. Understandably, very close to the rate during cooling phase alone.
Cooling power per watt is ~264.56 DTU/Ws.
Water is 0°C. Cooling will take 3.5 s + (20°C / 0.32°C/s) = 66 s.
Heat removed is 5 014.8 kDTU, meaning a heat deletion rate of 75.98 kDTU/s, accounting for machine's heat generation ~59.98 kDTU/s.
Cooling power per watt is ~249.92 DTU/Ws.
Note: (DTU/s)/W = DTU/Ws = DTU/J.
In conclusion, feeding hotter water allows for higher average heat removal and higher power efficiency. Hot water allows for less spin up phases and longer cooling phases. The difference in cooling is 3.5 kDTU/s which in absolute numbers isn't meaningless. However relatively it's ~5.5%.
Comparisons
In comparison the
Thermo Aquatuner
would cool 10 kg/s of (polluted) water by 14 K (°C) and consume 1200 W for 487.55 DTU/Ws. This is a heat transfer form the water to the Thermo Aquatuner so net heat deletion is 0 DTU/Ws.
Pairing a Thermo Aquatuner cooling water with a
Steam Turbine
can delete ~923.83 DTU/Ws. It's always the Steam Turbine that deletes heat, Thermo Aquatuner merely moves heat; it neither deletes or creates any itself.
Note that the Ice maker does not respect the usual hysteresis of water's freezing temperature: the
water
it contains will turn to
ice
/
snow
at exactly
0.6 °C
/
33.08 °F
.
This can be helpful early game. Especially since the heat can be removed from specific areas easily by moving the ice/snow into the vicinity and it's an easy way to turn duplicant labor into heat deletion.
Trivia
Before the "Automation Update" several years ago, the Ice Maker used to have no overheat temperature, so it could be used to achieve high temperatures. However, this has been addressed in a patch for the Automation Update, and it now has an overheat temperature.
History
AP-419840
:
Ice Maker
is now overheatable like other production buildings
U52-622222
: The Ice Maker can now optionally produce Snow. Increased the load capacity, cooling strength, and power consumption of the Ice Maker, but reduced relative heat production.
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
