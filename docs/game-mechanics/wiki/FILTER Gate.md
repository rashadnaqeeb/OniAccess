# FILTER Gate

FILTER Gate
Only lets a
Green Signal
through if the Input has received a
Green Signal
for longer than the selected filter time.
Will continue outputting a
Red Signal
if the
Green Signal
did not last long enough.
This gate only lets a Green Signal through if its Input has received a Green Signal that last longer than the selected filter time.
Research
Advanced Automation
Dimensions
2×1 tiles
Rotation
Turnable
Category
Automation
Decor
-5 (Radius: 1 tile)
Materials
Refined Metal
25 kg
FILTER Gate
is an
Automation
Building
that outputs Red while its input receives Red, and momentarily remains in Red when its input receives Green before becoming Green itself. It is connected to other buildings via
Automation Wire
.
The amount of time the FILTER Gate remains on Red can be adjusted by the player, and can range from 0.1 - 200 seconds. Whenever the input goes into Red, the FILTER Gate will also go into Red and the time until it becomes Green resets to the designated amount, regardless of the gate's current state. The FILTER Gate is the logical opposite of the
BUFFER Gate
.
Automation Ribbon
FILTER Gate's behavior to
Automation Ribbon
is a little weird. Generally interpreting 4-Bit Signal into 1-Bit Signal is only use first Bit, but it is interpreted as every Bit is passed through
OR Gate
when FILTER Gate input is 4-Bit - for example, when 4-Bit input is
RR
G
R
, then it is interpreted as
Green Signal
.
Tips
The FILTER Gate, like other Automation gates, can be placed behind buildings and tiles, as well as in the same space as pipes or wires.
The state of a Green FILTER Gate persists through game save / load (this was not always the case).
Blueprints
Available blueprints
Brightslug FILTER Gate
Cobalt FILTER Gate
Petal FILTER Gate
Bugs
Like all time-based sensors, it is very imprecise for large durations measurement at high cycle count.
[
1
]
See Also
BUFFER Gate
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
AND Gate
Atmo Sensor
Automated Notifier
Automation Broadcaster
Automation Receiver
Automation Ribbon
Automation Ribbon Bridge
Automation Wire
Automation Wire Bridge
BUFFER Gate
Critter Sensor
Cycle Sensor
Duplicant Checkpoint
Duplicant Motion Sensor
FILTER Gate
Gas Element Sensor
Germ Sensor
Hammer
Hydro Sensor
Light Sensor
Liquid Element Sensor
Memory Toggle
NOT Gate
OR Gate
Radbolt Sensor
Radiation Sensor
Ribbon Reader
Ribbon Writer
Signal Counter
Signal Distributor
Signal Selector
Signal Switch
Space Scanner
Thermo Sensor
Timer Sensor
Wattage Sensor
Weight Plate
XOR Gate
see all buildings
↑
https://forums.kleientertainment.com/klei-bug-tracker/oni/big-issue-with-filter-gate-gamebreaker-r24768/
