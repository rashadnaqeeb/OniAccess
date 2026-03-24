# Guide/Natural Gas Geysers

Natural Gas Geysers
are a great source of power for your colony. However, when it comes to getting the most out of your geysers, there are a lot of considerations.(NOTE: This page is considered outdated in the Cosmic Upgrade, as currently Fertilizer Synthesizers don't just take Polluted Water, but Dirt and Phosphorite too meaning you may need to do some drecko and pip ranching.)
The Puzzle Pieces
Natural Gas Generators
consume
Natural Gas
and produce
Polluted Water
and
Carbon Dioxide
. This Carbon Dioxide can be used by
Carbon Skimmers
to produce more Polluted Water. The Polluted Water, in turn, can be used by
Fertilizer Synthesizers
to produce fertilizer, which also produces more Natural Gas. Here are the numbers:
Building
Variable
Natural Gas
(g/s)
CO
2
(g/s)
Polluted Water
(g/s)
G
-90
+22.5
+67.5
M
+10
0
-39
S
0
-300
+1000
Y
+100
0
0
One additional piece to note is that a Carbon Skimmer also consumes up to 1000g of clean water per second. However, this doesn't depend on any of the other structures, so just keep that requirement in mind for now.
Putting It All Together
So this seems pretty complicated. However, we can express each of these resources as an equation in terms of the number of each of these structures. Let's use the variables from the table above to refer to how many of each structure we'll need.
CO
2
:
2
2
.
5
G
=
3
0
0
S
Polluted Water:
6
7
.
5
G
+
1
0
0
0
S
=
3
9
M
Natural Gas:
1
0
0
Y
+
1
0
M
=
9
0
G
That's a lot of equations. To figure this out, let's try to solve for the number of other structure for each geyser. First, we can solve the CO
2
equation to get how many scrubbers we need per generator:
S
=
2
2
.
5
3
0
0
G
=
0
.
0
7
5
G
This means each generator supplies 0.075 scrubbers. Next, let's plug this into the Polluted Water equation to get how many makers we need per generator:
6
7
.
5
G
+
(
1
0
0
0
)
(
2
2
.
5
3
0
0
)
G
=
3
9
M
M
=
1
4
2
.
5
3
9
≈
3
.
6
5
4
G
This means we need about 3.654 Fertilizer Synthesizers per generator. Finally, let's plug this into the Natural Gas equation to figure out how many generators we can support per geyser:
1
0
0
Y
+
(
1
0
)
(
1
4
2
.
5
3
9
)
G
=
9
0
G
1
0
0
Y
+
1
4
2
5
3
9
G
=
9
0
G
1
0
0
Y
=
2
0
8
5
3
9
G
G
=
3
9
0
0
2
0
8
5
Y
≈
1
.
8
7
Y
This means each geyser supports about 1.87 generators, account for its own natural gas production and the natural gas of the Fertilizer Synthesizers that are supplied by the Polluted Water from both sources. Now we can propagate this count backward through the other equations to get the counts for the other buildings:
M
=
(
1
4
2
.
5
3
9
)
(
3
9
0
0
2
0
8
5
)
Y
≈
6
.
8
3
Y
, or 6.83 Fertilizer Synthesizers per geyser.
S
=
(
2
2
.
5
3
0
0
)
(
3
9
0
0
2
0
8
5
)
Y
≈
0
.
1
4
Y
, or 0.14 Carbon Skimmers per geyser.
Maximum Capacity of a Geyser
All in all, this means that each natural gas geyser can support the following buildings:
Building
Count
Average Uptime
Total Power Production
2
93.5%
+1496 W
7
97.6%
-820 W
1
14%
-17W
Additionally, you need one
Gas Pump
to supply the generators, one
Liquid Pump
to feed Polluted Water to the Fertilizer Synthesizers, and one more Liquid Pump to feed clean
Water
to the Carbon Skimmer.
Overall, you can produce 659 W of power per geyser, at a cost of 140 g/s of clean water (in addition to 443.95 g/s of dirt and 177.58 g/s of phosphorite for the fertilizer synthesizers). Note that a
Steam Geyser
produce 750 g/s of water on average, so one Steam Geyser can supply five natural gas geysers' worth of these systems.
Example Builds
Natural Gas Complex with separate 1kW subsystems
