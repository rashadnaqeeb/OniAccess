# Bridge Tricks

Liquid Bridges
,
Gas Bridges
, and
Conveyor Bridges
have several tricks that enable better management of the resources they convey. This page uses the word "pipes" to refer to
Liquid Pipes
,
Gas Pipes
, and
Conveyor Rails
.
Most of these tricks rely on specific input and output game mechanics. Bridges are not technically sections of pipe, but rather buildings. As long as the pipes are not entirely full of elements, the bridge can move gases or fluids indefinitely, without power.
These tricks don't apply to
Wire
,
Automation Wire
and
Automation Ribbon Bridges
because electricity and automation signals do not travel in packets.
Heat
Transfer
All types of bridges can be used as a more precise alternative to
Tempshift Plates
.
Continuous Loop
Normally, if you create a closed loop of pipes, resources within them won't travel anywhere, as there are no inputs for them to go to. Replacing a section of pipe with a bridge will cause flow, as the bridge provides an input for the resources to travel to, as well as an output to "push" them. This also works with
Liquid Reservoirs
and
Gas Reservoirs
, which also provide a buffer.
A moving liquid loop. In some cases, it might be necessary to remove one packet from the loop to allow flow.
With just pipes and a bridge, a heat exchange loop could passively and continuously circulate a cooling or heating medium (such as
Hydrogen
or
Polluted Water
).
Priority Input/Output
Branches in pipes will distribute incoming resources in a round-robin fashion, distributing the resource packets evenly. This might not be what you want; you may want to prioritize one path over another. Bridges can help with that.
When the bridge input is attached to any section of pipe, it always accepts a resource packet if there is room in the output pipe for it. This can be used to prioritize resources down one path (the path the bridge outputs to) until it fills up, with the overflow continuing to travel down the pipe.
An example showing the differences between a splitter with and without a bridge. Note how the intermittent "consumer" receives a full line of gas in conjunction with the priority input. (Click on the file to see it animated)
Conversely, if the bridge output is attached to the middle of a section of pipe, it will only output a resource packet if there is room for it in the output pipe (possibly merging packets if needed). This can be used to prioritize the source connected to the main pipe over the source connected to the bridge input, so that the bridge input source is only used if the main input source is not filling the pipe.
Note that these tricks work with any buildings with inputs or outputs, such as valves and shutoffs, not just bridges.
