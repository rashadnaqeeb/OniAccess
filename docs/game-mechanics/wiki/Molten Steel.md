# Molten Steel

Molten Steel
Molten Steel is a Metal alloy of iron and carbon, heated into a hazardous Liquid state.
Vaporization Point
3826.85 °C
/
6920.33 °F
Vaporizes into
Steel Gas
(SHC: 0.49)
Freezing Point
1083.85 °C
/
1982.93 °F
Freezes into
Steel
(SHC: 0.49)
Specific Heat Capacity
0.386
DTU
g
∘
C
/
0.21
DTU
g
∘
F
Thermal Conductivity
80
DTU
m
s
∘
C
/
44.44
DTU
m
s
∘
F
Light
Absorption Factor
100%
Radiation
Absorption Factor
74%
Molar Mass
63.546
g
mol
Default Mass
900 kg
Max  Mass
3870 kg
Properties
Liquid
,
Metal Ore
,
Refined Metal
Code
elementId
MoltenSteel
localizationID
STRINGS.ELEMENTS.MOLTENSTEEL.NAME
Liquid Steel
is the Liquid state of
Steel
. It has the highest thermal conductivity and largest thermal range of any liquid, making it ideal for complex high-temperature processing.
Production
Liquid Steel is produced by heating
Steel
to
2429.85 °C
/
4405.73 °F
. The easiest way to do this is to run a
Kiln
in a vacuum, though care must be taken that its contents do not exchange heat with the steel upon melting.
Cooling a full
Pipe
of
Water
with a Steel
Thermo Aquatuner
in a vacuum will displace enough heat to melt the Aquatuner before the overheat damage breaks it, as long as it remains powered.
Buildings constructed from both
Steel
and
Plastic
will convert completely to liquid steel when melted. The
Germ Sensor
,
Liquid Pipe Germ Sensor
, and
Gas Pipe Germ Sensor
are the most effective way of doing this, all converting 50 kg of plastic to metal per tile, and melting quickly due to their low mass.
Handling
Diamond
is the only buildable material capable of holding maximum-temperature liquid steel indefinitely.
Airflow Tiles
containing a
Vacuum
can also function as a tank, but will melt almost instantly on exposure to gas (or if placed in the cell directly above the liquid). Natural tiles made of
Refined Carbon
can also be produced by heating
Coal
.
In practice,
Insulite
serves as an upper bound on the temperature of liquid steel as a working fluid, as it will melt at
3624.85 °C
/
6556.73 °F
, though this can be partially offset by alternating packets of high- and low-temperature fluid.
Tungsten
further restricts the use of
Radiant Liquid Pipes
to below
3421.85 °C
/
6191.33 °F
, or
2679.85 °C
/
4855.73 °F
for
Thermium
- while the alternating fluid bypass can also be used here, it still serves as a limit on the
average
temperature, which is what's relevant to heat exchange.
No
Liquid Pump
can be submerged in even low-temperature liquid steel without overheating, so a
tricked pump
, an
Auto-Sweeper
or a
Bottle Drainer
is necessary to load it into pipes without heavy resource use (repairs) or manual labour (repeated rebuilds).
Uses
Nothing directly consumes liquid steel, but its thermal properties make it an ideal means of conducting heat at high temperatures. The primary uses for this are melting solids, gaining advantage over machine processing:
Melting any natural tile will produce 100% of its mass as liquid, compared to 50% mass as debris from mining. Note that this is not always reversible -
Metal Ore
will produce
Refined Metal
when cooled, and any
Raw Mineral
will become
Igneous Rock
.
Of particular note is
Rust
. Mining and then using a
Rust Deoxidizer
and
Metal Refinery
will convert only around 27% of tile mass to
Iron
, while melting it will convert 100% of mass - albeit not producing any
Oxygen
or
Chlorine
.
Melting
Sand
will produce 100% of its mass in
Liquid Glass
, 4x the 25% mass output of the
Glass Forge
(8x if used on unmined natural sand tiles) without any duplicant labour.
Cooking natural
Clay
tiles will convert 100% of tile mass to
Ceramic
, which then drops to 50% when mined. Mining the clay (50% loss) and then firing it to ceramic in a
Kiln
ends in the same ratio of clay to ceramic, but also requires
Coal
.
Abyssalite
and
Insulation
can be melted into
Liquid Tungsten
, though radiant pipes cannot be used.
Melting
Thermium
separates it back into
Liquid Tungsten
and
Liquid Niobium
in the same ratio used to produce it.
Melting various materials and taking advantage of changes in specific heat capacity allows the conversion of minerals directly to heat, and thus to energy. As the generated heat is in the form of thermal mass, not an increase in temperature, this still requires an external heat source - it can't be directly used to heat the main liquid steel loop.
Melting
Regolith
into
Magma
at
1414.85 °C
/
2578.73 °F
multiplies its heat content by 5, producing
1
4
1
2
.
8
5
∘
C
×
1
.
0
0
0
kDTU
kg
∘
C
×
0
.
8
=
1
1
3
0
.
2
8
kDTU
kg
Obsidian
is generally harder to source than regolith, but has a similar reaction at the much higher temperature of
2726.85 °C
/
4940.33 °F
, resulting in 2181.48 kDTU/kg.
Salt Gas
condenses to
Molten Salt
at
1461.85 °C
/
2663.33 °F
. Re-boiling that molten salt by raising it to
1467.85 °C
/
2674.13 °F
requires less heat than was moved in condensing it, due to a change in SHC. Unlike regolith or obsidian this method does not require input materials, and acts as a flat 1.26x heat multiplier. This also makes molten salt useful as a "transformer", transferring heat from high-temperature pipelines through gas into lower-temperature pipelines or thermal storage before being fed to turbines.
Salt Gas
is one of a few materials that have a
specific heat change during phase change
.
Heat Sources
The simplest heat source for any liquid steel setup is the
Metal Refinery
, the only machine capable of working on a fluid which is in pipes and increasing its temperature arbitrarily.
Rocket
engines also transfer a fixed amount of heat to the tiles below them; using diamond
Window Tiles
and tungsten radiant pipes can capture and transfer this heat to the liquid steel system.
In
Spaced Out!
, a
Research Reactor
with limited coolant can produce extremely high-temperature waste, allowing for a steady heat source which does not require manual intervention or duplicants.
