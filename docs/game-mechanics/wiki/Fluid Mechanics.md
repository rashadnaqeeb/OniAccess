# Fluid Mechanics

This page is currently being worked on, expect frequent and significant changes over the next few days.
Reason:
This is a new page and is currently being filled in a community effort.
Fluid Mechanics
study the in-game mechanics of fluid tiles as mass inflow and mass outflow.
[
1
]
Given a fluid tile, the mass
m
will change over time
m
˙
=
d
m
d
t
with mass inflow and outflow
m
˙
=
m
˙
in
−
m
˙
out
. In a system of fluid tiles occupying distinct grid cells, the mass of a cell with coordinates
i
and
j
:
m
(
i
,
j
)
, net mass flow is the sum of mass inflow and mass outflow to each of it's neighbors
m
˙
i
,
j
=
m
˙
(
i
−
1
,
j
)
→
(
i
,
j
)
+
m
˙
(
i
,
j
−
1
)
→
(
i
,
j
)
+
m
˙
(
i
+
1
,
j
)
→
(
i
,
j
)
+
m
˙
(
i
,
j
+
1
)
→
(
i
,
j
)
.
Fluid mechanics divide into statics and dynamics based on the net mass flow of a fluid system. Statics describe the conditions on the fluid system where the net mass flow is zero, so mass is neither gained nor lost. In contrast, dynamics describe the processes on the fluid system that cause either mass inflow or mass outflow, so mass gain and mass loss are quantified.
The fluid mechanics are determined by in-game constant properties. For example, the property
molarMass
determines which elements will float (low
molarMass
) above other elements (high
molarMass
). Using in-game constant properties, we parameterize logical models and numerical equations to solve fluid mechanics problems in a fluid system.
Element Properties
The element properties
[
2
]
relevant to fluid mechanics are tabulated below.
property
mechanic
axes
molarMass
float (or sink)
Y
viscosity
orthogonal flow
XY
minHorizontalFlow
horizontal flow
X
minVerticalFlow
vertical flow
Y
maxMass
vertical flow
Y
liquidCompression
vertical flow
Y
References
↑
The flows are measured relative to the native simulation and sometimes the managed runtime.
↑
The element properties are defined YAML configuration files
{gas, liquid, solid}.yaml
.
