# ONI Research System

## How Research Works

Oxygen Not Included has a tech tree of about 100 technologies (86 in the base game, 100 with Spaced Out DLC). Each technology unlocks new buildings and items when completed. Technologies are organized into a directed graph where each tech can require one or more prerequisite techs before it becomes available to research.

### Starting the Game

Seven technologies are available from the very start with no prerequisites: Basic Farming, Ventilation, Interior Decor, Employment, Plumbing, Pharmacology, and Power Regulation. These are the roots of the tree. Everything else branches out from these seven.

### Researching a Tech

To research a technology, you select it on the research screen and a Duplicant with the Research skill goes to a Research Station and works on it. Each tech has a cost measured in research points. Early techs cost only basic research points (produced at a basic Research Station). Higher-tier techs additionally require advanced points (from a Super Computer), and the highest tiers require specialized points: space research (base game) or orbital and nuclear research (Spaced Out).

### Auto-Queue

When you select a tech that has unresearched prerequisites, the game automatically queues all missing prerequisites in order. For example, if you want Meal Preparation (tier 1) but haven't researched Basic Farming (tier 0), the game queues Basic Farming first, then Meal Preparation. This cascades through the entire chain, so selecting a tier 5 tech could queue half a dozen prerequisite techs automatically.

### The Research Queue

The queue is an ordered list. Techs are researched one at a time, in queue order. You can reorder or remove items from the queue. When the current tech finishes, the next one starts automatically.

### The Screen Layout

The research screen has two main areas:

1. **The Tree View** (center): A large 2D pannable/zoomable diagram showing all technologies as cards connected by lines indicating prerequisites. Cards are colored by state: dark/dim for locked (prerequisites not met), bright for available, green for completed, highlighted for queued. This is the primary visual interface but is not accessible as it requires mouse-based panning and clicking on specific card positions.

2. **The Sidebar** (right side): A searchable, categorized list of all technologies grouped by the 12 category bands (Food, Power, Solid Material, Colony Development, etc.). Each entry shows the tech name and can be clicked to view details and select it for research. The sidebar has a text search field at the top.

### Tech Card Details

When you select a tech (either from the tree or the sidebar), you see:
- The tech's name and description
- Its research cost (point types and amounts)
- What buildings/items it unlocks
- Its current state: locked, available, queued, in progress, or completed
- Its prerequisites (which other techs must be done first)

## Tiers and Costs

Techs are arranged in tiers based on how deep they are in the prerequisite chain. Tier 0 techs have no prerequisites. A tech's tier is one more than the highest tier among its prerequisites. Higher tiers cost more points and require more advanced research methods.

### Base Game Cost Progression

| Tier | Basic | Advanced | Space |
|------|-------|----------|-------|
| 0 | free | - | - |
| 1 | 15 | - | - |
| 2 | 20 | - | - |
| 3 | 30 | 20 | - |
| 4 | 35 | 30 | - |
| 5 | 40 | 50 | - |
| 6 | 50 | 70 | - |
| 7 | 70 | 100 | - |
| 8 | 70 | 100 | 200 |
| 9 | 70 | 100 | 400 |

### Spaced Out Cost Progression

| Tier | Basic | Advanced | Orbital | Nuclear |
|------|-------|----------|---------|---------|
| 0 | free | - | - | - |
| 1 | 15 | - | - | - |
| 2 | 20 | - | - | - |
| 3 | 30 | 20 | - | - |
| 4 | 35 | 30 | - | - |
| 5 | 40 | 50 | - | 20 |
| 6 | 50 | 70 | 30 | 40 |
| 7 | 70 | 100 | 250 | 370 |

## The 12 Category Bands

Technologies are visually grouped into 12 horizontal bands on the tree diagram:

1. **Food** - farming, cooking, ranching, animal care
2. **Power** - generators, wiring, batteries, fuel engines
3. **Solid Material** - refinement, smelting, forging, conveyor systems
4. **Colony Development** - skills, research stations, robots, space program, rockets
5. **Radiation Technologies** - nuclear research, radiation protection, nuclear power (Spaced Out)
6. **Medicine** - medical stations, disease sensors, advanced treatments
7. **Liquids** - pipes, pumps, filters, purifiers, temperature control
8. **Gases** - gas pipes, ventilation, decontamination, HVAC
9. **Exosuits** - atmo suits, jet suits, travel tubes
10. **Decor** - art, furniture, glass, monuments
11. **Computers** - automation, logic circuits, sensors, multiplexing
12. **Rocketry** - telescopes, cargo bays, engines, orbital modules

These are visual groupings only. A tech's position in a category band does not affect its prerequisites; dependencies cross freely between bands.

## The Tech Graph: Dependencies

Each tech is listed below with its display name, tier, what it requires, and what it leads to. "Requires" means direct prerequisites only. "Leads to" means direct dependents only (not the full chain).

Technologies that exist only in Spaced Out are marked with (SO). Technologies that exist only in the base game are marked with (BG).

### Tier 0 - Starting Techs (free, no prerequisites)

**Basic Farming**
Leads to: Meal Preparation
Unlocks: Algae Terrarium, Planter Box, Ration Box, Compost

**Ventilation**
Leads to: Pressure Management, Portable Gases
Unlocks: Gas Pipe, Gas Bridge, Gas Pump, Gas Vent

**Interior Decor**
Leads to: Artistic Expression, Smart Home
Unlocks: Flower Pot, Floor Lamp, Ceiling Light

**Employment**
Leads to: Advanced Research, Brute-Force Refinement
Unlocks: Water Cooler, Crafting Table, Disposable Electrobank (Raw Metal), Campfire

**Plumbing**
Leads to: Filtration, Air Systems, Sanitation, Improved Plumbing (SO only: none extra)
Unlocks: Liquid Pipe, Liquid Bridge, Liquid Pump, Liquid Vent

**Pharmacology**
Leads to: Medical Equipment
Unlocks: Apothecary, Lubricant Applicator

**Power Regulation**
Leads to: Internal Combustion
Unlocks: Battery, Switch, Wire Bridge, Small Electrobank Discharger

### Tier 1

**Meal Preparation**
Requires: Basic Farming
Leads to: Agriculture, Ranching
Unlocks: Electric Grill, Egg Cracker, Mess Table, Farm Tile

**Filtration**
Requires: Plumbing
Leads to: Distillation, Improved Plumbing
Unlocks: Gas Filter, Liquid Filter, Sludge Press, Oil Changer

**Air Systems**
Requires: Plumbing
Leads to: Liquid-Based Refinement
Unlocks: Electrolyzer, Rust Deoxidizer

**Sanitation**
Requires: Plumbing
Leads to: Flow Redirection
Unlocks: Lavatory, Wash Basin, Shower, Mesh Tile, Gunk Emptier

**Pressure Management**
Requires: Ventilation
Leads to: Decontamination, Improved Ventilation, Temperature Modulation, Space Gas (SO)
Unlocks: Liquid Valve, Gas Valve, Gas Permeable Membrane, Manual Airlock

**Portable Gases**
Requires: Ventilation
Leads to: (none)
Unlocks: Gas Bottler, Bottle Emptier (Gas), Oxygen Mask, Oxygen Mask Locker, Oxygen Mask Marker, Oxysconce

**Internal Combustion**
Requires: Power Regulation
Leads to: Improved Combustion, Advanced Power Regulation, Sound Amplifiers, Advanced Combustion (SO)
Unlocks: Coal Generator, Wood Burner, Peat Generator

**Medical Equipment**
Requires: Pharmacology
Leads to: Pathogen Diagnostics
Unlocks: Triage Cot, Hand Sanitizer

**Advanced Research**
Requires: Employment
Leads to: Notification Systems, Space Program, Artificial Friends, Pathogen Diagnostics, Nuclear Research (SO), Materials Science Research (SO)
Unlocks: Super Computer, Skill Scrubber, Telescope, Exobase Headquarters, Advanced Crafting Table

**Brute-Force Refinement**
Requires: Employment
Leads to: Refined Renovations, Smart Storage, Artificial Friends
Unlocks: Rock Crusher, Kiln

**Artistic Expression**
Requires: Interior Decor
Leads to: Textile Production, Fine Art
Unlocks: Airlock Door, Wall Pot, Hanging Pot, Corner Moulding, Crown Moulding, Pedestal, Small Sculpture, Ice Sculpture

**Smart Home**
Requires: Interior Decor
Leads to: Generic Sensors
Unlocks: Automation Overlay, Signal Switch, Automation Wire, Automation Wire Bridge, Duplicant Motion Sensor

### Tier 2

**Agriculture**
Requires: Meal Preparation
Leads to: Animal Control, Food Repurposing
Unlocks: Farm Station, Fertilizer Synthesizer, Refrigerator, Hydroponic Farm, Park Sign, Radiation Lamp

**Ranching**
Requires: Meal Preparation
Leads to: Animal Control
Unlocks: Grooming Station, Critter Drop-Off, Shearing Station, Critter Feeder, Fish Release, Fish Feeder, Critter Pickup, Critter Drop-Off

**Distillation**
Requires: Filtration
Leads to: Advanced Distillation
Unlocks: Algae Distiller, Ethanol Distillery, Water Sieve

**Improved Plumbing**
Requires: Filtration
Leads to: Liquid Tuning
Unlocks: Insulated Liquid Pipe, Liquid Pipe Pressure Sensor, Liquid Shutoff, Liquid Pipe Preferred Flow, Liquid Overflow Valve, Liquid Reservoir

**Liquid-Based Refinement**
Requires: Air Systems
Leads to: Radiation Protection (SO), Advanced Sanitation
Unlocks: Ore Scrubber, Desalinator

**Flow Redirection**
Requires: Sanitation
Leads to: Liquid Distribution
Unlocks: Mechanical Surfboard, Liquid Bottler, Rocket Liquid Port Loader, Rocket Liquid Port Unloader, Small Liquid Cargo Bay

**Decontamination**
Requires: Pressure Management
Leads to: (none)
Unlocks: Deodorizer, Carbon Skimmer, Mechanized Airlock

**Improved Ventilation**
Requires: Pressure Management
Leads to: Gas Distribution, Hazard Protection
Unlocks: Insulated Gas Pipe, Gas Pipe Pressure Sensor, Gas Shutoff, High Pressure Gas Vent

**Temperature Modulation**
Requires: Pressure Management
Leads to: HVAC
Unlocks: Insulated Door, Ice-E Fan, Ice Fan, Ice Machine, Ice Kettle, Insulated Tile, Space Heater

**Space Gas** (SO)
Requires: Pressure Management
Leads to: Gas Distribution
Unlocks: CO2 Engine, Rocket Gas Port Loader, Rocket Gas Port Unloader, Small Gas Cargo Bay

**Improved Combustion**
Requires: Internal Combustion
Leads to: Plastic Manufacturing
Unlocks: Natural Gas Generator, Oil Refinery, Petroleum Generator

**Advanced Power Regulation**
Requires: Internal Combustion
Leads to: Low-Resistance Conductors, Solid Transport
Unlocks: Heavi-Watt Wire, Heavi-Watt Wire Bridge, Hydrogen Generator, Power Relay, Small Transformer, Wattage Sensor

**Sound Amplifiers**
Requires: Internal Combustion
Leads to: Space Power
Unlocks: Smart Battery, Jukebox, Power Control Station, Electrobank Charger, Electrobank

**Advanced Combustion** (SO)
Requires: Internal Combustion
Leads to: Plastic Manufacturing
Unlocks: Sugar Engine, Small Oxidizer Tank

**Pathogen Diagnostics**
Requires: Medical Equipment + Advanced Research
Leads to: Micro-Targeted Medicine
Unlocks: Germ Sensor (Gas Pipe), Germ Sensor (Liquid Pipe), Germ Sensor (building)

**Notification Systems**
Requires: Advanced Research
Leads to: Celestial Detection
Unlocks: Hammer, Alarm, Telephone

**Space Program**
Requires: Advanced Research
Leads to: Crash Plan, Celestial Detection
Unlocks: Launch Pad, Trailblazer Module, Orbital Cargo Module, Rocket Control Station

**Artificial Friends**
Requires: Advanced Research + Brute-Force Refinement
Leads to: Robotic Tools
Unlocks: Sweepy, Scout Module, Fetch Drone

**Refined Renovations**
Requires: Brute-Force Refinement
Leads to: Smelting
Unlocks: Fabricated Wood Maker, Fire Pole, Thermal Regulator, Ladder Bed

**Smart Storage**
Requires: Brute-Force Refinement
Leads to: Solid Transport
Unlocks: Conveyor Overlay, Auto-Sweeper, Smart Storage Bin, Dispenser

**Textile Production**
Requires: Artistic Expression
Leads to: Home Luxuries, Hazard Protection
Unlocks: Textile Loom, Carpet Tile, Drywall

**Fine Art**
Requires: Artistic Expression
Leads to: High Culture
Unlocks: Canvas, Sculpture, Shelf

**Generic Sensors**
Requires: Smart Home
Leads to: Advanced Automation, Parallel Automation
Unlocks: Weight Plate, Gas Element Sensor, Liquid Element Sensor, NOT Gate, Clock Sensor, Timer Sensor, Light Sensor, Cluster Location Sensor

### Tier 3

**Animal Control**
Requires: Agriculture + Ranching
Leads to: Gourmet Meal Preparation, Creature Comforts
Unlocks: Airborne Critter Trap, Ground Critter Trap, Aquatic Critter Trap, Incubator, Critter Sensor

**Food Repurposing**
Requires: Agriculture
Leads to: Bioengineering (SO)
Unlocks: Juicer, Spice Grinder, Milk Press, Smoker

**Advanced Distillation**
Requires: Distillation
Leads to: (none)
Unlocks: Chemical Plant

**Liquid Tuning**
Requires: Improved Plumbing
Leads to: Advanced Caffeination
Unlocks: Radiant Liquid Pipe, Thermo Aquatuner, Liquid Pipe Temp Sensor, Liquid Pipe Element Sensor, Liquid Tepidizer, Liquid Meter Valve, Thermal Interface Plate

**Liquid Distribution**
Requires: Flow Redirection
Leads to: Advanced Sanitation (partially)
Unlocks: Liquid Emptier (Pipe), Rocket Interior Liquid Input, Rocket Interior Liquid Output, Wall Toilet

**HVAC**
Requires: Temperature Modulation
Leads to: Catalytics
Unlocks: Thermo Regulator, Temp Sensor, Gas Pipe Temp Sensor, Gas Pipe Element Sensor, Radiant Gas Pipe, Gas Reservoir, Gas Meter Valve

**Plastic Manufacturing**
Requires: Improved Combustion (BG: just this one; SO: + Advanced Combustion)
Leads to: Valve Miniaturization
Unlocks: Polymer Press, Oil Well

**Low-Resistance Conductors**
Requires: Advanced Power Regulation
Leads to: Renewable Energy
Unlocks: Conductive Wire, Conductive Wire Bridge, Heavi Conductive Wire, Heavi Conductive Wire Bridge, Large Transformer, Large Electrobank Discharger

**Solid Transport**
Requires: Smart Storage + Advanced Power Regulation
Leads to: Solid Control
Unlocks: Conveyor Loader, Conveyor Rail, Conveyor Bridge, Conveyor Chute, Small Solid Cargo Bay, Rocket Solid Port Loader, Rocket Solid Port Unloader, Rocket Port Bridge

**Home Luxuries**
Requires: Textile Production
Leads to: Glass Blowing
Unlocks: Comfy Bed, Plastic Ladder, Plastic Tile, Clothing Alterations, Wood Tile, Multi-Duplicant Dining Table

**High Culture**
Requires: Fine Art
Leads to: Renaissance Art
Unlocks: Large Canvas, Metal Sculpture, Wood Sculpture

**Hazard Protection**
Requires: Textile Production + Improved Ventilation
Leads to: Transit Tubes
Unlocks: Exosuit Overlay, Atmo Suit, Exosuit Forge, Exosuit Checkpoint, Exosuit Dock

**Advanced Automation**
Requires: Generic Sensors
Leads to: Computing
Unlocks: AND Gate, OR Gate, Buffer Gate, Filter Gate

**Parallel Automation**
Requires: Generic Sensors
Leads to: (none)
Unlocks: Ribbon Wire, Ribbon Bridge, Ribbon Writer, Ribbon Reader

**Smelting**
Requires: Refined Renovations
Leads to: Superheated Forging, Durable Life Support
Unlocks: Metal Refinery, Metal Tile

**Robotic Tools**
Requires: Artificial Friends
Leads to: (none)
Unlocks: Auto-Miner, Payload Opener, Robo-Pilot Module

**Micro-Targeted Medicine**
Requires: Pathogen Diagnostics
Leads to: Radiation Protection (SO)
Unlocks: Disease Clinic, Advanced Apothecary, Hot Tub, Radiation Sensor

**Crash Plan**
Requires: Space Program
Leads to: Durable Life Support
Unlocks: Orbital Research Point, Pioneer Module, Orbital Research Center, Cosmic Research Center

**Celestial Detection**
Requires: Notification Systems + Space Program (BG: Computing only)
Leads to: Missiles
Unlocks: Meteor Detector, Telescope, Research Module (Cluster), Enclosed Telescope, Astronaut Training Center

**Nuclear Research** (SO)
Requires: Advanced Research
Leads to: More Materials Science Research
Unlocks: Nuclear Research Point, Nuclear Research Center, Manual Radbolt Generator, Disposable Electrobank (Uranium)

### Tier 4

**Gourmet Meal Preparation**
Requires: Animal Control
Leads to: Brackene Flow
Unlocks: Gas Range, Food Dehydrator, Food Rehydrator, Deep Fryer

**Creature Comforts**
Requires: Animal Control
Leads to: (none)
Unlocks: Critter Condo, Underwater Critter Condo, Airborne Critter Condo

**Bioengineering** (SO)
Requires: Food Repurposing
Leads to: (none)
Unlocks: Genetic Analysis Station

**Advanced Caffeination**
Requires: Liquid Tuning
Leads to: Personal Flight, Missiles (BG)
Unlocks: Espresso Machine, Liquid Fuel Tank (Cluster), Mercury Ceiling Light

**Advanced Sanitation**
Requires: Liquid Distribution + Liquid-Based Refinement
Leads to: (none)
Unlocks: Decontamination Shower

**Catalytics**
Requires: HVAC
Leads to: (none)
Unlocks: Oxylite Refinery, Chlorinator, Molecular Forge, Super Liquids, Soda Fountain, Gas Cargo Bay (Cluster)

**Valve Miniaturization**
Requires: Plastic Manufacturing
Leads to: Hydrocarbon Propulsion, Brackene Flow
Unlocks: Liquid Mini Pump, Gas Mini Pump

**Renewable Energy**
Requires: Low-Resistance Conductors
Leads to: Improved Hydrocarbon Propulsion, Space Power
Unlocks: Steam Turbine, Solar Panel, Sauna, Steam Engine (Cluster)

**Space Power**
Requires: Sound Amplifiers + Low-Resistance Conductors
Leads to: (none)
Unlocks: Battery Module, Solar Panel Module, Rocket Interior Power Plug

**Glass Blowing**
Requires: Home Luxuries
Leads to: Environmental Appreciation, New Media
Unlocks: Glass Tile, Hanging Fancy Pot, Sun Lamp

**Renaissance Art**
Requires: High Culture
Leads to: New Media
Unlocks: Tall Canvas, Marble Sculpture, Fossil Display, Ceiling Fossil Display

**Transit Tubes**
Requires: Hazard Protection
Leads to: Personal Flight
Unlocks: Transit Tube Access, Transit Tube, Transit Tube Crossing, Vertical Wind Tunnel

**Computing**
Requires: Advanced Automation
Leads to: Sensitive Microimaging (SO), Multiplexing, Celestial Detection (BG)
Unlocks: Counter, Memory Toggle, XOR Gate, Arcade Cabinet, Checkpoint, Cosmic Research Center

**Solid Control**
Requires: Solid Transport
Leads to: Solid Management, High Velocity Transport
Unlocks: Solid Shutoff, Conveyor Receptacle, Solid Meter Valve, Rocket Interior Solid Input, Rocket Interior Solid Output

**Superheated Forging**
Requires: Smelting
Leads to: Pressurized Forging, Nuclear Refinement (SO)
Unlocks: Glass Forge, Bunker Tile, Bunker Door, Geo Tuner (+ Gantry in SO)

**Radiation Protection** (SO)
Requires: Micro-Targeted Medicine + More Materials Science Research + Liquid-Based Refinement
Leads to: Nuclear Refinement
Unlocks: Lead Suit, Lead Suit Checkpoint, Lead Suit Dock, HEP Sensor

**More Materials Science Research** (SO)
Requires: Nuclear Research
Leads to: Radbolt Containment, Radiation Protection
Unlocks: HEP Emitter, HEP Redirector, HEP Bridge

**Durable Life Support**
Requires: Crash Plan + Smelting
Leads to: (none)
Unlocks: Nosecone, Medium Habitat Module, Artifact Analysis Station, Artifact Cargo Bay, Special Cargo Bay (Cluster)

### Tier 5

**Brackene Flow**
Requires: Valve Miniaturization + Gourmet Meal Preparation
Leads to: (none)
Unlocks: Milk Feeder, Cream Separator, Milking Station

**Hydrocarbon Propulsion**
Requires: Valve Miniaturization
Leads to: Improved Hydrocarbon Propulsion
Unlocks: Small Petroleum Engine (Cluster), Mission Control (Cluster)

**Personal Flight**
Requires: Advanced Caffeination + Transit Tubes
Leads to: Radbolt Propulsion (SO), Solid Fuel Combustion (BG)
Unlocks: Jet Suit, Jet Suit Checkpoint, Jet Suit Dock, Liquid Cargo Bay (Cluster)

**Environmental Appreciation**
Requires: Glass Blowing
Leads to: Monuments
Unlocks: Beach Chair

**New Media**
Requires: Renaissance Art + Glass Blowing
Leads to: Monuments
Unlocks: Pixel Pack

**Multiplexing**
Requires: Computing
Leads to: (none)
Unlocks: Multiplexer, Demultiplexer

**Sensitive Microimaging** (SO)
Requires: Computing
Leads to: Data Science (SO)
Unlocks: Scanner Module, Interplanetary Transmitter, Interplanetary Receiver

**Solid Management**
Requires: Solid Control
Leads to: (none)
Unlocks: Solid Filter, Solid Pipe Temp Sensor, Solid Pipe Element Sensor, Solid Pipe Germ Sensor, Storage Tile, Cargo Bay (Cluster)

**High Velocity Transport**
Requires: Solid Control
Leads to: High Velocity Destruction (SO)
Unlocks: Railgun, Landing Beacon

**Missiles**
Requires: Celestial Detection (SO) or Advanced Caffeination (BG)
Leads to: (none)
Unlocks: Missile Fabricator, Missile Launcher

**Radbolt Containment** (SO)
Requires: More Materials Science Research
Leads to: Nuclear Refinement
Unlocks: HEP Battery

**Celestial Detection** (BG version)
Requires: Computing
Leads to: Introductory Rocketry
Unlocks: Meteor Detector, Telescope, Research Module

### Tier 5+ (Base Game Rocketry Chain)

**Introductory Rocketry** (BG)
Requires: Celestial Detection
Leads to: Solid Cargo, Solid Fuel Combustion
Unlocks: Command Module, Steam Engine, Research Module, Gantry

**Solid Cargo** (BG)
Requires: Introductory Rocketry
Leads to: Liquid and Gas Cargo
Unlocks: Cargo Bay

**Solid Fuel Combustion** (BG)
Requires: Introductory Rocketry + Personal Flight
Leads to: Hydrocarbon Combustion, Data Science (BG)
Unlocks: Solid Booster, Mission Control

**Liquid and Gas Cargo** (BG)
Requires: Solid Cargo
Leads to: Unique Cargo
Unlocks: Liquid Cargo Bay, Gas Cargo Bay

**Hydrocarbon Combustion** (BG)
Requires: Solid Fuel Combustion
Leads to: Cryofuel Combustion
Unlocks: Kerosene Engine, Biodiesel Engine, Liquid Fuel Tank, Oxidizer Tank

**Unique Cargo** (BG)
Requires: Liquid and Gas Cargo
Leads to: (none)
Unlocks: Tourism Module, Special Cargo Bay

**Cryofuel Combustion** (BG)
Requires: Hydrocarbon Combustion
Leads to: (none)
Unlocks: Liquid Oxidizer Tank, Oxidizer Tank (Cluster), Hydrogen Engine

**Data Science** (BG)
Requires: Solid Fuel Combustion
Leads to: (none)
Unlocks: Data Miner, Remote Worker Dock, Remote Work Terminal, Robo-Pilot Command Module

### Tier 5-6 (Spaced Out Late Game)

**Nuclear Refinement** (SO)
Requires: Radiation Protection + Superheated Forging + Radbolt Containment
Leads to: Radbolt Propulsion
Unlocks: Nuclear Reactor, Uranium Centrifuge, Self-Charging Electrobank

**Pressurized Forging**
Requires: Superheated Forging
Leads to: (none)
Unlocks: Diamond Press

**Improved Hydrocarbon Propulsion**
Requires: Hydrocarbon Propulsion + Renewable Energy
Leads to: Cryofuel Propulsion
Unlocks: Petroleum Engine (Cluster), Biodiesel Engine (Cluster)

**Data Science** (SO)
Requires: Sensitive Microimaging
Leads to: (none)
Unlocks: Data Miner, Remote Worker Dock, Remote Work Terminal

**High Velocity Destruction** (SO)
Requires: High Velocity Transport
Leads to: (none)
Unlocks: Harvest Nosecone

**Radbolt Propulsion** (SO)
Requires: Personal Flight + Nuclear Refinement
Leads to: Cryofuel Propulsion
Unlocks: Radbolt Engine

**Monuments**
Requires: Environmental Appreciation + New Media
Leads to: (none)
Unlocks: Great Monument Base, Great Monument Middle, Great Monument Top

### Tier 7 (Spaced Out)

**Cryofuel Propulsion** (SO)
Requires: Radbolt Propulsion + Improved Hydrocarbon Propulsion
Leads to: (none)
Unlocks: Hydrogen Engine (Cluster), Liquid Oxidizer Tank (Cluster)

## Notable Dependency Chains

Here are some of the longer chains to illustrate the planning depth:

**Nuclear Reactor (SO)**: Power Regulation > Internal Combustion > Advanced Power Regulation > Low-Resistance Conductors > (joins with) Employment > Advanced Research > Nuclear Research > More Materials Science Research > (joins with several other branches for Radiation Protection) > Nuclear Refinement. This is one of the deepest chains, requiring progress through power, research, medicine, and liquids branches.

**Cryofuel Propulsion (SO)**: Requires completing both the hydrocarbon engine chain (through Power > Combustion > Plastics > Valve Miniaturization > Hydrocarbon > Improved Hydrocarbon) and the nuclear propulsion chain (through the entire nuclear branch). Tier 7, the deepest tech in Spaced Out.

**Monuments**: Requires completing both the decor chain (Interior Decor > Artistic Expression > all the way through Renaissance Art) and the home luxury chain (through Glass Blowing and Environmental Appreciation). A cosmetic endgame goal that requires deep investment in the decor branch.

**Jet Suit**: Requires converging the liquids branch (Plumbing > Filtration > Improved Plumbing > Liquid Tuning > Advanced Caffeination) with the exosuit branch (Interior Decor > Artistic Expression > Textile Production > Hazard Protection > Transit Tubes). Two completely separate branches must meet.

## Graph Structure Summary

- 7 root techs (tier 0, no prerequisites)
- Most techs have 1 prerequisite; about 25% have 2; a handful have 3
- The tree is 8 tiers deep in Spaced Out, 10 tiers deep in the base game (due to the linear rocketry chain)
- At any point in the game, roughly 10-15 techs are directly available to research (prerequisites complete but tech not yet researched)
- The tree fans out quickly from tier 0 to tier 2-3 (7 > 12 > 20-24 > 18-21 techs) then narrows at higher tiers
- Multiple branches frequently converge at high-tier techs, creating decision points about which prerequisite chains to invest in first
