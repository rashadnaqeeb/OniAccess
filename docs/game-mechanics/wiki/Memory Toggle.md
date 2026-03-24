# Memory Toggle

Memory Toggle
Contains an internal Memory, and will output whatever signal is stored in that Memory. Signals sent to the Inputs
only
affect the Memory, and do not pass through to the Output.
Sending a
Green Signal
to the Set Port(S) will set the memory to
Green
.
Sending a
Green Signal
to the Reset Port(R) will reset the memory back to
Red
.
A Memory stores a Green Signal received in the Set Port (S) until the Reset Port (R) receives a Green Signal.
Research
Computing
Dimensions
2×2 tiles
Rotation
Turnable
Category
Automation
Decor
0
Materials
Refined Metal
25 kg
Memory Toggle
is an
Automation
Building
that takes in two inputs and produces an output. It is connected to other buildings via
Automation Wire
. Unlike other Automation buildings the Memory Toggle will maintain its output state after the input signal goes to Red until a new input signal becomes Green.
Truth Table
Set Input
Reset Input
Output
Red
Red
unchanged
Green
Red
Green
Red
Green
Red
Green
Green
Red
[
1
]
Usage
A typical usage of Memory involves two signal inputs and four phases.
An activation signal connecting to the Set Port(S).
A deactivation signal connecting to the Reset Port(R).
Phase 1: Activation.
The activation signal is Green, and the deactivation signal is Red. The Memory Toggle outputs Green.
Phase 2: Action.
The activation signal is Red, and the deactivation signal is Red. The Memory Toggle continues outputting Green.
Phase 3: Deactivation.
The activation signal is Red, and the deactivation signal is Green. The Memory Toggle outputs Red.
Phase 4: No-action.
The activation signal is Red, and the deactivation signal is Red. The Memory Toggle continues outputting Red.
After the phase 4 it goes back to phase 1.
Or it could be explained by below time sequence.
Port
Time Sequence
Set
000
111
000
000000
Reset
000000000
111
000
Output
000
111111
000000
Automation Ribbon
Memory Toggle's behavior to
Automation Ribbon
is same as
Automation Wire
activation. Generally interpreting 4-Bit Signal into 1-Bit Signal is only use first Bit - For example, when input is 4-Bit
R
GGG
, then it is interpreted as 1-Bit
Red Signal
.
Tips
The Memory Toggle, like other Automation gates, can be placed behind buildings and tiles, as well as in the same space as pipes or wires.
Like the
AND
and
OR
gates, the fourth space on the Memory Toggle's graphic does nothing, and Automation Wires can cross freely in this space.
Blueprints
Available blueprints
Brightslug Memory Toggle
Cobalt Memory Toggle
Petal Memory Toggle
History
U50-582745
: Bugfix where it could get stuck.
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
This is only allowed when the
Memory Toggle
is
SR AND-OR latch
