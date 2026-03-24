# Debug Commands

Oxygen Not Included features a Debug Menu capable of viewing and generating reports of data as well as exploiting and cheating (for those who wish to use it in this way). If debugging lacks a capability, then you need try the save file editor
Duplicity
.
One issue is the loss of game elements such as geysers due to point of interest buildings, and the debug commands can be used to "patch" a world seed that was unfortunate enough to lose game elements in this way. This is not necessarily cheating but rather allowing a player to continue as if they had a normal seed.
The Debug Menu is also currently the only stable means of patching new content into game worlds that have been generated prior to the content updates that introduce them, and which did not enable Sandbox mode.
However, only loose materials can be spawned. Things like plants and critters cannot be directly created, at least not without Sandbox Mode enabled.
Activating Debug Menu
Currently, the Debug Menu is
not activated by default
and must be enabled. Below is the steps necessary to allow access to the menu.
Using The Debug File
Create a text file anywhere on your computer and save the file name as
debug_enable (if you have file extensions enabled, save it as debug_enable.txt)
. The file doesn't need any content, it's just the name that matters.
Find the installation folder/directory for Oxygen Not Included. (Steam Users can find their files by right-clicking and viewing the properties of Oxygen Not Included in their library, then navigating to "Local Files" and "Browse Local Files".)
For Windows and Linux users, move "debug_enable.txt" inside the same folder where the executable is located ("OxygenNotIncluded.exe" on Windows, "OxygenNotIncluded" on Linux).
For Mac OS users, open the application bundle by right-clicking and then selecting "Open Package Contents". Place "debug_enable.txt" inside the "Contents" folder.
Restart Oxygen Not Included and start a game. (New games are advised, but the menu works on old saves too.)
Temporary Cheat Code
This method allows you to access debug mode temporarily without creating a file or restarting the game.  Access will last until the game is closed.
Go to the main menu (Just loading the game brings you there, or you can quit to the main menu from a save)
Type the letters `KLEIPLAY` in order
You can check that it worked because it will append a '-D'  to the build number shown on the bottom left of the screen.
Debug mode is activated until you close the game completely
Debug Menu Effects
Enabling debug mode allows certain things to be done.  On macOS
Ctrl+Fn
is taken by the OS so use
Cmd+Ctrl+Fn
instead.
Debug Mode Buildings
Debug mode being enabled unlocks several special buildings.
Dev Generator
produces a large amount of power with no inputs or duplicant work.
Dev Pump Liquid
produces any liquid on demand. If nothing is selected, it starts with water.
Dev Pump Gas
produces any gas on demand. If nothing is selected, it starts with oxygen.
Dev Pump Solid
produces solid (debris) on demand.
Dev Life Support
produces
30 °C
/
86 °F
oxygen and deletes carbon dioxide produced by the duplicants.
Dev Light Source
produces light.
Dev Heater
produces heat on demand.
Dev Radbolt Generator
produces radbolts on demand (only available on
Spaced Out
).
Dev Radiation Emitter
produces radiation on demand (only available on
Spaced Out
).
This is frequently useful for testing systems in a test world without needing to set up things properly.
Primary debugging mode (Backspace)
The primary debugging mode is turned on and off with Backspace (the first time it's turned on after loading a game it might take a few seconds to appear, depending upon the speed of your computer). This mode does the following:
Turns off fog-of-war.
Lets you zoom out as far as you want.
Allows you to "paint" solids, liquids and gases. (Painting replaces existing matter with the new matter you intend to paint)
Lets you copy/paste areas of a world, including buildings with their settings/contents and critters, but not including dupes.
Lets you save areas of a world as a template file, then later load and paste the template.
Debug Instant Build Mode (Ctrl+F4)
Toggling this mode on has several effects:
When you place an order for something to be built it will appear instantly without using up any existing resources.
All building and construction types become available, even if they haven't been researched yet.
Any new dig orders will dig out the blocks instantly, though if the game is paused you'll have to unpause for it to take effect.
For any buildings where you can queue up orders for items to be produced (
Microbe Musher
,
Apothecary
, etc), queuing up and order with "+1" will cause the request item to be instantly produced, even if the building is receiving no power.
Duplicants can be assigned any
Skill
, even if they don't meet the requirements for the skill, and will retain that skill after Instant Build Mode is toggled off.
If you select a
Research
subject that hasn't yet been fully researched it will become fully researched, along with any prerequisite research subject. These research subjects will stay fully researched after Instant Build Mode is toggled off.
All
Lore
entries are revealed in the Index. This includes
Story Traits
. This will not permanently unlock the entries, and will go back to only the previously unlocked entries after disabling Instant Build Mode.
Debug Teleport (Alt+Q)
Teleports the selected dupe, critter, plant or building to the location of the mouse cursor.
Debug Spawn Duplicant (Ctrl+F2)
Spawns a new randomly generated dupe at the location of the mouse cursor.
Debug Spawn Duplicant in Atmosuit (Alt+F2)
Spawns a new randomly generated dupe in a filled Atmosuit at the location of the mouse cursor.
Debug Ultra Test Mode (Ctrl+U)
Toggling this mode on makes the game run as fast as your computer's CPU can handle.
Debug Invincible (ALT+F7)
Toggling this mode on will make your dupes invincible.
Debug Menu keys
See
Controls#Debug
