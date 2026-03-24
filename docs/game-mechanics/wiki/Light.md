# Light

This article may be
outdated
. It was last updated for
U33-473720
. These versions were tagged as updating the game mechanics discussed here:
U52-622222
The interior of your planetoid is mostly pitch black, though your
Duplicants
appear to function just fine. Producing
light
, however, can increase Duplicant workspeed at certain workstations, improve the
decor
of wherever it shines, and cause specific
plants
to grow; light also disturbs the sleep of most Duplicants and can prevent the growth of certain plants that need darkness. The exterior of your planetoid, accessed by breaching the surface at the top of the map, has natural
sunlight
that brightens and dims over the course of every cycle, which can be used to gain
power
through
solar panels
.
Use the
Light Overlay
to see which
tiles
are lit at any time. Brightness is measured in lux, and if a cell is lit by multiple sources of light, the cell's brightness is the sum of all lux values.
Point sources of light only emit for a limited range of cells, losing brightness over distance. They are blocked fully by most solid tiles.
Window tiles
and
pneumatic doors
allow light to pass through them at all times.
Airlocks
and
bunker doors
also allow light through while they are open. Sunlight has no maximum range, but is partially or fully absorbed by some gasses and liquids as well.
Duplicant Interactions
Light interferes with most
Duplicants
' sleep, so lights near their beds can negatively affect their stamina recovery. Dupes will even attempt to sleep in unlit areas if they pass out on the floor. The exceptions to these details are Duplicants with the Loud Sleeper trait, who are indifferent to light when sleeping, and those with the Nyctophobic trait, who require light to peacefully sleep.
In addition, long exposure to very bright light can cause stress relief (the "Bright and Cheerful" status at 40,000 lux and the "Intensely Bright" status at 72,000 lux) or
stress
(in the form of
sunburn
after 120 seconds of "Intensely Bright" status) in Duplicants.
Lit Workspace
While working at a building, a Duplicant can get a +15% speed bonus if they occupy a cell with at least 1 lux of light. This "Lit Workspace" status effect can be viewed when selecting a Duplicant, and generally applies to almost any operation that has a progress bar, which includes eating at a
Mess Table
, using a
Lavatory
, and pouring a drink from the
Espresso Machine
. Does not apply to
Grooming Station
or
Shearing Station
.
Decor
Any brightness over 0 lux provides a static +15 to
decor
for the alighted cell, ignoring the quantity of light sources or their cumulative intensity.
Building
Agriculture
A few
plants
require some light in order to grow, while a few cannot grow in the presence of any light; however, most plants are indifferent to light, unless they have specific mutations (only in the
Spaced Out!
DLC).
Bristle Blossoms
require any amount of light over 200 lux.
Gas Grass
requires 10,000 lux of light, which generally relegates its location to the planetoid's surface as sunlight is the only feasible and efficient way to acquire light of that intensity.
Dusk Caps
and
Bog Bucket
require darkness; even 1 lux of light prevents growth.
Plants with the Bountiful or Leafy mutation require an extra +200 or +1,000 lux above their normal requirement, whereas Exuberant plants require 0 lux.
Bonbon Trees
need light to enable their
Nectar
production, up to 10,000 lux to max out the production rate.
Oxygen Generation
Algae Terrariums
produce 10% more
oxygen
when in any amount of light above 0 lux.
Recreation
Beach Chairs
are reactional buildings used by Duplicants to gain +5
morale
, but if the bottom-left corner of a Beach Chair building (its
cell of interest
) is over 10,000 lux, then the Duplicant gains +8 morale instead.
Light Levels
Brightness [Lux]
Category
Effect
0
Pitch Black
Lit Workspace status removed
Up to 500
Very Dim
Up to 1,000
Dim
Distresses sleeping Duplicants (Unrested: Too Bright status for 0.7 cycles)
Up to 10,000
Well Lit
Up to 50,000
Bright
Above 40,000 Lux: Bright and Cheerful status for Duplicants,
Beach Chair
morale bonus
Up to 100,000
Brilliant
Above 72,000 Lux: Intensely Bright status for Duplicants (
Sunburn
after 120 seconds)
100,000 and beyond
Blinding
Light Sources
Floor Lamps
,
Ceiling Lamps
, and
Sun Lamps
can be used to create light using electricity.
The
Printing Pod
provides a large amount of light without needing power, making it a good area for recreational building in the early game.
Shine Bugs
produce light.
Natural sunlight can be found in the
Space Biome
.
The
Carved Lumen Quartz
produces infinite light without power, but doesn't count toward the Light Source requirement for Rooms.
The
Mercury Ceiling Light
charges itself during 60 seconds before reaching its peak lux emission.
Glow Stick
Duplicants
produce 500 lux in a radius of 2.
Bionic Duplicant
during defragmentation produce 1800 lux in a radius of 3 with a dropoff of 50%.
Light-Emitting Object
Light Range
Printing Pod
Mini-Pod
Shine Bug
Lamp
Ceiling Light
Sun Lamp
Carved Lumen Quartz
Mercury Ceiling Light
Duplicant
(Glow Stick trait)
...
Bionic Duplicant
(defragmentation)
...
Sunlight
Sunlight shines down from the top of the world. Unlike other light sources, which either have a circular or a cone shape, and lose their intensity with distance from the light source, the sun's light travels downwards indefinitely, if not absorbed by some
element
. An element's effective absorption (
A
e
) is its potential absorption factor (
A
0
) multiplied by (
m
1
0
0
0
) where
m
is its mass in
g
for
gasses
or
kg
for
liquids
(maximum 1). The resulting percent of absorbed light is relative to the base intensity of the sunlight (
I
0
), not the intensity of the light present in the cell (
I
i
n
). Hence, shaded light doesn't just approach 0 intensity, but can quickly turn pitch black.
I
o
u
t
=
I
i
n
−
I
0
(
1
−
A
0
min
⁡
(
m
1
0
0
0
,
1
)
)
The actual proportion of unabsorbed light is on average around 0.2% lower than what the above calculations would suggest. This is in most part due to the fact that the game maps the light exposure percentage (1 - the sum of the effective absorption of all cells above) to an integer between 0 and 255, truncating the decimals, and in lesser part because of floating point conversion errors result in slightly off values in the elements' potential light absorption factors (
A
′
).
I
=
⌊
I
0
2
5
5
max
⁡
(
⌊
2
5
5
(
1
−
∑
n
=
1
d
e
p
t
h
A
'
e
,
n
)
⌋
,
0
)
⌋
The intensity of sunlight gradually changes by time of day. During 12.5% of the cycle (during night) the intensity is 0. In the remaining 87.5%, it follows a sine wave with a half-period of 0.875 cycles and a maximum of 80000 Lux.
Using the provided details, we can calculate the average sunlight intensity on any planetoid.
I
m
a
x
is planetoid maximum light intensity.
1
.
7
5
π
I
m
a
x
≈
0
.
5
5
7
I
m
a
x
History
LU-356355
:
Some rooms now require a Light source to be considered a room.
Duplicants work 15% faster when working in a lit space.
Loud Sleepers sleep too deeply to be bothered by light while they sleep.
Duplicants' sleep is disturbed by sleeping in a lit space.
U33-472677
: (
Spaced Out!
only) Added varied sunlight & radiation values per world.
