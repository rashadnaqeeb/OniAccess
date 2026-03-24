# Guide/Multiplexing

Overview
For non-programmers understanding the
Multiplexing
components can be confusing.
Basically it's like a ribbon cable in that it lets you have more "status pipes" on fewer squares, but with the change that you can only access one at a time
In their simplest form multiplex devices are like walkie talkies that you can pick a channel to listen and/or speak on, and switch channels without having to rewire everything.
In more advanced modes they can be used to dynamically link two different channels together like how
old school telephones
worked
Or they can be used to cycle through all channels in response to a trigger (like checking on each of your rooms once per cycle, or a home base checking in on the status of systems at a remote base)
You can even add Time based multiplexing (ex. by adding a
clock signal
using a
Timer
) to build actual circuits that allow the exchange of large amounts of data on very few cables
Direct Connections (walkie talkie style)
TBD
Switching (Telephone style)
TBD
Cycling (Status Update style)
When would you use this
Imagine you have multiple bases, but want one single place to look to see the status of all the systems of all the bases.
Each base probably has a "Control Room" where you bring the status of it's relevant systems
With multiplexing you can also create a "Master Control" room that can ask each base for a status update so you only have to check in at a single place
You do this by having the Master Control communicate to each base with ribbon cables and multiplexing.
Why not just use more ribbon cables
With 3 ribbon cables normally you'd be able to check on 12 systems which is fine for an outpost
But add multiplexing and with just 2 ribbon cables you can check on 64 systems (and up to 1024 systems if you go back to 3 ribbon cables)
How do you do this
You have 1 tube that the remote base triggers when something changes
You have 1 tube that's the actual status of the source you're checking
And the rest are selectors, kind of like a remote that lets you pick a tv channel
When the trigger tube flips you have the Master Control cycle through all the channels, and stick the result for each in a Memory box tied to a pixel pack.
The result is for each tube you add, you double the amount of channels you can pick from
So with a single ribbon cable (1 trigger + 1 data + 2 control tubes) you only get 4 channels, but with 2 ribbon cables (1 trigger + 1 data + 6 control tubes) you can get 64 channels, and 3 ribbon cables gets you 1024 channels.
With 2 ribbon cables you have 8 tubes and could assign them as follows:
1 - [set by remote base]    Trigger: Green = good, Red = one of the systems has changed, and we need to update
2 - [set by remote base]    Data: Green or Red depending on the status of the system we're asking about
3 - [set by Master Control] Selector: These pick the multiplexer channels (total of 64 channels)
4 - [set by Master Control] Selector: These pick the multiplexer channels (total of 64 channels)
5 - [set by Master Control] Selector: These pick the multiplexer channels (total of 64 channels)
6 - [set by Master Control] Selector: These pick the multiplexer channels (total of 64 channels)
7 - [set by Master Control] Selector: These pick the multiplexer channels (total of 64 channels)
8 - [set by Master Control] Selector: These pick the multiplexer channels (total of 64 channels)
G = Green
R = Red
? = Status we're checking
- = Ignored
Normally the channels would look like this
G--- ----
When something changes at the remote base, it would then switch to this:
R--- ----
At which point the Master Control starts cycling through each channel one at a time and storing the status in a Memory+Pixel combo for your convienience:
-?RR RRRG -> update pixel block 1
-?RR RRGR -> update pixel block 2
-?RR RRGG -> update pixel block 3
-?RR RGRR -> update pixel block 4
...
-?RG RRRR -> update pixel block 16
-?RG RRRG -> update pixel block 17
...
-?GG GGGG -> update pixel block 63
-?RR RRRR -> update pixel block 64 (or 0 if you prefer)
What the funyuns?!? How am I supposed to keep track of that?
Short answer: You don't have to.
As long as each combination of Selectors is unique you don't actually need to care which number it represents
But if really want to know (cough: *nerd*) most advanced calculators (like the one on your phone) have a Scientific or Programmer setting where you can flip between Binary and Decimal as the display options
Clocked (Circuit style)
You can get a LOT more information passed if you use a
Clock signal
(set up with a
timer
on one side),
Time-division multiplexing
and/or start+stop codes
TBD
(Here's a very dry circuits
tutorial
.  Ideally something more accessible to people who aren't electronics engineers would be preferred though )
Note: For the following "Flip Flop" basically means the
Memory Toggle
(specifically it's an
And-Or SR Latch
)
(
Clock Signals, What are they good for?
)
(
The Clock, The Circuit, and The Edge
)
(
Parallel to Serial, or how I learned to stop worrying and put more data on a single cable
)
