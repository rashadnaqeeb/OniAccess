# Automation (Building)

Automation
buildings
are used to turn on and off buildings in the player's base using
Boolean Logic
and
Sensors
.
List of Automation buildings
Connections
Name
Description
Automation
Dimensions
Materials
Research
Automation Broadcaster
Broadcasts Automation signals across space to other planetoids in range.
Input
Green
: Broadcasts a green signal to reciever(s)
Red
: Broadcasts a red signal to receivers(s)
1 x 1 tiles
100
kg
Sensitive Microimaging
Automation Receiver
Receives automation signals from an Automation Broadcaster.
Output
Green
: Green signals sent from a broadcaster
Red
: Red signals sent from a broadcaster
1 x 1 tiles
100
kg
Sensitive Microimaging
Automation Ribbon Bridge
Runs one
Automation Ribbon
section over another without joining them.
Can be run through wall and floor tile.
Wire bridges allow multiple automation grids to exist in a small area without connecting.
3 x 1 tiles
25
kg
Parallel Automation
Automation Ribbon
A 4-Bit
Automation Wire
which can carry up to four automation signals.
Use a
Ribbon Writer
to output to multiple Bits, and a
Ribbon Reader
to input from multiple Bits.
Logic ribbons use significantly less space to carry multiple automation signals.
1 x 1 tiles
25
kg
Parallel Automation
Automation Wire Bridge
Runs one
Automation Wire
section over another without joining them.
Can be run through wall and floor tile.
Wire bridges allow multiple automation grids to exist in a small area without connecting.
3 x 1 tiles
5
kg
Smart Home
Automation Wire
Connects buildings to
Sensors
.
Can be run through wall and floor tile.
Automation wire is used to connect building ports to automation gates.
1 x 1 tiles
5
kg
Smart Home
Light Sensor
Sends a
Green Signal
or a
Red Signal
when ambient
Brightness
enters the chosen range.
Light sensors can tell surface bunker doors above solar panels to open or close based on solar light levels.
Output
Green
: Ambient brightness within chosen range
Red
: Ambient brightness out of chosen range
1 x 1 tiles
25
kg
25
kg
Generic Sensors
Radbolt Sensor
Sends a Green Signal or a Red Signal when Radbolts detected enters the chosen range.
Output
Green
: When the condition is met
Red
: When the condition is not met
1 x 1 tiles
25
kg
Radiation Protection
Radiation Sensor
Sends a Green automation signal when ambient rads enter the chosen range.
Output
Green
: When ambient Rads enters the desired range.
Red
: When ambient radiation is outside of the desired range.
1 x 1 tiles
25
kg
Micro-Targeted Medicine
Ribbon Reader
Reads a
Green Signal
or a
Red Signal
from the specified Bit of an
Automation Ribbon
onto an
Automation Wire
.
Inputs the signal from a single Bit in an
Automation Ribbon
into an
Automation Wire
.
Input
Green
: Write a
Green Signal
to the connected
Automation Wire
Red
: Write a
Red Signal
to the connected
Automation Wire
Output
Green
:
Green Signal
is read from the selected Bit of the attached
Automation Ribbon
Red
:
Red Signal
is read from the selected Bit of the attached
Automation Ribbon
2 x 1 tiles
25
kg
Parallel Automation
Ribbon Writer
Writes a
Green Signal
or a
Red Signal
to the specified Bit of an
Automation Ribbon
Automation Ribbon
must be used for the output wire to avoid overloading.
Translates the signal from an
Automation Wire
to a single Bit in an
Automation Ribbon
.
Input
Green
: Write a
Green Signal
to the selected Bit of the connected
Automation Ribbon
Red
: Write a
Red Signal
to the selected Bit of the connected
Automation Ribbon
Output
Green
:
Green Signal
is read from the attached
Automation Wire
Red
:
Red Signal
is read from the attached
Automation Wire
2 x 1 tiles
25
kg
Parallel Automation
Signal Distributor
Control A toggles between the outputs in the selector switches. Control B toggles between the two selector switches to determine which output receives a
Green
or
Red
signal from the input.
Up to four outputs can be toggled to determine which recieves the signal from one input.
3 x 4 tiles
25
kg
Multiplexing
Signal Selector
Control A toggles between the inputs in the selector switches. Control B toggles between the switches to determine which input sends a
Green
or
Red
signal to the output.
Up to four separate inputs can be toggled to determine which sends the signal to one output.
3 x 4 tiles
25
kg
Multiplexing
Logic gates
Logic gates
cost a uniform 25 kg of
Refined Metal
.
Name
Description
Dimensions
Research
AND Gate
Outputs a
Green Signal
when both Input A AND Input B are receiving
Green
.
Outputs a
Red Signal
when even one Input is receiving
Red
.
This gate outputs a Green Signal when both its inputs are receiving Green Signals at the same time.
2 x 2 tiles
Advanced Automation
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
2 x 1 tiles
Advanced Automation
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
2 x 1 tiles
Advanced Automation
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
2 x 2 tiles
Computing
NOT Gate
Outputs a
Green Signal
if the Input is receiving a
Red Signal
.
Outputs a
Red Signal
when its Input is receiving a
Green Signal
.
This gate reverses automation signals, turning a Green Signal into a Red Signal and vice versa.
2 x 1 tiles
Generic Sensors
OR Gate
Outputs a
Green Signal
if at least one of Input A
OR
Input B is receiving
Green
.
Outputs a
Red Signal
when neither Input A or Input B are receiving
Green
.
This gate outputs a Green Signal if receiving one or more Green Signals.
2 x 2 tiles
Advanced Automation
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
1 x 3 tiles
Computing
XOR Gate
Outputs a
Green Signal
if exactly one of its Inputs is receiving
Green
.
Outputs a
Red Signal
if both or neither Inputs are receiving a
Green Signal
.
This gate outputs a Green Signal if exactly one of its Inputs is receiving a Green Signal.
2 x 2 tiles
Computing
Sensors
Name
Automation Output
Materials
Research
Description
Atmo Sensor
Green
: Gas pressure within chosen range
Red
: Gas pressure out of chosen range
25
kg
Improved Ventilation
Sends a
Green Signal
or a
Red Signal
when
Gas
pressure enters the chosen range.
Atmo Sensors can be used to prevent excess oxygen production and overpressurization.
Critter Sensor
Green
: Number of Critters and Eggs in Room above threshold
Red
: Number of Critters and Eggs in Room below or at threshold
25
kg
Animal Control
Sends a
Green Signal
or a
Red Signal
based on the number of eggs and critters in a room.
Detecting critter populations can help adjust their automated feeding and care regiments.
Cycle Sensor
Green
: Current time in sensor's Green range
Red
: Current time in sensor's Red range
25
kg
Generic Sensors
Sets an automatic
Green Signal
and
Red Signal
schedule using a timer.
Cycle sensors ensure systems always turn on at the same time, day or night, every cycle.
Duplicant Motion Sensor
Green
: Duplicant in sensor's tile range
Red
: No Duplicant in sensor's tile range
25
kg
Smart Home
Sends a
Green Signal
or a
Red Signal
based on whether a Duplicant is in the sensor's range.
Motion sensors save power by only enabling buildings when Duplicants are nearby.
Gas Element Sensor
Green
: Selected Gas detected
Red
: Selected Gas not detected
25
kg
Generic Sensors
Sends a
Green Signal
when the selected
Gas
is detected on this sensor's tile.
Sends a
Red Signal
when the selected
Gas
is not present.
These sensors can detect the presence of a specific gas and alter systems accordingly.
Germ Sensor
Green
: Germ count within chosen range
Red
: Germ count out of chosen range
25
kg
50
kg
Pathogen Diagnostics
Sends a
Green Signal
or a
Red Signal
based on quantity of surrounding
Germs
.
Detecting germ populations can help block off or clean up dangerous areas.
Hydro Sensor
Green
: Liquid pressure within chosen range
Red
: Liquid pressure out of chosen range
25
kg
Improved Plumbing
Sends a
Green Signal
or a
Red Signal
when
Liquid
pressure enters the chosen range.
Must be submerged in
Liquid
.
A hydro sensor can tell a pump to refill its basin as soon as it contains too little liquid.
Liquid Element Sensor
Green
: Selected liquid detected
Red
: Selected liquid not detected
25
kg
Generic Sensors
Sends a
Green Signal
when the selected Liquid is detected on this sensor's tile.
Sends a
Red Signal
when the selected Liquid is not present.
These sensors can detect the presence of a specific liquid and alter systems accordingly.
Signal Switch
Green
: Turned ON
Red
: Turned OFF
25
kg
Smart Home
Sends a
Green Signal
or a
Red Signal
on an
Automation
grid.
Signal switches don't turn grids on and off like power switches, but add an extra signal.
Space Scanner
Green
: Incoming target detected
Red
: No target detected
25
kg
Celestial Detection
Sends a
Green Signal
to its automation circuit when it detects incoming objects from space. Can be configured to detect incoming meteor showers or returning space rockets.
Networks of many scanners will scan more efficiently than one on its own.
Thermo Sensor
Green
: Temperature within chosen range
Red
: Temperature out of chosen range
25
kg
HVAC
Sends a
Green Signal
or a
Red Signal
when ambient Temperature enters the chosen range.
Thermo sensors can disable buildings when they approach dangerous temperatures.
Timer Sensor
Green
: Timer in Green period
Red
: Timer in Red period
25
kg
Generic Sensors
Creates a timer to send
Green Signals
and
Red Signals
for specific amounts of time.
Timer Sensors create automation schedules for very short or very long periods of time.
Wattage Sensor
Green
: Power consumed within chosen range
Red
: Power consumed out of chosen range
25
kg
Generic Sensors
Sends a
Green Signal
or a
Red Signal
when
Wattage
consumed enters the chosen range.
Wattage sensors can send a signal when a building has switched on or off.
Weight Plate
Green
: Object or Duplicant present on top
Red
: No Object or Duplicant on top
50
kg
Generic Sensors
Sends a
Green Signal
when an object or Duplicant is placed atop of it.
Cannot be triggered by Gas or Liquids.
Weight plates can be used to turn on amenities only when Duplicants pass by.
Effectors
Name
Automation Input
Materials
Research
Description
Automated Notifier
Green
: Push notification
Red
: Nothing
25
kg
Notification Systems
Attach to sensors to send a notification when certain conditions are met.
Notification can be customized.
Sends a notification when it receives a Green Signal.
Duplicant Checkpoint
Green
: Duplicants may pass
Red
: Duplicants blocked
100
kg
Computing
Allows Duplicants to pass when receiving a
Green Signal
.
Prevents Duplicants from passing when receiving a
Red Signal
.
Checkpoints can be connected to automated sensors to determine when it's safe to enter.
Hammer
Green
: Hammer strikes once
Red
: Nothing
25
kg
Notification Systems
In its default orientation, the hammer strikes the building to the left when it receives a Green Signal.
Each building has a unique sound when struck by hammer. The hammer does no damage when it strikes.
The hammer makes neat sounds when it strikes buildings.
Pixel Pack
Green
: Display the configured
Green Signal
pixels
Red
: Display the configured
Red Signal
pixels
25
kg
100
kg
New Media
Pixels can be designated a color when it receives a
Green Signal
and a different color when it deceives a
Red Signal
.
Input from an
Automation Wire
controls the whole strip. Input from an
Automation Ribbon
can control individual pixels on the strip.
Four pixels which can be individually designated different colors.
List of all output buildings
This section lists all buildings that has an
Output Port.
[show]
Name
Green Automation Output
Red Automation Output
Atmo Sensor
Gas pressure within chosen range
Gas pressure out of chosen range
Automation Receiver
Green signals sent from a broadcaster
Red signals sent from a broadcaster
Critter Sensor
Number of Critters and Eggs in Room above threshold
Number of Critters and Eggs in Room below or at threshold
Cycle Sensor
Current time in sensor's Green range
Current time in sensor's Red range
Duplicant Motion Sensor
Duplicant in sensor's tile range
No Duplicant in sensor's tile range
Gas Element Sensor
Selected Gas detected
Selected Gas not detected
Germ Sensor
Germ count within chosen range
Germ count out of chosen range
Hydro Sensor
Liquid pressure within chosen range
Liquid pressure out of chosen range
Light Sensor
Ambient brightness within chosen range
Ambient brightness out of chosen range
Liquid Element Sensor
Selected liquid detected
Selected liquid not detected
Radbolt Sensor
When the condition is met
When the condition is not met
Radiation Sensor
When ambient Rads enters the desired range.
When ambient radiation is outside of the desired range.
Ribbon Reader
Green Signal
is read from the selected Bit of the attached
Automation Ribbon
Red Signal
is read from the selected Bit of the attached
Automation Ribbon
Ribbon Writer
Green Signal
is read from the attached
Automation Wire
Red Signal
is read from the attached
Automation Wire
Signal Counter
Counter reached target
Counter less than target
Signal Switch
Turned ON
Turned OFF
Space Scanner
Incoming target detected
No target detected
Thermo Sensor
Temperature within chosen range
Temperature out of chosen range
Timer Sensor
Timer in Green period
Timer in Red period
Wattage Sensor
Power consumed within chosen range
Power consumed out of chosen range
Weight Plate
Object or Duplicant present on top
No Object or Duplicant on top
Gas Reservoir
Emptied below low threshold
Filled above high threshold
Liquid Reservoir
Emptied below low threshold
Filled above high threshold
Smart Storage Bin
Full
Not full
Airborne Critter Trap
Critter has been trapped
No critter in trap
Critter Trap
Critter has been trapped
No critter in trap
Fish Trap
Critter has been trapped
No critter in trap
Refrigerator
Full
Not Full
Liquid Meter Valve
When the total amount of liquid meets the set value.
When the amount of liquid is below the set value.
Liquid Pipe Element Sensor
Chosen Liquid is detected
Chosen Liquid is not detected
Liquid Pipe Germ Sensor
When the germ count in the pipe meets the criteria
When the germ count in the pipe does not meet the criteria
Liquid Pipe Thermo Sensor
When temperature of liquid content meets the criteria
When temperature of liquid content does not meet the criteria
Smart Battery
under low charge threshold
over high charge threshold
Radbolt Chamber
Storage full
Storage not full
Command Capsule
Rocket launch checklist is complete
Rocket launch checklist is incomplete
Robo-Pilot Capsule
Rocket launch checklist is complete
Rocket launch checklist is incomplete
Rocket Platform
(1) Rocket is ready for flight; (2) Rocket is on the platform
Starmap Location Sensor
current location in filter
current location not in filter
Conveyor Meter
When the total amount of solids meets the set value.
When the amount of solids is below the set value.
Conveyor Rail Element Sensor
The configured item is on the rail
The configured item is not on the rail
Conveyor Rail Germ Sensor
The number of Germs on the object on the rail is within the selected range
Otherwise
Conveyor Rail Thermo Sensor
The contained item is within the selected Temperature range
Otherwise
Geotuner
Sends a
Green Signal
when the geyser is erupting
Sends a
Red Signal
when the geyser is not erupting
Gas Meter Valve
When the total amount of gas meets the set value.
When the amount of gas is below the set value.
Gas Pipe Element Sensor
Chosen gas is detected
Chosen gas is not detected
Gas Pipe Germ Sensor
When the germ count in the pipe meets the criteria
When the germ count in the pipe does not meet the criteria
Gas Pipe Thermo Sensor
When the temperature of pipe content meets the criteria
When the temperature of pipe content does not meet the criteria
List of all input buildings
This section lists all buildings that has an
Input Port.
[show]
Name
Green Automation Input
Red Automation Input
Automated Notifier
Push notification
Nothing
Automation Broadcaster
Broadcasts a green signal to reciever(s)
Broadcasts a red signal to receivers(s)
Duplicant Checkpoint
Duplicants may pass
Duplicants blocked
Hammer
Hammer strikes once
Nothing
Ribbon Reader
Write a
Green Signal
to the connected
Automation Wire
Write a
Red Signal
to the connected
Automation Wire
Ribbon Writer
Write a
Green Signal
to the selected Bit of the connected
Automation Ribbon
Write a
Red Signal
to the selected Bit of the connected
Automation Ribbon
Signal Counter
(1) Increment; (R) Set to 0
Nothing
Automatic Dispenser
Drops content
Keeps content
Bunker Door
Open
Close
Mechanized Airlock
Open
Close
Pneumatic Door
Open
Close
Transit Tube Access
Enable
Disable
Airborne Critter Trap
Set trap
Disarm and empty trap
Critter Fountain
Enable
Disable
Critter Lure
Enable
Disable
Critter Pick-Up
Wrangle excess critters
Ignore excess critters
Critter Trap
Set trap
Disarm and empty trap
Deep Fryer
Enable
Disable
Dehydrator
Enable
Disable
Electric Grill
Enable
Disable
Fish Trap
Set trap
Disarm and empty trap
Gas Range
Enable
Disable
Incubator
Enable
Disable
Microbe Musher
Enable
Disable
Rehydrator
Enable
Disable
Spice Grinder
Enable
Disable
Arcade Cabinet
Enable
Disable
Ceiling Light
Enable
Disable
Espresso Machine
Enable
Disable
Hot Tub
Enable
Disable
Juicer
Enable
Disable
Jukebot
Enable
Disable
Lamp
Enable
Disable
Mechanical Surfboard
Enable
Disable
Mercury Ceiling Light
Enable building
Disable building
Party Line Phone
Enable Building
Disable Building
Pixel Pack
Display the configured
Green Signal
pixels
Display the configured
Red Signal
pixels
Sauna
Enable
Disable
Sun Lamp
Enable
Disable
Vertical Wind Tunnel
Enable
Disable
Apothecary
Enable
Disable
Disease Clinic
Enable
Disable
Massage Table
Enable
Disable
Carbon Skimmer
Enable
Disable
Electrolyzer
Enable
Disable
Oxygen Diffuser
Enable
Disable
Rust Deoxidizer
Enable
Disable
Liquid Filter
Enable
Disable
Liquid Meter Valve
Reset counter and start liquid flow
Nothing
Liquid Pump
Enable
Disable
Liquid Rocket Port Loader
Enable
Disable
Liquid Rocket Port Unloader
Enable
Disable
Liquid Shutoff
Turn flow on
Turn flow off
Liquid Vent
Enable
Disable
Mini Liquid Pump
Enable
Disable
Coal Generator
Enable
Disable
Compact Discharger
Enable
Disable
Hydrogen Generator
Enable
Disable
Large Discharger
Enable
Disable
Large Power Transformer
Enable
Disable
Manual Generator
Enable
Disable
Natural Gas Generator
Enable
Disable
Peat Burner
Enable
Disable
Petroleum Generator
Enable
Disable
Power Bank Charger
Enable
Disable
Power Shutoff
Turn power on
Turn power off
Power Transformer
Enable
Disable
Steam Turbine
Enable
Disable
Wood Burner
Enable
Disable
Radbolt Chamber
Emit
Radbolts
Do not emit
Radbolts
Radiation Lamp
Enable
Disable
Uranium Centrifuge
Enable
Disable
Algae Distiller
Enable
Disable
Bleach Stone Hopper
Enable
Disable
Brackwax Gleaner
Enable
Disable
Desalinator
Enable
Disable
Emulsifier
Enable
Disable
Ethanol Distiller
Enable
Disable
Fertilizer Synthesizer
Enable
Disable
Glass Forge
Enable
Disable
Kiln
Enable
Disable
Metal Refinery
Enable
Disable
Molecular Forge
Enable
Disable
Oil Refinery
Enable
Disable
Oxylite Refinery
Enable
Disable
Plywood Press
Enable
Disable
Polymer Press
Enable
Disable
Rock Crusher
Enable
Disable
Sludge Press
Enable
Disable
Water Sieve
Enable
Disable
Command Capsule
Launch rocket
Awaits launch commmand
Enclosed Telescope
Enable
Disable
Gantry
Extend
Retract
Meteor Blaster
Enable
Disable
Mission Control Station
Enable
Disable
Robo-Pilot Capsule
Launch rocket
Awaits launch commmand
Rocket Control Station
Restrict access to interior buildings
Unrestrict access to interior buildings
Rocket Platform
Launch Rocket
Cancel Launch
Targeting Beacon
Enable
Disable
Telescope
Enable
Disable
Auto-Sweeper
Enable
Disable
Conveyor Chute
Enable
Disable
Conveyor Loader
Enable
Disable
Conveyor Meter
Reset counter and start material transfer
Nothing
Conveyor Shutoff
Enable
Disable
Robo-Miner
Enable
Disable
Solid Filter
Enable
Disable
Solid Rocket Port Loader
Enable
Disable
Solid Rocket Port Unloader
Enable
Disable
Artifact Analysis Station
Enable
Disable
Atmo Suit Checkpoint
Enable
Disable
Atmo Suit Dock
Enable
Disable
Blastshot Maker
Enable
Disable
Clothing Refashionator
Enable
Disable
Crafting Station
Enable
Disable
Data Miner
Enable
Disable
Exosuit Forge
Enable
Disable
Farm Station
Enable
Disable
Geotuner
Enable
Disable
Grooming Station
Enable
Disable
Jet Suit Checkpoint
Enable
Disable
Jet Suit Dock
Enable
Disable
Milking Station
Enable
Disable
Orbital Data Collection Lab
Enable
Disable
Oxygen Mask Checkpoint
Enable
Disable
Power Control Station
Enable
Disable
Remote Controller
Enable
Disable
Remote Worker Dock
Enable
Disable
Research Station
Enable
Disable
Shearing Station
Enable
Disable
Skill Scrubber
Enable
Disable
Soldering Station
Enable
Disable
Space Cadet Centrifuge
Enable
Disable
Telescope
Enable
Disable
Textile Loom
Enable
Disable
Virtual Planetarium
Enable
Disable
Virtual Planetarium
Enable
Disable
Ice Maker
Enable
Disable
Liquid Tepidizer
Enable
Disable
Oil Well
Enable
Disable
Space Heater
Enable
Disable
Sweepy's Dock
Enable
Disable
Thermo Aquatuner
Enable
Disable
Thermo Regulator
Enable
Disable
Gas Filter
Enable
Disable
Gas Meter Valve
Reset counter and start gas flow
Nothing
Gas Pump
Enable
Disable
Gas Rocket Port Loader
Enable
Disable
Gas Rocket Port Unloader
Enable
Disable
Gas Shutoff
Turn flow on
Turn flow off
Gas Vent
Enable
Disable
High Pressure Gas Vent
Enable
Disable
Mini Gas Pump
Enable
Disable
