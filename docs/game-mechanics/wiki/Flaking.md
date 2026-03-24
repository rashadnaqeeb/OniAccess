# Flaking

Flaking is a mechanism which makes small 5 kg bits of liquids and solids to instantly phase up when two tiles with a high enough temperature differential come into contact, regardless of their respective
Thermal Conductivity
. There are two separate but very similar mechanisms: partial melting, where a solid melts to a fluid state and partial evaporation, where a liquid evaporates to another fluid state (liquid or gas).
Terms
Donor
: The hot cell that gives away some heat to enable flaking.
Parent
: The cell to be flaked. This cell will lose mass when flaked.
Child
: The new chunk of phased up matter created from the parent.
How flaking occurs
There are a few conditions, slightly different for partial melting and partial evaporation, that must be met for flaking to occur. Common and most important conditions are:
Cells must be adjacent after new tile location calculation occurs.
The Parent must be above 5 kg.
The Parent must be at least 3 °C below its phase up temperature.
The Donor must be at least 3 °C above the parent’s phase up temperature. With the previous conditions, this means at least a 6 °C difference between Donor and Parent.
The Child appears in 5 kg chunks at exactly 3 °C above the phase up temperature of the Parent (melting point if the parent is solid, vaporization point if the Parent is a liquid).
Flaking can happen during each game tick (there are 5 ticks per second), which means you can flake 25kg/s on any cell.
Crude Oil flaking to Petroleum then Sour Gas by being dropped on a very hot Abyssalite tile.
Natural occurrences
Flaking doesn’t occur a lot in the wild, because most materials' interactions are nowhere near the phase transition point. There are a few notable exceptions:
Excavating the
Oily Biome
until you reach a very hot (above
405 °C
/
761 °F
)
Abyssalite
tile and letting
Crude Oil
come into contact with it, the Crude Oil will flake to
Petroleum
. If it’s hot enough, the Petroleum will then be flaked to
Sour Gas
. Those two state changes follow the partial evaporation rules. This is sometimes referred to as “the abyssalite bug”, even though this is an intentional game mechanic.
Letting gas hotter than 3 °C in a
Tundra Biome
will turn
Ice
into
Water
at a much faster rate than it should according to regular
Thermal Conductivity
rules. This state change follows the partial melting rules.
Partial Melting
Additional conditions for this type of flaking:
A solid Parent and a gas Donor. It does not work with a liquid Donor.
The Parent must be a natural tile. Dupe-built tiles won't be flaked.
The Parent must have a mass strictly above 5kg (so 5000.001 g works, but 5000.0 does not).
The gas Donor must be able to stay in gas form after giving off heat.
The Parent phase up must be a liquid phase. It does not work with solid to solid phase up.
The normal heat conduction is calculated before flaking in this case. For this formula, we take into account temperatures after heat conduction:
D
T
′
=
D
T
−
5
0
0
0
g
⋅
C
c
⋅
Δ
T
D
m
⋅
D
c
where
D
T
′
is the temperature of the Donor after flaking
D
T
is the temperature of the Donor after heat conduction and before flaking
C
c
is the
specific heat capacity
of the Child
D
c
is the specific heat capacity of the Donor
D
m
is the mass of the Donor
Δ
T
is the difference between the Parent's and the Child's temperatures.
This formula is approximate.
[
1
]
Partial Evaporation
Additional conditions for this type of flaking:
A liquid Parent. The Donor can be a liquid, gas or solid, including a Dupe-built tile.
The Parent must have a minimum mass of exactly 5010 g (so 5009.99999 g fails).
The Parent liquid cannot be trapped (it must be able to be displaced).
The Donor must be adjacent to the parent after the game computes new tile locations, but before it updates their location. As such, you can flake liquid from 2 tiles away, with the 5 kg child constantly between the Donor and Parent, never allowing the Donor and Parent to exchange heat via conduction.
The Donor must not decrease in temperature by more than 10 °C when the flaking occurs.
The normal heat conduction is calculated after flaking. For this formula, the result before that happens:
D
T
′
=
D
T
−
5
0
0
0
g
⋅
P
c
⋅
Δ
T
D
m
⋅
D
c
where
D
T
′
is the temperature of the Donor after flaking and before heat conduction
D
T
is the temperature of the donor before flaking
P
c
is the Parent's specific heat capacity.
Tips
With intentional flaking, you can refine
Crude Oil
to
Petroleum
early without the need for a
Petroleum Boiler
, by dripping Crude Oil on natural hot
Abyssalite
tiles. Care should be taken to avoid having
Sour Gas
instead.
Flaking can be used to improve the boiling chamber of a Petroleum Boiler, as it avoids
State Change
Hysteresis.
[
2
]
Flaking allows melting
Abyssalite
to
Tungsten
without any regards for
Abyssalite
's very low
Thermal Conductivity
.
For Partial Evaporation, an Igneous Rock Tile is a great option, as its low Thermal Conductivity will reduce heat bleed, while its high Specific Heat Capacity will make flaking easier.
Bugs
If the liquid parent is pushed upwards (trapped on left/right/below), then the new temperature of the parent is reset to match the donor's new temperature. You can generate massive amounts of heat with this.
Solid flaking uses the specific heat capacity of the child, this can be abused because flaking will, in some cases, use much less energy than it should. Any Parent/Child pairing that results in a large temperature change from parent to child can generate or destroy a lot of heat as well.
References
The main reference for this page explanations (pre-patch formulas)
The main reference for this page formulas (post-patch)
See also
Thermal Conductivity
↑
Spreadsheet used to find formulas
↑
The Bead Flaker
