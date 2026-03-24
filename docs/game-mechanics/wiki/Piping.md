# Piping

The
Piping
(
Gas
,
Liquid
, or
Solid
) in
Oxygen Not Included
can behave in unobvious and unrealistic ways due to the nature of a simulation. These aspects can both lead to frustrations with designs not working when they are not understood and can offer very useful mechanics when employed properly.
Construction
Different heat-types of pipes (insulated/regular/radiant) can replace one another without losing their content.
Nearby pre-built pipes can be linked without a
dupe
-worker. It can be used to built cautiously.
Maintenance
Valves are operated by dupes, and will continue to pump at the same rate until dupe switches them.
Instead, shutoffs paired with
automation
(atmo/hydro sensor) can be used as more work-safe, instant, player-accessible switches.
However, valves can be used to ensure that downstream pipe has some slack and will not get blocked in a single moment. Or to equalize multiple inputs while merging a section or splitting output unequally.
Duplicant with Plumbing
skill
can empty pipes from their content into manually transportable containers, in a way similar to
Canister Filler
or
Pitcher Pump
. It is continuous action and has to be cancelled to stop.
Flow
Materials are transmitted through pipes using "packets". Each tile of pipes can contain only one packet and each packet can consist of only one type of fluid. Packets will only flow from one pipe tile to another if there is both:
An input building capable of receiving packets.
Pipe junction segments count as such.
A pipe that can accept at least part of the packet.
It is generally desirable to maintain a simple, strict direction of flow, with all sources on one half of a pipeline and all consumers on the other. Mixing sources and consumers on the same line can lead to unpredictable path-finding, where some pipe segments will not be filled and some consumers might be ignored all-together. The direction can be forced by breaking continuous pipelines with bridges, valves and shutoffs.
Examples
A pump connected to any number of pipes but nothing else will not pump.
A vent connected to any number of pipes but nothing else will empty those pipes.
A pump connected to a tank through a series of pipes will pump until the tank is full and all pipes leading up to the tank contain a packet.
If a tank is full and the pipe connected to it contains 5 kg of water and the pipe before that contains 8 kg of water then the second to last packet will send 5 kg of water to the last pipe to fill that packet. (Provided the liquid has some destination)
If in the same setup the 5 kg of water are followed by 8 kg of polluted water then the packets cannot merge.
Junctions
Most pipes will lead to more than one destination and often enough will also have more than one source. Advanced schematics can also contain loops. This leads to many junctions and understanding the behavior of these junctions is important.
Pure pipe junctions
Junctions that consist of only pipes operate on an alternating pattern. If there are 3 inputs and one output then the junction will take a packet from each input pipe in sequence over and over again. Conversely if there are 3 output pipes then the junction will send one packet to each output pipe on sequence over and over. If one output pipe is full the others will receive more packets.
Using valve on outgoing pipe will help even the load over time.
Pipes, buildings, and priority
When a pipe passes through the input or output node of a building the handling of packets changes dramatically. The input node (white symbol) of a building (bridge, vent, hydroponic farm plot) will always
take priority
IF the building can accept a packet. The output node (green symbol) of a building (bridge, pump, water sieve) will always
yield priority
to any incoming pipe.
Examples
If you imagine two horizontal lines of pipes going in parallel with one tile separating them. They each do their own thing which does not concern this example. Now you put a bridge going from the upper line to the lower line. The effect of this is that:
If liquid is flowing in the upper line but not in the lower line then the bridge will take all packets from the upper line as long as it can output them in the lower line.
If liquid is flowing in both lines with only full packets then both lines will flow uninterrupted and the bridge will do nothing. This is because every time there is a packet for the bridge to take there is also a full packet at the other side which takes precedence.
Useful patterns
This is an example of managing excess flows. In this example, fuel from the reservoir moves past the input port of the rocket engine - now full - and will return to the reservoir. This works if the loop does not have any viable junction
between the two input ports
. The reservoir can be disabled later to keep the fuel in.
Infinite loop
If you just build a loop of pipe and add the output of a bridge this loop will not fill, nor would any liquid inside that loop continue to flow. However if you build a loop of pipe and substitute one pipe tile with a bridge then the loop can both accept packets and will move them around because all three criteria for flow are satisfied (building output, building input, free pipe).
This can be used to pump a coolant medium between an area that needs to be cooled and a central room where medium is cooled back down. It can also be used to build buffers (preferably with
reservoirs
of either type
) and
loop filters
.
In general, it is useful to keep some pipe empty and rather regulate output rates of sources with automation.
Top-Up junction
If you need to supply a fluid to a network and have two sources available but you prefer to use one primary source over the other secondary source, you can use a top-up junction. To do this feed the primary source directly into the network and the secondary source via a bridge, valve or shutoff. As long as the primary source can satisfy the demands of the network the bridge will be blocked and the secondary source will not be used.
Overflow junction
If you have a source that can more than adequately supply your network and you want all surplus fluid to be diverted somewhere else you can use an overflow junction. To do this you pipe your source directly to whatever place you want your overflow to go to. Then you connect your main network to the source via a bridge. Now, as long as the primary network has demand for the resource the bridge will assure that all packets will be sent there. Only when the primary network is full will the bridge become blocked and surplus packets can find their way to some other place.
Short overflow loop, that feeds back into the line, can be used to place sensors.
Implications
Every other situation is just a specific case of the above mentioned rules but there are a few that warrant mentioning:
Mixing content in pipes is undesirable, as it drastically reduces throughput – same-type content can merge and "bypass", while mixed content prevents this behavior. If it is not possible, minimizing merges and splits, and filtering out contamination early will mitigate that.
In case of gasses
mechanical filters
at the input point are strictly preferable to pipe-filtering.
Running pipes directly through consumers like
Hydroponic Farm
tiles, vents or exo-suit docks leads to a strict
first-come-first-serve
basis and is generally undesirable due to hefty internal buffers. The first consumers will starve all successive ones if they are unable to fill their internal buffers consistently, it also makes continuously run buildings less efficient due to delivery lag.
Running pipes directly through several outputs leads to last-serve queue and tends to block the outputs in the middle, as the building cannot output if there is a full or incompatible packet in the pipe.
It is more acceptable with storage reservoirs that are hooked to the same input
and
output pipelines.
