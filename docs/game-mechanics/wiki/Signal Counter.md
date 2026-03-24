# Signal Counter

Signal Counter
Counts how many times a
Green Signal
has been received up to a chosen number.
When the chosen number is reached it sends a
Green Signal
until it receives another
Green Signal
, when it resets automatically and begins counting again.
For numbers higher than ten connect multiple counters together.
Research
Computing
Dimensions
1×3 tiles
Rotation
Mirrored
Category
Automation
Decor
-5 (Radius: 1 tile)
Auto In
Green
: (1) Increment; (R) Set to 0
Red
: Nothing
Auto Out
Green
: Counter reached target
Red
: Counter less than target
Materials
Refined Metal
25 kg
Signal Counter
is an
Automation building
that can be used to count signals on a
wire
, or to display numbers dynamically on the map.
Mechanics
The Counter has two modes of operation, normal and advanced. Additionally, it has a target parameter that can be set between 1-10, including the boundaries. Its display can be reset to 0 via a UI button. This is equivalent to activating the reset input, except it can be done even when that input is a constant green.
The value displayed will increase by one when a green signal is received on the main input. In normal mode the Counter's output will turn green when the target value is displayed, and the next green signal will reset the display to one. In advanced mode the display will be reset to 0 instead of reaching the target, and the Counter's output will emit a green pulse. Activating the reset port or manually resetting will also trigger a green pulse in advanced mode, even when the display was already 0.
The maximum target value, 10, is displayed as 0. Multiple Counters can be chained together to display and count to a larger number. Rotating the Counter will not change the rotation of the number.
Wiring multiple counters together. Note the orientations.
Blueprints
Available blueprints
Brightslug Signal Counter
Cobalt Signal Counter
Petal Signal Counter
History
AP-395113
: Introduced.
AP-404823
: Added advance mode.
AP-408920
:
Counter Sensor defaults to the "off" anim state when first built.
Counter Sensor no longer sends Green Signal for its initial "zero" state, making daisy-chaining possible without requiring to reset.
U50-582745
: Fixed various logic buildings getting stuck in an uninitialized state, such as the Memory Toggle and Signal Counter.
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
