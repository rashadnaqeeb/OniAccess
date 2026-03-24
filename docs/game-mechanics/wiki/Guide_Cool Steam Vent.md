# Guide/Cool Steam Vent

Cool Steam Vents
are a guaranteed
Geyser
for any asteroid with a
Swamp Biome
or
Caustic Biome
, and a source of precious clean
Water
. Unfortunately, they produce it at the inconvenient temperature of
110 °C
/
230 °F
, which makes properly harnessing their output an engineering challenge for a midgame colony.
Pre-Build: Heating vs Cooling
The low-temperature steam produced by a Cool Steam Vent is only usable on its own as fuel for a
Steam
Engine
, a niche
Rocketry
component that many colonies will never even build. To make full use of the geyser's output, steam must be condensed to
Water
: either by cooling it to
96.35 °C
/
205.43 °F
(roughly -53 kDTU/kg), or heating it to
125 °C
/
257 °F
(roughly +63 kDTU/kg) and using a
Steam Turbine
.
Cooling the steam is generally easier to implement, and will occur naturally as uncovered geysers flood their chambers and heat up the surrounding biome. This can be accelerated with
Tempshift Plates
,
Metal Tiles
, and Radiant
Gas
and
Liquid
Pipes to more rapidly transfer heat away from the geyser. While
Refined Metal
is recommended, it's possible to radiate enough heat with just
Metal Ore
and a
Thermo Regulator
, making cooling steam viable for early-game colonies. Of course, heating up your whole asteroid is rarely a sustainable approach.
Heating steam requires
Refined Metal
and
Plastic
for turbines, as well as
Steel
for a heat source unless natural heat is immediately available (such as a nearby
Volcano
). A
Thermo Aquatuner
with a full packet of
Water
moves enough heat for 9.3 kg/s of
Steam
, as long as there's enough heat to move - this is more than sufficient for most geysers.
Earlygame Cooling-Based Build
TBD - Honestly, just heating is better.
Midgame Heating-Based Build
An enclosed Cool Steam Vent boiler.
A heating-based vent tamer has three core components:
A sealed room containing the vent, a
Thermo Aquatuner
, and extra dry thermal mass
One turbine above the steam room for every 2 kg/s of output
A heat exchanger, which transfers heat from the turbine output back to the Aquatuner
While it's technically possible to feed the turbine output directly through the Aquatuner, having a constantly cycling coolant loop has several key advantages. The Aquatuner's energy consumption is independent of the size of the packets, so an additional bit of plumbing must be done to ensure that only 10kg packets are admitted to the Aquatuner; further, passing liquid through the steam chamber can introduce a failure case if water production exceeds consumption, which causes water to sit in pipes and eventually burst (even if using
Ceramic
). Circulating coolant avoids both issues, and having a separate cooling system allows for more efficient late-game heat transfer by replacing the
Water
with
Super Coolant
.
The Aquatuner in the example is controlled by both a
Liquid Pipe Thermo Sensor
(to stop it from freezing any coolant) and a
Thermo Sensor
(to prevent heating the steam chamber above
200 °C
/
392 °F
). The
Atmo Sensor
is set to 800 g of pressure, allowing some steam to remain in the chamber to regulate the Aquatuner's temperature and serve as extra thermal mass to heat new steam.
The heat exchanger is designed to counterflow coolant against output, similarly to a petroleum boiler. The vacuum divides it into sections, such that the warmer coolant is paired with warmer output without backwards heat transfer. An alternative design can pass coolant directly through the output water poured from a
Liquid Vent
;
Airflow Tiles
in vacuum can serve to separate layers.
