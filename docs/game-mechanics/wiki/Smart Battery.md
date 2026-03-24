# Smart Battery

Smart Battery
Stores Power from generators, then provides that Power to buildings.
Sends a Green Signal or Red Signal based on the configuration of the Logic Activation Parameters.
Very slightly loses charge over time.
Smart batteries send a Green Signal when they require charging.
Research
Sound Amplifiers
Dimensions
2×2 tiles
Category
Power
Heat
+
500 DTU/s
Overheat
at
75 °C
/
167 °F
Decor
-15 (Radius: 3 tiles)
Requires
Power generators
Effects
Power capacity: 20 kJ
Power runoff: 0.4 kJ/cycle
Auto Out
Green
: under low charge threshold
Red
: over high charge threshold
Other Properties
Susceptible to
flooding
Materials
Refined Metal
200 kg
Smart Battery
stores excess power from generators, but loses charge over time. Unlike other batteries it can be used for
automation
purposes.
Usage
The battery has two parameters:
High Threshold: the battery sends out a Red Signal when its charge is increased to this threshold.
Low Threshold: the battery sends out a Green Signal when its charge is reduced to this threshold.
The battery will activate an automation signal when its charge falls BELOW the 'Low Threshold' setting. It will not switch the signal off until the charge is greater than the 'High Threshold' value.
A typical sample is to use Smart Battery to control
Coal Generators
. Connect the Coal Generator and Smart Battery with power wires and
automation wires
. Set the Smart Battery High Threshold to 90% and Low Threshold to 50%. When the Smart Battery charge falls below 50%, it will send out a Green Signal to enable the Coal Generator. Once it is charged to 90%, it will send out a Red Signal to disable the Coal Generator. This is an effective way to avoid wasting the fuel and works with other continuous generators too.
Tips
The power loss of 15 Smart batteries is as small as the drain of one
Ceiling Light
. But, each battery produces the same amount of
heat
as the lamp.
The logic output is dependent on the percentage of charge stored. If the Smart Battery is sharing a circuit with other batteries this will effectively monitor their cumulative charge.
This does not work with
Jumbo Battery
, as they have double capacity and will not charge fully on the circuit regulated by a smart battery.
Batteries on a grid are drained out evenly by consumers. This means that if smart batteries are evened out after they are built (for example charged to full), then they can be used to control priorities of power sources usage - so, for example, you only start using coal generators when your hydrogen storage is depleted. See a
this guide
on Klei forums.
Because of their
hysteretic
behaviour, automation using a Smart Battery as a power level indicator can be tricky to setup. It is recommended to set parameters like 50-49 to overcome their hysteresis.
Smart Batteries send a green signal when power is low, which can be used to activate power generators. However, if connected to a
NOT Gate
, smart batteries will instead send a green signal when power is high, which can then be used to activate power consumers.
See also
Power
Battery
Jumbo Battery
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
Battery
Coal Generator
Compact Discharger
Conductive Wire
Conductive Wire Bridge
Dev Generator
Heavi-Watt Conductive Joint Plate
Heavi-Watt Conductive Wire
Heavi-Watt Joint Plate
Heavi-Watt Wire
Hydrogen Generator
Jumbo Battery
Large Discharger
Large Power Transformer
Manual Generator
Natural Gas Generator
Peat Burner
Petroleum Generator
Power Bank Charger
Power Shutoff
Power Transformer
Smart Battery
Solar Panel
Steam Turbine
Switch
Wire
Wire Bridge
Wood Burner
see all buildings
