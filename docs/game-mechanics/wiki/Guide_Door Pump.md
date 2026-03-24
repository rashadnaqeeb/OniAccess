# Guide/Door Pump

A door pump is an automated machine consisting of at least 3
Mechanized Airlocks
, some automation or similar way of timing, and liquids or gases to move.
Theory
When an airlock opens, it creates a Vacuum where it was, therefore sucking in any nearby gasses or liquids, and when it closes, it will push the elements inside of it out.
A Door Pump chains multiple Doors together to transport Elements from one side to another, usually used to keep a
Geyser
from overpressurizing. See
Geyser Taming
for more info.
Cycle:
The first two doors may open at the same time. Sucking the element in.
The fist door closes, locking the element inside the center door.
The last door opens, letting the element out.
The center and last door close in succession.
Basic Design
Theoretical Design realized in the Game
A
Timer Sensor
can give an impuls, and each door can be put behind a
BUFFER
and
FILTER Gate
.
In the simplest terms, the
BUFFER Gate
prolongs that input until the door needs to close, and the
FILTER Gate
cuts the signal off to when the door needs to open.
Sensor Timing
Mechanized Airlock
Powered
Unpowered
Timer Sensor
2 Seconds On
8 Seconds Off
6 Seconds On
20 Seconds Off
First Door (Buffer Sensor)
1.6 Seconds
6 Seconds
First Door (Filter Sensor)
1.3 Seconds
4 Seconds
Middle Door (Buffer Sensor)
4 Seconds
12 Seconds
Middle Door (Filter Sensor)
2.7 Seconds
8 Seconds
Last Door (Buffer Sensor)
6 Seconds
18 Seconds
Last Door (Filter Sensor)
4 Seconds
12 Seconds
Improved Design
A powered Door Pump Design activated if it senses gas in the Geyser Room.
This design immediately uses the Timer Sensor more efficiently, removing the necessity of half the gates. Furthermore only activates it if there is enough Gas Pressure in the Geyser Room, which stops it from wasting power. However, The Design can also be used without needing to power the doors, as long as the sensor timing is adjusted accordingly.
Sensor Timing
Mechanized Airlock
Powered
Unpowered
Timer Sensor
2 Seconds On
8 Seconds Off
8 Seconds On
11 Seconds Off
Middle Door (Buffer Sensor)
4 Seconds
5 Seconds
Last Door (Buffer Sensor)
6 Seconds
9 Seconds
Last Door (Filter Sensor)
4 Seconds
10 Seconds
Insulated Design
A modified door pump that keeps the middle door open for a layer of vacuum insulation when not in use.
Sensor Timing
Mechanized Airlock
Powered
Unpowered
Timer Sensor
2 Seconds On
8 Seconds Off
First Buffer Sensor (Last Door)
5 Seconds
First Filter Sensor (Last Door)
3 Seconds
Second Filter Sensor (Middle Door)
2 Seconds
Final Buffer Sensor (Middle Door)
1 Second
When temperature gradients are involved, such as handling the output of
Steam Vents
or
Hydrogen Vents
, it may be desirable to alter the logic of your door pump to maintain a vacuum seal while not actively pumping gas. The introduction of a
NOT Gate
on the middle door changes the flow compared to the above examples:
At the start, the first and third doors are closed. The second door is open, but contains a vacuum.
The first door opens, and activates the first buffer gate.
The first door closes. The first buffer gate remains active.
The first filter gate activates, opening the third door.
The second filter gate activates, activating the second buffer gate and closing the middle door.
The first buffer gate deactivates,  deactivating both filter gates and immediately closing the third door.
The second buffer gate deactivates, opening the middle door and resetting the system.
Other Designs
Liquid-controlled pump
An alternate design of pump is the liquid-controlled pump. It looks like this:
This type of door pump does the same thing as an automation-controlled one. It consists of 3 packets of liquid moving through an
infinite loop
, triggering pipe sensors as they move.
This could be coupled with a
liquid shutoff
on the bottom left of the pipe circuit to turn the pump on and off, however you must make sure that the packets of liquid are all maximum size, or they will combine when the shutoff turns on.
