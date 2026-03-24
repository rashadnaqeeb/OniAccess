# Guide/Self-Powering Oxygen Machine

A
Self-Powering
Oxygen
Machine
(SPOM) is an engineering construct which produces Oxygen as well as additional power which is used to support it.
The Elements
The key element is the
Electrolyzer
, which consumes
water
and produces
Oxygen
and
Hydrogen
.
Gas Pumps
are used to siphon both gases out; at least two are needed per Electrolyzer or it will be constantly overpressured.
One or multiple
Hydrogen Generators
are used to convert the Hydrogen into Power.
Gas separation is done either by using a
Gas Filter
or by utilizing the fact that Hydrogen is less dense than Oxygen (i.e. it rises up).
A kickstart and reserve generator, e.g. a
Coal Generator
,
Manual Generator
.
Some kind of cooling to prevent the Electrolyzer(s) and the Hydrogen Generator(s) from overheating, such as an
Anti Entropy Thermo-Nullifier
or a bunch of
Wheezeworts
. Cooling is typically not needed if all elements are made of
Gold Amalgam
; the SPOM can then be self-cooled.
Tips
Geysers
are a renewable source of water that can often easily support a SPOM. Use a
Geyser Calculator
to easily find out how much water one produces and how many Electrolyzers it can support. Note that
Salt Water
needs to go through a
Desalinator
first, which outputs only 93% of its intake Salt Water mass as Water. ( (4650g/s) / (5000g/s) = 0.93 = 93%)
Due to the high output temperature of Oxygen of an Electrolyzer (
70 °C
/
158 °F
+), it is a good idea to cool the produced Oxygen before releasing it into a base. Coolers typically use a
Steam Turbine
-
Thermo Aquatuner
heat deletion device.
As an alternative non-permanent solution, the SPOM can be built in or near a cold biome with radiant pipes running through that biome before piping to the base.
Some SPOMs produce excess Hydrogen, which can be used as an auxiliary power source or to power an Anti-Entropy Thermo-Nullifier.
Simple setup
The simple setup uses two Gas Pumps and filters out Hydrogen with a Gas Filter. It occasionally needs a power injection; the Hydrogen Generator cannot power it by itself. Buildings in the upper chamber will overheat if not made from Gold Amalgam. It will also leak heat through the
Manual Airlock
and the
Pneumatic Doors
.
Simple SPOM
Simple SPOM (Pipes)
Basic SPOM
An efficient, basic, self-filtering SPOM.
The initial startup will pump the wrong element, which will cause damage to the hydrogen generator. Under most circumstances, this will quickly set itself up to only have the correct gasses pump.
The primary benefit of this design is that all components run under a single 1k wire. It also produces a small amount of excess hydrogen (just under 20g/s per electrolyzer), which allows for external power usage.
Setup with an environmental gas filter
A more complex setup is to use three Gas Pumps: two for the Oxygen and one for the Hydrogen and exploits the fact that the tile can hold only one type of gas at the same time.
[
1
]
Additionally it uses an
Atmo Sensor
to extract only some part of hydrogen and maintain enough pressure so that no oxygen passes the filter. The empirical best value for the Atmo Sensor is
Activate if above 600 g
.
Besides being more difficult, more expensive and having more complicated maintenance this setup also requires a '
purging
' phase to start proper separation. Also, even in the normal working mode it lets a bit of hydrogen into the oxygen pipe (be aware of that if you plan to use it for Atmo Suits). However, once it's been installed, it works perfectly, netting 600-650 kg of oxygen per cycle, sometimes more.
It is also possible to build this with
Insulated Tiles
instead of regular
Tiles
and
Metal Tiles
. However, this machine will then leak heat from the
Mechanized Airlocks
and buildings will have to be constructed from Gold Amalgam to prevent overheating.
SPOM with an environmental gas filter
SPOM with an environmental gas filter (pipes)
Common Designs
SPOM
"Self-Powering Oxygen Module"
by QuQuasar
Oxygen
888 g/s
Hydrogen
112 g/s
Self-Cooled?
No
Operating
Temperature
Low
Cheap Material
Options?
Yes
Efficiency
Medium
SPOM, 888 g/s Oxygen
Power Overlay
Materials Overlay (Gas)
Ventilation Overlay
This design
[
2
]
uses a single electrolyzer with a gas hood, utilizing the one-element-per-tile rule to filter hydrogen into the upper hood area, with atmo sensors to maintain certain levels of gas pressure in the system. It is capable of producing 888 g/s of oxygen.
This design may have been one of the first SPOMs ever created; it is certainly one of the first popular designs for an SPOM. While it is only named "the SPOM", here "SPOM" stood for
Self-Powering Oxygen Module
.
The original design of this SPOM used four
Wheezeworts
planted in
Flower Pots
(which is no longer possible) to provide cooling. Wheezeworts have been re-balanced long since; the example depicted above uses four wild Wheezeworts, based on an implementation showcased by Francis John on Youtube
[
3
]
, which (in
Hydrogen
gas) is theoretically enough cooling for the entire setup,
Jumbo Batteries
included,
although cooling efficiency is limited somewhat by material choices,
Tempshift Plate
placements, and so on; see
Wheezewort
for details on their cooling capabilities. Domesticated Wheezeworts provide more cooling, but require duplicant access; this is viable, since the Wheezewort section is atmospherically isolated from the actual SPOM itself, so if
Airlocks
and/or
Liquid Locks
are put in place to keep the hydrogen in the Wheezewort room, duplicants should be able to easily access it.
This design can use budget materials and still work:
Granite
can be used for piping and tempshift plates,
Copper Ore
can be used for the machinery,
Copper
for metal tiles, and even
Igneous Rock
for the insulated tiles (although some heat will likely leak into its surroundings). However, if not enough Wheezewort cooling is provided, then its machines may overheat.
Unusually, this design was intended for use specifically with three
Jumbo Batteries
- its creator notes that if a single
Smart Battery
is used instead, excess power can be generated and utilized - but there will be an excess of hydrogen gas, which must be transported away somewhere or else the SPOM will break down.
The lower atmo sensor is set to
Above 500 g
; the upper sensor is set to
Above 1000 g
.
Half Rodriguez
"Half Rodriguez"
by Francis John
Oxygen
1000 g/s
Hydrogen
≈ 126 g/s
Self-Cooled?
Yes
Operating
Temperature
Over
75 °C
/
167 °F
Cheap Material
Options?
Limited
Efficiency
Low
Half Rodriguez, 1000 g/s Oxygen
The
"Half Rodriguez"
design
[
3
]
, a smaller variant of the
full Rodriguez
design, uses the same concepts as
QuQuasar's SPOM
(above), but expanded and optimized slightly. Its advantages over the latter are that it is self-cooling (requiring no Wheezeworts or external cooling sources), can produce an entire 1000 g/s of oxygen due to utilizing two electrolyzers (versus the awkward 888 g/s of the "SPOM"), and can fit inside a common four-tile-high room (unlike its big brother).
This design is not an extremely efficient setup, but has become popular as a small, self-contained module. Its machines (
especially
the
Hydrogen Generator
) should be made out of
Gold
/
Gold Amalgam
(or better), since their operating temperatures will be close to (and probably a bit over)
70 °C
/
158 °F
- assuming the input water is no hotter than
70 °C
/
158 °F
, of course. The hydrogen is used first to cool the hydrogen generator (and smart battery and
Power Transformer
), then the extra hydrogen can be easily stored and transported elsewhere.
Granite
is used for the pipes here: although
Radiant Gas Pipes
can be used instead for the pipes crossing the hydrogen generator/battery/transformer, they are not required.
The lower atmo sensors are set to
Above 450 g
; the upper sensor is set to
Above 750 g
. Be sure to set the low threshold of the
Smart Battery
above zero, to avoid blackouts (e.g.
High Threshold: 90, Low Threshold: 10
).
Rodriguez
"Rodriguez"
by (Unknown)
[
4
]
Oxygen
3000 g/s
[
5
]
Hydrogen
≈ 378 g/s
Self-Cooled?
Yes
Operating
Temperature
Over
75 °C
/
167 °F
Cheap Material
Options?
Limited
Efficiency
Low
Full Rodriguez, 3000 g/s Oxygen
Power Overlay
Plumbing Overlay
Alternative plumbing arrangement, for better cooling of the output gases
Ventilation Overlay
Automation Overlay
The
"Rodriguez"
, also known as the
"Full Rodriguez"
[
3
]
(especially to contrast it with its
smaller brother
), is a much larger SPOM designed to support roughly 30 duplicants. It is another self-cooling design, capable of producing 3000 g/s of oxygen by using a total of four electrolyzers.
This design is not an extremely efficient setup, but has become popular as a self-contained module for larger, more-populated colonies; it is generally considered overkill for colonies with only a dozen duplicants. Its machines (especially the Hydrogen Generators) should be made out of Gold / Gold Amalgam (or better), since their operating temperatures will be close to (and probably a bit over)
70 °C
/
158 °F
- assuming the input water is no hotter than
70 °C
/
158 °F
, of course. The hydrogen is used first to cool the hydrogen generators (and the smart battery and the power transformers), then the extra hydrogen can be easily stored and transported elsewhere. Due to the larger number of heat-generating machinery in its power section,
Radiant Gas Pipes
are used in this example for the self-cooling hydrogen pipeline to better facilitate heat transfer, but strictly-speaking, this is
optional
: as long as the machines in the power section do not overheat at
75 °C
/
167 °F
, even regular
Gas Pipes
made of
Granite
are perfectly capable of self-cooling the module indefinitely. (If radiant gas pipes are desired, it is important to note that some material choices are deceptively bad: gold amalgam is especially poor, barely beating granite gas pipes.)
Unlike the smaller designs,
Radiant Liquid Pipes
are used here to provide additional self-cooling for the electrolyzer section: if the water supplied is lower than
70 °C
/
158 °F
, it will absorb some heat from the lower section, which will then be destroyed as the water is electrolyzed into
70 °C
/
158 °F
gases. This
also
cools the oxygen (and hydrogen), reducing the need for further cooling of the oxygen before it can be used by duplicants. This effect can be increased by snaking the radiant liquid pipes throughout the lower section, in a similar fashion to the radiant gas pipes in the upper section; refer to the pictures above for an example of such an arrangement.
The lower atmo sensors are set to
Above 450 g
; the upper sensor is set to
Above 750 g
. Be sure to set the low threshold of the
Smart Battery
above zero, to avoid blackouts (e.g.
High Threshold: 90, Low Threshold: 10
).
Hydra (High-Efficiency Designs)
The
Hydra
is an advanced class of oxygen machine which boasts higher efficiency than most of the above designs. While not necessarily
self-powered
in design, the hydrogen they produce can be used to generate power for them to operate on, thus turning them into SPOMs.
The high efficiency comes from taking advantage of the
One element per cell rule
: Electrolyzers work by outputting gas (alternating between hydrogen and oxygen) in their top-left tile. But since a single tile can only hold either liquid
or
gas, then if the electrolyzer (and especially its top-left corner) is submerged in liquid, its gas outputs will be immediately forced out of the top-left tile and into neighboring tiles. Also, since the electrolyzer only checks for
gas
pressure (not liquid) and only in its top-left tile, this means that a submerged electrolyzer can be kept running at 100% uptime - never overpressurizing, since its outputs are instantaneously evacuated from its
Cell of Interest
. Thus, a Hydra design will tend to have a higher oxygen/hydrogen output than a similarly-sized non-Hydra design.
See
Guide/Hydra
for details.
Notes
The "efficiency" of an oxygen machine is determined by several factors: the uptime (i.e active-vs-inactive ratio) of its electrolyzer(s), whether or not its electrolyzers are capable of overpressurizing during normal operation, and whether any gas is deleted during operation (and how much). Additional limiting factors can reduce efficiency, such as how the system behaves if one (or more) or its outputs are backed up (the Rodriguez family of SPOMs are especially vulnerable to this: if backed up, gases can enter the wrong gas pumps).
References
↑
Video guide, Quality of Life Update Mk 1 Version
↑
Self-Powering Oxygen Module MkII - (Production-And-Cooling)
(the original design by QuQuasar)]
↑
3.0
3.1
3.2
Electrolyzer, SPOM, O2, Oxygen: Tutorial nuggets : Oxygen not included
by Francis John: a guide on how to build SPOMs, showcasing several different designs and concepts
↑
The Rodriguez was
named
by Francis John after
Nicolás Rodriguez
, who had sent the design to him. Rodriguez probably did not design it; its exact authorship is apparently unknown.
↑
The Rodriguez is commonly cited as producing 2,975 g/s of oxygen, including in the above-referenced Francis John video, but rebalancing and bug fixes seem to have increased this since it was popularized; while still probably not
always
a consistent 3000 g/s (especially after reloading), it is very close.
