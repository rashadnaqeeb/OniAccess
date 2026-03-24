# BUFFER Gate

BUFFER Gate
Outputs a
Green Signal
if the Input is receiving a
Green Signal
.
Continues sending a
Green Signal
for an amount of buffer time after the Input receives a
Red Signal
.
This gate continues outputting a Green Signal for a short time after the gate stops receiving a Green Signal.
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
BUFFER Gate
is an
Automation
Building
that outputs a Green Signal when its input is Green, and momentarily continues to output Green when its input receives Red. It is connected to other buildings via
Automation Wire
.
The amount of time the BUFFER Gate remains Green can be adjusted by the player, and can range from 0.1 – 200 seconds. When the input becomes Green again, the timer resets to the designated amount, and does not add to the current remaining time (if any).
Automation Ribbon
The BUFFER Gate's behavior when connected to an
Automation Ribbon
is not bitwise like most gates. The gate instead merges all bits in the Automation Ribbon like an
OR Gate
into a single bit and then acts as usual on this bit.
For example, when the input is 4-Bit
RR
G
R
, then it is interpreted as a 1-Bit
Green Signal
.
Tips
The BUFFER Gate, like other Automation gates, can be placed behind buildings and tiles, as well as in the same space as pipes or wires.
The state of a Green BUFFER Gate persists through game save / load (this was not always the case)
Blueprints
Available blueprints
Brightslug BUFFER Gate
Cobalt BUFFER Gate
Petal BUFFER Gate
Bugs
Like all time-based sensors, it is very imprecise for large durations measurement at high cycle count.
[
1
]
See Also
FILTER Gate
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
