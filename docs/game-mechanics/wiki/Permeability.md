# Permeability

Various objects in Oxygen Not Included have special behavior in regards to permeability.
Heat Permeability
Object
Thermal Category
Allows Cell Heat Transfer
Allows Cell Heat Transfer Beyond Tile Bounds
Allows Building Heat Transfer
Notes
Most Natural Tile
Cell
• • True
• • True
• • True
Transfers heat in the 4 connected tiles
Natural Tile (<1g)
Cell
• False
• False
• • True
Liquids cannot exist below 10 grams.
Vacuum
Cell
• False
• False
• False
Most Debris
Entity
• • True
• • True
• False
Exchanges heat with tile below (even if on a conveyor). Includes bottled liquids and gasses.
Debris (<1g)
Entity
• False
• False
• False
Geysers
N/A
• False
• False
• False
Most
Buildings
(building component)
Building
• • True
• False
• False
Most
Buildings
(entity component)
Entity
• • True
• • True
• False
Mesh Tile
,
Airflow Tile
Most
Buildings
(solid component)
Cell
• • True
• False
• • True
Most
Buildings
(contents)
Entity
• • True
• • True
• False
Includes resevoir contents
Conduction Panel
Building
• • True
• False
• • True
Building heat transfer on middle tile only
Solar Panel
(building component)
Building
• • True
• False
• False
Solar Panel
(solid component)
Cell
• False
• False
• False
Tempshift Plate
Building
• • True
• • True
• False
3x3 heat transfer area, 1x1 bounds
Ice-E Fan
Building
• • True
• • True
• False
5x2 heat transfer area, 2x2 building bounds, 2x2 tile collision bounds
Oil Well
Building
• • True
• False
• False
4x4 heat transfer area, 4x2 building bounds, 4x4 tile collision bounds (needs verifying)
Refrigerator
(powered, contents)
Entity
• False
• False
• False
Refrigerator
(unpowered, contents)
Entity
• • True
• • True
• False
Collision
Object
Allows Liquid Tile Movement
Allows Liquid Droplet Movement
Allows Gas Tile Movement
Allows Debris Movement
Allows Entities On Top
Allows Entity Movement Through
Allows Duplicant Movement Through
Allows Radbolt Movement
Notes
Solid Natural Tile
• False
• False
• False
• False
• • True
• False
• False
• False
Liquid Natural Tile
• • True
• • True
• False
• • True
• False
• • True
• • True
• • True
Only allows liquid tile and droplet movement if it is a greater (and not equal) density. Gas tiles may move through a liquid only if there is another liquid or gas to replace it.
Slicksters
may stand on top of sufficiently high mass liquids.
Gas Natural Tile
• • True
• • True
• • True
• • True
• False
• • True
• • True
• • True
Debris
• • True
• • True
• • True
• • True
• False
• • True
• • True
• • True
Debris do not hinder any movement
Radbolts
• • True
• • True
• • True
• • True
• False
• False
• False
• False
Collides with entities, including other radbolts
Most
Buildings
(building component)
• • True
• • True
• • True
• • True
• False
• • True
• • True
• • True
Most
Buildings
(entity component)
• • True
• • True
• • True
• • True
• False
• • True
• • True
• • True
Most
Buildings
(solid component)
• False
• False
• False
• False
• • True
• False
• False
• False
For example,
Tile
,
Fish Feeder
,
Solar Panel
Radbolt Joint Plate
• False
• False
• False
• False
• • True
• False
• False
• • True
Pneumatic Door
,
Manual Airlock
,
Mechanized Airlock
(open)
• • True
• • True
• • True
• • True
• • True
• • True
• • True
• • True
Entities only move if they are already falling
Pneumatic Door
(closed)
• • True
• • True
• • True
• • True
• • True
• False
• False
• • True
Manual Airlock
,
Mechanized Airlock
(closed)
• False
• False
• False
• False
• • True
• False
• False
• False
Mesh Tile
• • True
• • True
• • True
• False
• • True
• False
• False
• False
Droplets may be converted into liquid tiles
Airflow Tile
• False
• False
• • True
• False
• • True
• False
• False
• False
Detection
Object
Allows
Auto-Sweepers
Allows Space View
Allows Artificial Light
Allows Sunlight
Dims Light
Allows Decor
Valid Building Base
Decreases Radiation
Notes
Solid Natural Tile
• False
• False
• False
• False
• • True
• False
• • True
• • True
Liquid Natural Tile
• • True
• • True
• • True
• • True
• • True
• • True
• False
• • True
Most Gas Natural Tile
• • True
• • True
• • True
• • True
• • True
• • True
• False
• • True
Oxygen
• • True
• • True
• • True
• • True
• False
• • True
• False
• • True
Vacuum
• • True
• • True
• • True
• • True
• False
• False
• False
• False
Debris
• • True
• • True
• • True
• • True
• • True
• False
• False
• False
Most
Buildings
(building component)
• • True
• • True
• • True
• • True
• False
• • True
• False
• False
Most
Buildings
(entity component)
• • True
• • True
• • True
• • True
• False
• • True
• False
• False
Most
Buildings
(solid component)
• False
• False
• False
• False
• • True
• False
• • True
• • True
Solar Panels
• False
• False
• False
• False
• • True
• False
• • True
• False
Solar Panel Modules
• • True
• • True
• • True
• • True
• • True
• • True
• False
• False
Window Tiles
• False
• • True
• • True
• • True
• • True
• • True
• • True
• • True
Mesh Tile
,
Airflow Tile
• False
• • True
• False
• • True
• False
• • True
• • True
• False
Pneumatic Door
(open/closed)
• • True
• • True
• • True
• • True
• False
• • True
• False
• False
Manual Airlock
,
Mechanized Airlock
(open)
• • True
• • True
• • True
• • True
• False
• • True
• False
• False
Manual Airlock
,
Mechanized Airlock
(closed)
• False
• False
• False
• False
• • True
• False
• • True
• • True
