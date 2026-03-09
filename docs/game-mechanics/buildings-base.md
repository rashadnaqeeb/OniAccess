# Base

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

## Ladder

Enables vertical mobility for Duplicants.

- Size: 1x1
- HP: 10
- Build time: 10s
- Materials: 100 kg Raw Minerals or Wood
- Decor: -5 (radius 1)
- Movement speed: 1x up, 1x down
- Not floodable, not entombable, not overheat-able
- No repair (BaseTimeUntilRepair = -1)
- Drag-buildable
- No tech required

## Fire Pole

Allows rapid descent; significantly slows upward climbing.

- Size: 1x1
- HP: 10
- Build time: 10s
- Materials: 100 kg Metal (any)
- Decor: -5 (radius 1)
- Movement speed: 4x down, 0.25x up
- Not floodable, not entombable, not overheat-able
- No repair
- Drag-buildable
- Tech: Refined Objects

## Plastic Ladder

Increases Duplicant climbing speed. Mildly antiseptic.

- Size: 1x1
- HP: 10
- Build time: 10s
- Materials: 50 kg Plastic
- Decor: -5 (radius 1)
- Movement speed: 1.2x up, 1.2x down
- Not floodable, not entombable, not overheat-able
- No repair
- Drag-buildable
- Tech: Luxury

## Tile

Basic floor/wall tile. Increases Duplicant runspeed.

- Size: 1x1
- HP: 100
- Build time: 3s
- Materials: 200 kg Raw Minerals
- Decor: +5 (radius 1)
- Movement speed: 1.25x (BONUS_2)
- Strength multiplier: 1.5x
- Replaces cell element: yes
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Drag-buildable
- Does not use structure temperature (uses tile temperature)
- No tech required

## Snow Tile

Insulating tile made of snow. Low thermal conductivity but will melt if too hot.

- Size: 1x1
- HP: 100
- Build time: 3s
- Materials: 30 kg Snow
- Decor: 0
- Default temperature: 263.15 K (-10 C)
- Strength multiplier: 1.5x
- Replaces cell element: yes (placed as Stable Snow)
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Drag-buildable
- DLC: Frosty Planet Pack

## Wood Tile

Cozy tile with insulation and decor bonus. Increases runspeed.

- Size: 1x1
- HP: 100
- Build time: 3s
- Materials: 200 kg Wood
- Decor: +10 (radius 2)
- Movement speed: 1.25x (BONUS_2)
- Strength multiplier: 1.5x
- Replaces cell element: yes
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Drag-buildable
- POI unlockable
- DLC: Frosty Planet Pack
- Tech: Luxury

## Airflow Tile

Blocks liquid flow without obstructing gas.

- Size: 1x1
- HP: 100
- Build time: 30s
- Materials: 100 kg Metal (any)
- Decor: -5 (radius 1)
- Liquid impermeable: yes
- Gas permeable: yes (does not replace cell element)
- Not floodable, not entombable, not overheat-able
- No repair
- Tech: Pressure Management

## Mesh Tile

Does not obstruct liquid or gas flow.

- Size: 1x1
- HP: 100
- Build time: 30s
- Materials: 100 kg Metal (any)
- Decor: -5 (radius 1)
- Liquid permeable: yes
- Gas permeable: yes (does not replace cell element)
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tech: Sanitation Sciences

## Insulated Tile

Reduces heat transfer between walls. Thermal conductivity multiplier 0.01.

- Size: 1x1
- HP: 30
- Build time: 30s
- Materials: 400 kg Raw Minerals
- Decor: -5 (radius 1)
- Thermal conductivity: 0.01 (1% of material's base)
- Strength multiplier: default
- Replaces cell element: yes
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Tech: Temperature Modulation

## Plastic Tile

Significantly increases runspeed. Mildly antiseptic.

- Size: 1x1
- HP: 100
- Build time: 30s
- Materials: 100 kg Plastic
- Melting point: 800 K (526.85 C)
- Decor: +5 (radius 1)
- Movement speed: 1.5x (BONUS_3)
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tech: Luxury

## Metal Tile

Significantly increases runspeed. Transfers heat quickly.

- Size: 1x1
- HP: 100
- Build time: 30s
- Materials: 100 kg Refined Metal
- Melting point: 800 K (526.85 C)
- Decor: +15 (radius 3)
- Movement speed: 1.5x (BONUS_3)
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tech: Smelting

## Window Tile

Transparent tile that allows light and decor to pass through.

- Size: 1x1
- HP: 100
- Build time: 30s
- Materials: 100 kg Transparent material (Glass/Diamond)
- Melting point: 800 K (526.85 C)
- Decor: +5 (radius 1)
- Transparent: yes (light passes through)
- Notifies on melt: yes
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tagged as Window
- Tech: Glass Furnishings

## Storage Tile

Floor tile with built-in storage for non-edible solids.

- Size: 1x1
- HP: 30
- Build time: 30s
- Materials: 100 kg Refined Metal + 100 kg Glass
- Melting point: 800 K (526.85 C)
- Decor: -10 (radius 2)
- Storage capacity: 1000 kg
- Movement speed: 0.75x (PENALTY_2)
- Stored items are insulated, sealed, and hidden
- Storage filters: standard solid materials
- Special item sizes: Atmo Suits count as 0.5x, Dehydrated items 0.6x, Molt Shells 0.5x
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tech: Solid Management

## Bunker Tile

Extremely durable tile. Can withstand extreme pressures and impacts.

- Size: 1x1
- HP: 1000
- Build time: 60s
- Materials: 100 kg Steel
- Melting point: 800 K (526.85 C)
- Decor: +5 (radius 1)
- Strength multiplier: 10x
- Notifies on melt: yes
- Tagged as Bunker (meteor resistant)
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tech: High Temp Forging

## Carpeted Tile

Soft tile that increases decor but slows movement.

- Size: 1x1
- HP: 100
- Build time: 30s
- Materials: 200 kg Raw Minerals + 2 kg Reed Fiber
- Decor: +20 (radius 4)
- Movement speed: 0.75x (PENALTY_2)
- Replaces cell element: yes
- Tagged as Carpeted
- Not floodable, not entombable, not overheat-able
- No repair
- Does not use structure temperature
- Tech: Clothing

## Drywall

Backwall building that prevents gas and liquid loss in space. Builds insulating backwall behind buildings.

- Size: 1x1
- HP: 30
- Build time: 3s
- Materials: 100 kg Raw Minerals or Wood
- Decor: +10 (radius 0)
- Placement: Not in tiles (backwall layer)
- Rotatable: 360 degrees
- Not floodable, not entombable, not overheat-able
- No repair
- Tech: Clothing

## Mini-Pod

Portable bioprinter that produces new Duplicants or care packages. Only one Printing Pod or Mini-Pod permitted per planetoid.

- Size: 3x3
- HP: 250
- Build time: 30s
- Materials: 50 kg Raw Minerals (any)
- Decor: +30 (radius 6)
- Not floodable, not overheat-able
- Repair time: 400s
- One per world
- Emits light: circle, range 5
- DLC: Spaced Out
- Tech: Advanced Research

## Pneumatic Door

Encloses areas without blocking liquid or gas flow. Wild critters cannot pass through.

- Size: 1x2
- HP: 30
- Build time: 10s
- Materials: 100 kg Metal (any)
- Decor: 0
- Door type: Internal (does not block gas/liquid)
- Rotatable: 90 degrees (can be placed horizontally)
- Entombable: yes
- Not floodable
- Animation speed: 1x (unpowered)
- Work time to open/close: 3s
- Automation: 1 input port (Green = open, Red = close and lock)
- Access control: yes
- No tech required

## Wicker Door

Breezy wooden door that encloses areas without blocking liquid or gas flow. Wild critters cannot pass through.

- Size: 1x2
- HP: 30
- Build time: 10s
- Materials: 100 kg Wood
- Decor: 0
- Door type: Internal (does not block gas/liquid)
- Rotatable: 90 degrees
- Entombable: yes
- Not floodable, not disinfectable, not repairable
- Animation speed: 1x (unpowered)
- Work time to open/close: 3s
- Drag-buildable
- Access control: yes
- Not replaceable
- DLC: Frosty Planet Pack
- Tech: Artistry

## Manual Airlock

Blocks liquid and gas flow, maintaining pressure between areas. Wild critters cannot pass through.

- Size: 1x2
- HP: 30
- Build time: 60s
- Materials: 200 kg Metal (any)
- Decor: -15 (radius 3)
- Door type: ManualPressure (blocks gas/liquid)
- Is foundation: yes (supports buildings above)
- Rotatable: 90 degrees
- Not overheat-able, not floodable, not entombable
- Animation speed: 1x (unpowered)
- Work time to open/close: 5s
- Access control: yes
- Tech: Pressure Management

## Insulated Door

Significantly reduces temperature exchange between rooms. Thermal conductivity 0.01.

- Size: 1x2
- HP: 100
- Build time: 60s
- Materials: 800 kg Raw Minerals + 2 kg Reed Fiber
- Decor: -15 (radius 3)
- Door type: ManualPressure (blocks gas/liquid)
- Thermal conductivity: 0.01
- Insulation modifier: 0.01
- Is foundation: yes
- Rotatable: 90 degrees
- Not overheat-able, not floodable, not disinfectable, not entombable, not repairable
- Drag-buildable
- Not replaceable
- Animation speed: 1x (unpowered)
- Work time to open/close: 5s
- Access control: yes
- Tech: Temperature Modulation

## Mechanized Airlock

Powered door that opens and closes quickly. Blocks liquid and gas flow. Functions as Manual Airlock when unpowered.

- Size: 1x2
- HP: 30
- Build time: 60s
- Materials: 400 kg Metal (any)
- Decor: -10 (radius 2)
- Door type: ManualPressure with power (blocks gas/liquid)
- Is foundation: yes
- Rotatable: 90 degrees
- Power: 120 W
- Overlay: Power
- Not overheat-able, not floodable, not entombable
- Animation speed: 5x powered, 0.65x unpowered
- Work time to open/close: 5s
- Automation: 1 input port (Green = open, Red = close and lock)
- Access control: yes
- Tech: Directed Air Streams

## Bunker Door

Massive, nearly indestructible powered door. Blocks liquid and gas flow. Can withstand extreme pressures and impacts.

- Size: 4x1 (horizontal orientation)
- HP: 1000
- Build time: 120s
- Materials: 500 kg Steel
- Decor: 0
- Door type: Powered pressure door
- Is foundation: yes
- Rotatable: 90 degrees (vertical orientation also valid)
- Power: 120 W
- Overheat temperature: 1273.15 K (1000 C)
- Not entombable
- Animation speed: 0.1x powered, 0.01x unpowered (very slow)
- No auto-control (must be manually set or automated)
- Work time to open/close: 3s
- Automation: 1 input port (Green = open, Red = close and lock)
- Tagged as Bunker (meteor resistant)
- Tech: High Temp Forging

## Storage Bin

Stores solid materials of your choosing.

- Size: 1x2
- HP: 30
- Build time: 10s
- Materials: 400 kg Raw Minerals or Metal
- Decor: -10 (radius 2)
- Storage capacity: 20000 kg (default)
- Storage filters: standard solid materials
- Not floodable, not overheat-able
- Allows item removal
- Nameable
- No tech required

## Smart Storage Bin

Stores solid materials. Sends Green automation signal when full.

- Size: 1x2
- HP: 30
- Build time: 60s
- Materials: 200 kg Refined Metal
- Decor: -10 (radius 2)
- Storage capacity: 20000 kg (default)
- Storage filters: standard solid materials
- Power: 60 W
- Heat output: 0.125 kDTU/s
- Overlay: Logic
- Automation: 1 output port (Green when full, Red otherwise)
- Not floodable, not overheat-able
- Allows item removal
- Nameable
- Tech: Smart Storage

## Liquid Reservoir

Stores liquid piped into it. Cannot receive manually delivered resources.

- Size: 2x3
- HP: 100
- Build time: 120s
- Materials: 400 kg Metal (any)
- Decor: -10 (radius 2)
- Melting point: 800 K (526.85 C)
- Storage capacity: 5000 kg
- Conduit: liquid input (offset 1,2) and liquid output (offset 0,0)
- Overlay: Liquid Conduits
- Not floodable
- Automation: 1 output port (Smart Reservoir - configurable high/low thresholds)
- Tech: Improved Liquid Piping

## Gas Reservoir

Stores gas piped into it. Cannot receive manually delivered resources.

- Size: 5x3
- HP: 100
- Build time: 120s
- Materials: 400 kg Metal (any)
- Decor: -10 (radius 2)
- Melting point: 800 K (526.85 C)
- Storage capacity: 1000 kg
- Conduit: gas input (offset 1,2) and gas output (offset 0,0)
- Overlay: Gas Conduits
- Not floodable
- Stored items: sealed and hidden
- Automation: 1 output port (Smart Reservoir - configurable high/low thresholds)
- Tech: HVAC

## Automatic Dispenser

Stores solid materials delivered by Duplicants. Dumps stored materials when it receives a Green automation signal.

- Size: 1x2
- HP: 30
- Build time: 10s
- Materials: 400 kg Metal (any)
- Decor: -10 (radius 2)
- Storage capacity: 20000 kg (default)
- Storage filters: standard solid materials
- Power: 60 W
- Heat output: 0.125 kDTU/s
- Overlay: Power
- Rotatable: horizontal flip
- Does not allow manual item removal
- Automation: 1 input port (Green = dump all, Red = store)
- Not floodable, not overheat-able
- Tech: Smart Storage

## Transit Tube

Quickly transports Duplicants from a Transit Tube Access to the tube's end. Only transports Duplicants.

- Size: 1x1
- HP: 30
- Build time: 10s
- Materials: 50 kg Plastic
- Decor: +5 (radius 1)
- Not floodable, not entombable, not overheat-able
- Repair time: 0s (instant)
- Drag-buildable
- Is utility (like pipes/wires)
- Placed on TravelTube tile layer
- No digging required during construction
- Tech: Travel Tubes

## Transit Tube Access

Allows Duplicants to enter the connected Transit Tube system. Stops drawing power once fully charged.

- Size: 3x2
- HP: 100
- Build time: 120s
- Materials: 800 kg Refined Metal
- Decor: -10 (radius 2)
- Power: 960 W (draws until internal battery is charged)
- Internal battery: 40000 J capacity (enough for 4 launches at 10000 J each)
- Wax consumption: 0.05 kg per launch (Milk Fat / Tallow)
- Wax storage: 10 kg capacity (200 launches)
- Not overheat-able
- Entombable: yes
- Automation: 1 input port (operational control)
- Self-sustaining energy consumer (stops drawing power when battery full)
- Tech: Travel Tubes

## Transit Tube Crossing

Allows Transit Tubes to pass through wall and floor tile. Functions as regular tile.

- Size: 1x1
- HP: 100
- Build time: 3s
- Materials: 100 kg Plastic
- Decor: 0
- Movement speed: 0.5x (PENALTY_3)
- Replaces cell element: yes
- Notifies on melt: yes
- Rotatable: 90 degrees
- Not overheat-able, not floodable, not entombable
- No repair
- Acts as foundation tile
- Tube bridge links: offsets (-1,0) to (1,0)
- Tech: Travel Tubes
