# Guide/Power Circuits

Definitions
When you look at the
Power Grid Overlay
, White indicates a healthy circuit, yellow indicates the power draw is approaching/hitting the max, red indicates the circuit is overloaded, and blue indicates the circuit is inactive.
Machines consume Power. A
Ceiling Light
consumes 10W, or 10 Joules per second in the game (your common household light bulb however, is 40 to 60 W. A household LED light bulb uses 4 to 13 W)
Power
is the rate at which energy is generated or consumed and hence is measured in units (e.g. Watts) that represent energy per unit time. The amount of energy generated or consumed over a given period of time is equal to the average power times the duration.
A
Watt-second
is a unit of energy, equal to one Joule:
1 Watt-second (W*s or Ws) = 1 Joule (J)
1 Watt (W) = 1 Joule per second (J/s)
Elements of a circuit
Wires
Wires transport electrical energy. You have to connect Generators and consumers via wires.
There are four types of wires, differentiated by their capacity.
The capacity is only calculated for consumers, which is refered to as the "load". It is possible and intended behavior, that you can generate an unlimited amount of power without overloading and damaging the power circuit.
Wire
Power capacity
Building material
Decor
Wire
Wire Bridge
1000 W
Conductive Wire
Conductive Wire Bridge
2000 W
Heavi-Watt Wire
Heavi-Watt Joint Plate
2
20 kW
Heavi-Watt Conductive Wire
Heavi-Watt Conductive Joint Plate
2
50 kW
2
These wires cannot pass through through walls or doors. You have to use the Joint Plates to pass power through a wall.
Generators
Generator
Power Output [W, J/s]
Requirement
Manual Generator
400
Duplicant operation
1
Coal Generator
600
Coal
: 1kg/s
Wood Burner
300
Wood
: 1200g/s
Hydrogen Generator
800
Hydrogen
: 100g/s
Natural Gas Generator
800
Natural Gas
: 90g/s
Petroleum Generator
2000
Petroleum
: 2000g/s
Peat Burner
480
Peat
: 1000g/s
1
The Manual Generator generates 400W no matter who is running on it or what their
Athletics
skill is. But it
does
train the dupe's Athletics skill. If no batteries are connected to the Manual Generator, dupes will just keep running on it until they need something. This assumes other machines are connected to the circuit.
Batteries
Batteries can do both, provide and consume Power;
when they consume power, i.e. are being charged, batteries
do not count
in the wattage limit of the wire
this means, that you can connect and charge, e.g. 10 Jumbo Batteries with only Wire to your Power Generators. As long as the Batteries are the only consumers in the circuit, the Wire will never be overloaded and never break.
Batteries consume only
excess
Wattage: If you're powering 40 Ceiling Lamps off your Manual Generator, the battery may appear to spark, but it won't be charging.
Tiny Batteries generate the same amount of heat as large batteries while active.
Batteries have no apparent limit to their charge rate
or
their discharge rate.
Batteries lose charge over time.
As of the cosmic upgrade, the length of wire does not matter (unlike real world power lines that lose power over long distances). So if you have the metal to spare, it can't hurt to set up large battery banks far away from the base. Some clever designs have been created and shared online, including in the "Designs" section of this guide, where colonies with a heavy power load is transporing large amount of power using small 1.000 W wires, by utilizing
Power Shutoff
to alternate smart batteries between power circuits, meaning one circuit is charging the batteries while the other drains them.
Battery
Capacity [kJ]
Leak [kJ/cycle]
Battery
10
1
Jumbo Battery
40
2
Smart Battery
20
0.4
Transformers
Transformers are the
only
intended way to separate circuits:
as soon as any two wires are connected with each other, they are on the same circuit
this circuit has the capacity of the lowest of the present wires
overloading this circuit will result in wire tiles becoming damaged
bridges brake before wires brake; if you can't avoid damage, place them at convenient locations for easy repair access
Do note that large power transformers can supply (output) 4.000 W, which will overload conductive wires (but only if the power draw is present on that power circuit, it wont overload unless more than 2000 W is being used).
Power transformers do stack, so using 2 regular power transformers from the power backbone to the same other power circuit, will combine the 1000 W each transformer provide to a total of 2000 W available, which is exactly the safe limit for conductive wires + bridges, this will however produce heat for both transformers.
Also, do beware that an internal battery of 1.000 W for the regular and 4.000 W for the large power transformer. If unaware of this detail, it could lead to overloading of the power circuits.
Power Transformer
Large Power Transformer
Switches
To interrupt the power flow through a wire build a
Switch
on top of it. When switched "Off", the wire under the switch is disconnected. A switch does
not
turn off an entire circuit. Place a switch on a wire between power generators/batteries and the machines you wish to turn off. Multiple switches can be used on a circuit at different branches. Since a switch turned off essentially "removes" that wire from the circuit, this alters the circuit's properties. Turn off a switch and you have essentially split a single circuit into two.
A great use for a switch is to set up an emergency battery bank. Charge the bank to full, then turn the switch off between it and the circuit. Now you have backup power when the coal runs out or whatever the case may be.
A
Power Shutoff
is a switch that allows automation, increasing its usability.
Efficiency
Not using smart batteries is the number 1 cause of electricity problems in your colony. Without a few connected to your generators, you will be wasting huge amounts of power, resulting in a shortage of fuel, a surplus of heat, and, in the case of the coal, wood, natural gas, and petroleum generators, a huge amount of co2.
However, there is an easy solution to this issue. Smart batteries will use automation to activate your generators when the batteries are empty, and turn off the generators when the batteries are full, therefore saving huge amounts of electricity from being wasted.
To set up a smart battery, just connect it to the generator like you would a regular battery, and run an automation wire from the output port on the smart battery to the input port on the generator. You can still have regular batteries on the same circuit, because all batteries on a circuit will charge and discharge at the same rate.
Lastly, select the smart battery and set the Logic Activation Parameters to 99 (high threshold) and 5 (low threshold). Setting it up this way prevents frequent switching between on and off.
Now, your smart battery is set up and you'll be saving a significant amount of power.
Designs
Compact Electrical Circuit Separator - Automation View
Compact Electrical Circuit Separator - Wiring View
Electrical Circuit Separator
The wattage limits of wires are only calculated for energy consuming end-devices, but not for batteries or energy producing generators. This means that having heavy-watt wire between generators, batteries, and transformers is not strictly necessary if all energy consumers are strictly separated from all energy producers using automatically alternating batteries as a bridge between the circuits. Using an Electrical Circuit Separator permits building your generator backbone out of standard 1kW wire and only requires 2kW conductive wire on the consumer side for high wattage end-devices. This avoids the expense, decor penalty, and insulation limitations of heavy-watt wire. The heat produced by both batteries is equal to a transformer. To provide uninterrupted power, set the "recharge" threshold of the control battery to 1 or more.
Circuit Breaker
Circuit Breaker
To build a standard circuit breaker requires a wattage sensor, signal switch, power shutoff, and memory toggle. The generator side is to the left and end-devices are to the right. Arrange them as pictured and set the wattage sensor to send a green signal above 1kW for standard wire. This avoids damage to your wiring.
Notes
Have as many power generators as you desire on a circuit, just make sure to supply the appropriate number of batteries.
A single battery connected to any generator on an inactive circuit will result in wasted energy.
Thermo Sensors
are
very
handy for hot machines. Place a thermal switch next to your machine, run a wire through the thermal switch and then to the machine. Now set the thermal switch to activate IF COLDER THAN
80 °C
/
176 °F
(depending on building material). Now your machine won't be constantly running and heating up your base. Apply to all hot machines.
Dupes refuel
Coal Generators
or hop on the
Manual Generator
when ANY of the circuit's battery's power stored is below 50% - unless you change the setting on the power generator itself. But generally all batteries stay equally charged, so its safe to just consider multiple batteries as a single "battery bank".
Coal generators create lots of
heat
and
CO
2
. Plan accordingly.
