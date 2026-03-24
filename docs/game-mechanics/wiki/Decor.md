# Decor

This article may be
outdated
. It was last updated for
an unknown version
. These versions were tagged as updating the game mechanics discussed here:
U57-700386
Decor
is a measure of how beautiful a place is. It is affected by almost everything:
Furniture
,
Wires
,
Plants
,
Critters
and
Duplicants
, just to name a few.
What affects Decor?
Decor in a tile is influenced by many different items. Almost everything has a decor value (be it positive or negative) and a radius - the number of tiles its effect covers. For example, some plants, pieces of art, and the printing pod increase decor. Decor is decreased by things like dropped resources (debris), machines, wires, ladders, and storage bins. The Decor value in a tile is the sum of all the individual Decor values of items which include that tile in their Decor radius.
Window Tiles
and
Pneumatic Doors
let Decor pass through them. This can be especially helpful if
Artwork
cannot be placed to increase Decor, for example in areas containing densely placed machines.
Mechanized Airlock
and
Manual Airlock
do not allow Decor to pass through while closed, which can be used to block negative Decor areas while still allowing Duplicants to pass though when needed.
Materials and Decor
An item's decor value can be affected by the material it is built with. For example,
Granite
lists a value of +20% for Decor; this means the base Decor of whatever is built from Granite gets multiplied by 1.2. Negative Decor is also positively affected, with wires made of copper having 0.9 times the base Decor of the wire.
Materials with the highest Decor bonuses in each category
Material Category
Material
Decor Bonus
Notes
Metal Ore
+10%
Overheat Temperature +50°C
Thermally Reactive
Refined Metal
+50%
Overheat Temperature +50°C
High Thermal Conductivity
Thermally Reactive
Raw Mineral
+20%
Overheat Temperature +15°C
Overheat Temperature +200°C
Insulator
Transparent
+100%
Overheat Temperature +200°C
High Thermal Conductivity
Wood
+50%
In some cases, it may be better to choose a material with a lower or no Decor bonus.
Diamond
, for example, has a very high thermal conductivity, which may be undesirable, so
Glass
should better be used.
Other effects on decor
Lighting
gives a +15 Decor bonus, independent of its brightness. Coupled with the "Lit Workspace" speed bonus, this makes light very useful in industrial areas.
Duplicant outfits by themselves have -5 Decor. This can be counteracted by giving them a
Snazzy Suit
, which gives +30 Decor, or upgrading one of these to
Primo Garb
with +40 Decor. Warm Coats, however, have -10 Decor penalty.
Some non-decorative
Plants
have a positive decor value, giving farms some decor without
Furniture
.
Critters
have a positive decor value, some a very high one.
Shine Bugs
and their advanced morphs especially.
The Decor Overlay
By pressing F8, the
Decor Overlay
can be accessed, which displays decor values. Tiles with positive decor are tinted green and tiles with negative decor are tinted red. When tile moused over, Decor in that tile as well as a detailed report of everything contributing to Decor is displayed. Everything that contributes to Decor in that tile (
potted
Plants, for example), positively and negatively, gets a bright tint. If Decor in a tile reaches 120, "(Maximum Decor)" will be displayed behind that value and it won't increase. This refers to 120 being the maximum Decor Duplicants get a Morale bonus for (See "Decor Calculations"). However, Decor is not limited to 120 and can exceed that value by quite a lot if many decorative items are placed in an area.
Decor Calculations
Duplicants evaluate decor from their lower cell. Each Duplicant has a Decor value (shown as
Decor
in the Status panel; referred to below as
Smoothed Display Decor
). This can be viewed by selecting a Duplicant and opening the
Status
tab under
Condition
. Hovering over it shows three values:
Current Environmental Decor
,
Average Decor This
Cycle
, and
Average Decor Last Cycle
.
Smoothed Display Decor
is a smoothed display value that gradually moves toward Current Environmental Decor. It exists to avoid sudden UI jumps and does not have any impact on gameplay:
If the difference between Current Environmental Decor and the Smoothed Display Decor is less than 0.5, Smoothed Display Decor is set to Current Environmental Decor.
If Current Environmental Decor is greater than Smoothed Display Decor, increase Smoothed Display Decor by
12.5 /s
.
If Current Environmental Decor is less than Smoothed Display Decor, decrease Smoothed Display Decor by
4.167 /s
.
Average Decor This Cycle
is a running average for the cycle so far: it accumulates Current Environmental Decor once per second and divides that total by the number of seconds elapsed since the start of the cycle.
At the start of each cycle, the running total resets to 0, and
Average Decor Last Cycle
is set to the previous cycle’s Average Decor This Cycle. Morale bonuses are then applied based on Average Decor Last Cycle.
High Decor gives a
Morale
bonus. If a Duplicant's
Average Decor Last Cycle
exceeds specific values, they will get additional Morale, up to +12 for a value of at least 120.
Morale Bonus and required Decor
Average Decor last Cycle
Classification
Morale Bonus
< -30
Ugly
-1
≥ -30
Poor
+0
≥ 0
Mediocre
+1
≥ 30
Average
+3
≥ 60
Nice
+6
≥ 90
Charming
+9
≥ 120
Gorgeous
+12
Example:
A Duplicant currently has a running total of 17850 decor 595 seconds into the cycle. The Average Decor This Cycle would be 30. If it's Perceived Decor is currently 0 and its Current Environmental Decor is 1000 for the next 5 seconds, then at the end of the 5 seconds, the Average Decor This Cycle would be 38.08 and it's Perceived Decor would be 62.5. The duplicant will receive a morale bonus of +3 for the next cycle.
List of decor values
The following table contains each building and their impact on decor, as well as the decor range the object has. As of now, it is incomplete. Just to note some of the items below can be sculptured/painted by a duplicant that has a art skill, increasing the decor depending on the quality of the artwork. Items: Blank canvas (landscape and portrait included), Ice sculpture, large/regular sculpture, etc. Also, keep in mind that a duplicant that has the art fundamentales skill will produce crude artwork, aesthetic design mediocre, and masterworks, well masterworks.
List of objects decor parameters
Object
Decor
Range
80
7
52.5
8
48
10
45
8
30
8
30
6
25
6
25
4
25
4
24
4
20
4
15
11
15
3
15
2
10
2
10
1
10
0
5
1
0
N/A
-1
1
-5
1
-10
2
-15
3
-20
4
-20
5
-25
6
Tips
Due to the negative Decor, it is a good idea to either space out machinery where possible so the decor penalty can be easily countered, make it out of beautiful materials or place something with a positive decor value around its vicinity to reduce the decor penalty.
Before placing
Category:Artwork
type
Furniture
, make sure to set the
Priority
chart to only allow Duplicants with the Masterworks
Skill
to use them and use whatever material gives the highest decor bonus (Granite, Gold Amalgam/Copper Ore, Gold) to get the most decor out of them. Duplicants with low
Creativity
skill will only be able to slowly create crude and mediocre paintings and "abstract" sculptures, while Masterworks Artists will always create masterpiece quality Art: "Masterpiece" paintings and "Genius" sculptures.
Because Duplicants evaluate decor from their bottom tile, drywall being used exclusively for decor should be placed directly above the floor.
See also
Furniture
