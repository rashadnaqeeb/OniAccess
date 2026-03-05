namespace STRINGS {
	// Translation notes:
	// - These strings are spoken by a screen reader, not displayed visually.
	// - Game terms (Duplicant, Cycle, Block, Morale, Decor, Errand, Chore)
	//   must match the base game's official translation for the target language.
	// - Consult the game's .po translation files for canonical term translations.
	public class ONIACCESS {
		// Spoken descriptions of sprite/icon overlays
		public class SPRITES {
			public static LocString WARNING = "warning:";
			// Automation wire signal states
			public static LocString LOGIC_GREEN = "green signal";
			public static LocString LOGIC_RED = "red signal";
		}

		// System-level speech announcements
		public class SPEECH {
			// {0} = mod version string (e.g. "1.2.3")
			public static LocString MOD_LOADED = "Oni-Access version {0} loaded";
			public static LocString MOD_ON = "Oni-Access on";
			public static LocString MOD_OFF = "Oni-Access off";
			public static LocString NO_COMMANDS = "No commands available in this context";
			// {0} = screen handler display name (e.g. "Key Bindings", "Minion Select")
			public static LocString HANDLER_FAILED = "Error, {0} failed";
		}

		// Type-ahead search feedback
		public class SEARCH {
			public static LocString CLEARED = "Search cleared";
			// {0} = the user's search query text
			public static LocString NO_MATCH = "No match for {0}";
		}

		// Hotkey action descriptions shown in key binding UI
		public class HOTKEYS {
			public static LocString TOGGLE_MOD = "Toggle Oni-Access on/off";
		}

		// Screen handler display names, announced when a screen opens
		public class HANDLERS {
			public static LocString LOADING = "Loading";
			public static LocString HELP = "Help";
			public static LocString MAIN_MENU = "Main menu";
			public static LocString PAUSE_MENU = "Pause menu";
			public static LocString AUDIO_OPTIONS = "Audio options";
			public static LocString GRAPHICS_OPTIONS = "Graphics options";
			public static LocString GAME_OPTIONS = "Game options";
			public static LocString COLONY_SUMMARY = "Colony summary";
			public static LocString WORLD_GEN = "Generating world";
			public static LocString MINION_SELECT = "Select duplicants";
			public static LocString SAVE_LOAD = "Save and load";
			public static LocString MODS = "Mods";
			public static LocString TRANSLATIONS = "Translations";
			public static LocString DATA_OPTIONS = "Data options";
			public static LocString FEEDBACK = "Feedback";
			public static LocString KEY_BINDINGS = "Key bindings";
			public static LocString SUPPLY_CLOSET = "Supply closet";
			public static LocString ITEM_DROP = "Claim blueprints";
			public static LocString WELCOME_MESSAGE = "Welcome message";
			public static LocString STORY_MESSAGE = "Story message";
			public static LocString VIDEO = "Video";
			public static LocString COLONY_VIEW = "Colony View";
			public static LocString ENTITY_PICKER = "object selection";
			public static LocString DETAILS_SCREEN = "Entity details";
			public static LocString PRINTING_POD = "Printing pod";
			public static LocString ERROR_SCREEN = "Error";
		}

		// Supply closet (Klei rewards) screen messages
		public class SUPPLY_CLOSET {
			public static LocString NO_ITEMS = "No items to claim";
			public static LocString OFFLINE = "Not connected to server";
		}

		// Help overlay key descriptions
		public class HELP {
			public static LocString NAVIGATE = "Step through help entries";
			public static LocString CLOSE = "Close";
			public static LocString NAVIGATE_ITEMS = "Navigate items";
			public static LocString JUMP_FIRST_LAST = "Jump to first or last";
			public static LocString SELECT_ITEM = "Activate selected item";
			public static LocString ADJUST_VALUE = "Adjust value by 1 or 1 percent";
			public static LocString ADJUST_VALUE_LARGE = "Adjust value by 10 or 10 percent";
			public static LocString ADJUST_VALUE_LARGER = "Adjust value by 100 or 25 percent";
			public static LocString ADJUST_VALUE_LARGEST = "Adjust value by 1000 or 50 percent";
			public static LocString TYPE_SEARCH = "Type-ahead search";
			public static LocString SWITCH_PANEL = "Switch panel";
			public static LocString SWITCH_SECTION = "Switch section";
			public static LocString COPY_SETTINGS = "Copy settings";
			public static LocString SWITCH_DUPE_SLOT = "Switch duplicant slot";
			public static LocString SWITCH_OPTION = "Switch option";
			public static LocString MOVE_CURSOR = "Move tile cursor";
			public static LocString READ_COORDS = "Read coordinates";
			public static LocString CYCLE_COORD_MODE = "Cycle coordinate display";
			public static LocString READ_TOOLTIP_SUMMARY = "Read tooltip summary at cursor";
			public static LocString CYCLE_GAME_SPEED = "Cycle game speed";
			public static LocString SELECT_ENTITY = "Select object at cursor";
			public static LocString OPEN_GROUP = "Open group";
			public static LocString GO_BACK = "Go back";
			public static LocString JUMP_GROUP = "Jump to next or previous group";
			public static LocString CYCLE_RECIPE = "Cycle recipe";

			// Help entries for tool-specific keys
			public class TOOLS_HELP {

				public static LocString SET_CORNER = "Set rectangle corner";
				public static LocString CLEAR_RECT = "Clear rectangle at cursor";
				public static LocString CONFIRM_TOOL = "Apply tool at cursor and dismiss";
				public static LocString APPLY_SETTINGS = "Apply settings";
				public static LocString APPLY_AND_EXIT = "Apply settings and exit";
				public static LocString CANCEL_TOOL = "Cancel tool";
				public static LocString SET_PRIORITY = "Set priority";
				public static LocString OPEN_FILTER = "Change filter";
			}
		}

		// Crew assignment screen labels
		public class CREW_SCREEN {
			public static LocString AVAILABLE = "Available";
			// {0} = number of assigned dupes (int)
			public static LocString ASSIGNED_COUNT_FORMAT = "{0} assigned";
			// {0} = total assigned count after toggle (int)
			public static LocString TOTAL_FORMAT = "{0} total";
		}

		// Recipe queue screen labels
		public class RECIPE {
			// {0} = queue count number or "Forever"
			public static LocString QUEUE_COUNT = "Queue: {0}";
		}

		// Select module side screen section headers
		public class MODULE_SCREEN {
			public static LocString MODULES = "Modules";
			public static LocString MATERIALS = "Materials";
			public static LocString FACADE = "Skin";
		}

		// Labels for colony summary statistics
		public class COLONY_STATS {
			// Label preceding the most recent cycle's value (e.g. "Population, last cycle 12")
			public static LocString LAST_CYCLE = "last cycle";
			// Label preceding the all-time high value (e.g. "peak 15")
			public static LocString PEAK = "peak";
		}

		// Panel/tab names announced when switching UI sections
		public class PANELS {
			public static LocString SEED = "World seed";
			public static LocString ACHIEVEMENTS = "Achievements";
			public static LocString VICTORY_CONDITIONS = "Victory conditions";
			public static LocString STATS = "Stats";
			public static LocString BUTTONS = "Buttons";
			public static LocString PLANETOIDS = "Planetoids";
			public static LocString DLC = "DLC";
			public static LocString NEWS = "News";
			public static LocString NO_NEWS = "No news available";
			public static LocString COLONY_NAME = "Colony name";
			public static LocString SELECT_DUPLICANTS = "Select duplicants";
			public static LocString RENAME = "Rename";
			public static LocString SHUFFLE_NAME = "Shuffle name";
		}

		// DLC ownership status labels
		public class DLC {
			public static LocString ACTIVE = "Active";
			public static LocString OWNED_NOT_ACTIVE = "Owned, not active";
			public static LocString NOT_OWNED = "Not owned";
		}

		// Text input field feedback
		public class TEXT_EDIT {
			// Announced when entering text edit mode
			public static LocString EDITING = "Editing";
			public static LocString CANCELLED = "Cancelled";
			public static LocString COPIED = "Copied";
			public static LocString PASTED = "Pasted";
		}

		// Standalone state labels appended to item names.
		// These are single words/phrases spoken after an item name
		// to describe its current state (e.g. "Algae Distiller, selected").
		public class STATES {
			public static LocString SELECTED = "selected";
			// World trait is guaranteed to appear on the asteroid
			public static LocString GUARANTEED = "present";
			// World trait is excluded from the asteroid
			public static LocString FORBIDDEN = "not present";
			public static LocString ON = "on";
			public static LocString OFF = "off";
			// Checkbox with mixed state (some children on, some off)
			public static LocString MIXED = "mixed";
			public static LocString ENABLED = "enabled";
			public static LocString DISABLED = "disabled";
			// Filter dropdown: no filter applied
			public static LocString ANY = "Any";
			// Filter dropdown: nothing selected
			public static LocString NONE = "None";
			// Victory/achievement condition status
			public static LocString CONDITION_MET = "met";
			public static LocString CONDITION_MET_OTHER = "met by past colony";
			public static LocString CONDITION_NOT_MET = "not met";
			public static LocString CONDITION_FAILED = "failed";
			// Widget type labels announced for focus context
			public static LocString INPUT_FIELD = "input field";
			public static LocString SLIDER = "slider";
			// Item availability (e.g. blueprint available to claim)
			public static LocString AVAILABLE = "available";
			public static LocString LOCKED = "locked";
			public static LocString ASSIGNED = "assigned";
			public static LocString UNASSIGNED = "unassigned";
			public static LocString QUEUED = "queued";
		}

		// Receptacle (e.g. Display Shelf, Flower Pot) side screen
		public class RECEPTACLE {
			// {0} = number of depositable items (int)
			public static LocString ITEM_COUNT = "{0} items";
		}

		// Fabricator (e.g. Rock Crusher, Kiln) side screen
		public class FABRICATOR {
			// {0} = queue count from the fabricator's UI label (int)
			public static LocString QUEUED = "{0} queued";
			public static LocString CONTINUOUS = "continuous";
			public static LocString NOT_QUEUED = "not queued";
			// Recipe cannot be fabricated (missing materials or research)
			public static LocString UNAVAILABLE = "unavailable";
		}

		public class GEOTUNER {
			// {0} = number of geotuners targeting this geyser
			public static LocString TUNER_COUNT = "{0} tuners";
		}

		// Button labels for actions not covered by game strings
		public class BUTTONS {
			public static LocString ACCEPT = "Accept";
			public static LocString MANAGE = "Manage";
			public static LocString VIEW_OTHER_COLONIES = "View other colonies";
			public static LocString TOGGLE_ALL = "Toggle all";
			public static LocString MOVE_UP = "Move up";
			public static LocString MOVE_DOWN = "Move down";
		}

		// World generation progress screen
		public class WORLD_GEN {
			public static LocString COMPLETE = "World generation complete";
			// {0} = percentage complete (int, 0-100)
			public static LocString PERCENT = "{0} percent";
		}

		// Labels used in the duplicant selection and game setup screens
		public class INFO {
			public static LocString DIFFICULTY = "Difficulty";
			public static LocString STORY_TRAITS = "Story traits";
			// Suffix for game setting names
			public static LocString SETTING = "setting";
			// Suffix for achievement names
			public static LocString ACHIEVEMENT = "achievement";
			// Duplicant skill interest label
			public static LocString INTEREST = "Interest";
			public static LocString INTEREST_FILTER = "Interest filter";
			// Duplicant trait type labels
			public static LocString TRAIT = "Trait";
			public static LocString POSITIVE_TRAIT = "Positive trait";
			public static LocString NEGATIVE_TRAIT = "Negative trait";
			public static LocString BIONIC_UPGRADE = "Bionic upgrade";
			public static LocString BIONIC_BUG = "Bionic bug";
			// {0} = duplicant selection slot number (1-indexed int)
			public static LocString SLOT = "Slot {0}";
			// Prefix for listing resources the colony already has
			public static LocString COLONY_HAS = "Colony has: ";
		}

		// Save/load screen labels
		public class SAVE_LOAD {
			public static LocString SAVE_INFO = "Save info";
			public static LocString CONVERT_ALL_TO_CLOUD = "Convert all to cloud";
			public static LocString CONVERT_ALL_TO_LOCAL = "Convert all to local";
			public static LocString DELETE = "Delete";
			// Tag appended to the most recent save file
			public static LocString NEWEST = "newest";
			// Tag appended to auto-save files
			public static LocString AUTO_SAVE = "auto-save";
		}

		// Key bindings screen
		public class KEY_BINDINGS {
			// Announced when a key action has no binding
			public static LocString UNBOUND = "Unbound";
			// {0} = action name being rebound (e.g. "Move cursor left")
			public static LocString PRESS_KEY_FOR = "Press a key for {0}";
			public static LocString RESET_ALL = "Reset all to defaults";
			public static LocString BINDINGS_RESET = "All bindings reset to defaults";
		}

		// Big cursor area survey
		public class BIG_CURSOR {
			public static LocString HELP_CYCLE_SIZE = "Increase or decrease cursor size";
			// {0} = dimension (int), e.g. "3x3"
			public static LocString SIZE_FORMAT = "{0}x{0}";
			// {0} = percent (int)
			public static LocString UNEXPLORED_PCT = "{0}% unexplored";
			// {0} = element name (string), {1} = percent (int)
			public static LocString ELEMENT_PCT = "{0} {1}%";
			// {0} = element name (string), {1} = percent (int), {2} = formatted mass (string)
			public static LocString ELEMENT_MASS_PCT = "{0} {1}%: {2}";
			// {0} = count (int), {1} = building name (string)
			public static LocString BUILDING_COUNT = "{0} {1}";
			// {0} = count (int)
			public static LocString DUPE_SINGULAR = "{0} dupe";
			// {0} = count (int)
			public static LocString DUPE_PLURAL = "{0} dupes";
			// {0} = count (int)
			public static LocString CRITTER_SINGULAR = "{0} critter";
			// {0} = count (int)
			public static LocString CRITTER_PLURAL = "{0} critters";
			// {0} = count (int), {1} = order type (string)
			public static LocString ORDER_COUNT = "{0} {1}";
			// {0} = formatted temperature (string)
			public static LocString AVG_TEMPERATURE = "average {0}";
			// {0} = formatted lux (string)
			public static LocString AVG_LUX = "average {0}";
			// {0} = sign+value (string, e.g. "+12" or "-5")
			public static LocString AVG_DECOR = "average {0} decor";
			// {0} = formatted rads (string)
			public static LocString AVG_RADS = "average {0}";
			// {0} = germ type name (string), {1} = formatted germ count (string)
			public static LocString AVG_DISEASE = "{0} {1}";
			public static LocString DISEASE_CLEAR = "no germs";
			// {0} = count (int), {1} = plant name (string), {2} = avg growth percent (int)
			public static LocString PLANT_ENTRY = "{0} {1}, {2}% grown";
			public static LocString NO_PLANTS = "no plants";
			// {0} = comma-separated room list (string)
			public static LocString ROOMS_LIST = "{0}";
			// {0} = count (int)
			public static LocString UNCATEGORIZED_ROOMS = "{0} uncategorized";
			public static LocString NO_ROOMS = "no rooms";
			public static LocString EMPTY = "empty";
			public static LocString SCAN_ERROR = "scan error";
			public static LocString SOLID = "solid";
			public static LocString LIQUID = "liquid";
			public static LocString GAS = "gas";
			public static LocString VACUUM = "vacuum";
		}

		// Tile cursor navigation and coordinate display
		public class TILE_CURSOR {
			// Announced when cursor enters an unexplored map tile
			public static LocString UNEXPLORED = "unexplored";
			// {0} = X coordinate (int), {1} = Y coordinate (int)
			public static LocString COORDS = "{0}, {1}";
			// Coordinate display mode labels
			public static LocString COORD_OFF = "coordinates off";
			public static LocString COORD_APPEND = "coordinates append";
			public static LocString COORD_PREPEND = "coordinates prepend";
			public static LocString OVERLAY_NONE = "default view";
			// Announced when cursor tile is not inside any defined room
			public static LocString NO_ROOM = "no room";
			public static LocString NOTHING_TO_SELECT = "nothing to select";
			// Prompt when multiple objects occupy the cursor tile
			public static LocString SELECT_OBJECT = "select an object";

			// Spelled-out name of the period/full-stop key (the punctuation mark).
			// Localizers: this is the keyboard key ".", not an abbreviation.
			public static LocString KEY_PERIOD = "period";

			// Help descriptions for base game management screen hotkeys.
			// These are not mod keys — listed here so blind players can discover them.
			public class MANAGEMENT_HELP {
				public static LocString PRIORITIES = "Priorities";
				public static LocString CONSUMABLES = "Consumables";
				public static LocString VITALS = "Vitals";
				public static LocString RESEARCH = "Research";
				public static LocString SCHEDULE = "Schedule";
				public static LocString SKILLS = "Skills";
				public static LocString COLONY_REPORT = "Colony report";
				public static LocString DATABASE = "Database";
				public static LocString STARMAP = "Starmap";
			}
		}

		public class VIDEO {
			public static LocString PLAYING = "Video playing";
		}

		// Dupe cycle navigator ([ / ] / \ keys)
		public class DUPES {
			public static LocString NO_DUPLICANTS = "no duplicants";
			public static LocString IDLE = "idle";
			public static LocString INCAPACITATED = "incapacitated";
			public static LocString HEALTH_CRITICAL = "critical health";
			public static LocString HEALTH_INJURED = "injured";
			public static LocString SICK = "sick";
			public static LocString KEY_BRACKETS = "Left bracket / Right bracket";

			public static LocString HELP_CYCLE = "Cycle through duplicants";
			public static LocString HELP_JUMP = "Jump cursor to current duplicant, or open details";
		}

		// Game clock and speed announcements
		public class GAME_STATE {
			// {0} = speed name (e.g. "1x", "2x", "3x")
			public static LocString UNPAUSED = "unpaused, {0}";
			// {0} = cycle number (int)
			public static LocString CYCLE = "Cycle {0}";
			// {0} = cycle number (int), {1} = schedule block/hour (int, 0-23)
			public static LocString CYCLE_STATUS = "Cycle {0}, block {1}";
			public static LocString READ_CYCLE_STATUS = "Read cycle status";
			public static LocString READ_TIME_PLAYED = "Read total playtime";
			public static LocString RED_ALERT_OFF = "Red Alert off";
			public static LocString YELLOW_ALERT_OFF = "Yellow Alert off";
			public static LocString TOGGLE_RED_ALERT = "Toggle red alert";
			public static LocString READ_COLONY_STATUS = "Read colony status";
			// {0} = dupe count
			public static LocString DUPES = "{0} dupes";
			// {0} = local world dupe count, {1} = total dupe count
			public static LocString DUPES_CLUSTER = "{0}/{1} dupes";
			// {0} = sick count
			public static LocString SICK = "{0} sick";
			// {0} = formatted kcal string from GameUtil.GetFormattedCalories
			public static LocString RATIONS = "{0}";
			// {0} = stress percentage (int)
			public static LocString STRESS = "{0}% stress";
			// {0} = formatted joules string from GameUtil.GetFormattedJoules
			public static LocString ELECTROBANKS = "{0}";
		}

		// Tooltip summary readout at cursor
		public class TOOLTIP {
			public static LocString NO_TOOLTIP = "no tooltip";
			public static LocString CLOSED = "closed";
			public static LocString CANNOT_CONTINUE = "cannot continue";
			public static LocString CONTINUING = "continuing";
		}

		// Tool activation, selection feedback, and work order confirmations
		public class TOOLS {
			// Tool picker/filter panel names
			public static LocString PICKER_NAME = "tool menu";
			public static LocString FILTER_NAME = "tool filter";
			// Singular/plural for generic counted items in tool confirmations
			public static LocString ITEM_SINGULAR = "item";
			public static LocString ITEM_PLURAL = "items";
			// Announced when first rectangle corner is placed
			public static LocString CORNER_SET = "corner set";
			// {0} = width (int), {1} = height (int), {2} = valid cell count (int)
			public static LocString RECT_SUMMARY = "{0}x{1}, {2} valid";
			// {0} = width (int), {1} = height (int), {2} = valid cell count (int), {3} = invalid cell count (int)
			public static LocString RECT_SUMMARY_INVALID = "{0}x{1}, {2} valid, {3} invalid";
			public static LocString SELECTED = "selected";
			public static LocString CANCELED = "canceled";
			public static LocString NO_VALID_CELLS = "no valid cells";
			// Tool confirmation messages after applying a tool to a selection.
			// {0} = cell/item count (int), {1} = priority level (int), {2} = item type word (singular or plural)
			public static LocString CONFIRM_DIG = "marked {0} for digging at priority {1}";
			public static LocString CONFIRM_MOP = "marked {0} for mopping at priority {1}";
			public static LocString CONFIRM_DISINFECT = "marked {0} for disinfection at priority {1}";
			public static LocString CONFIRM_SWEEP = "marked {0} for sweeping at priority {1}";
			public static LocString CONFIRM_DECONSTRUCT = "marked {0} for deconstruction at priority {1}";
			// {0} = count (int), {2} = item type word (singular or plural). No {1} used.
			public static LocString CONFIRM_CANCEL = "cancelled {0} {2}";
			// {0} = count (int), {1} = priority level (int), {2} = item type word (singular or plural)
			public static LocString CONFIRM_PRIORITIZE = "updated {0} {2} to priority {1}";
			// {0} = plant count (int), {1} = priority level (int)
			public static LocString CONFIRM_HARVEST = "set harvest on {0} plants at priority {1}";
			// {0} = count (int), {1} = priority level (int)
			public static LocString CONFIRM_ATTACK = "marked {0} for attack at priority {1}";
			public static LocString CONFIRM_CAPTURE = "marked {0} for capture at priority {1}";
			public static LocString CONFIRM_EMPTY_PIPE = "marked {0} pipe cells for emptying at priority {1}";
			// {0} = segment count (int)
			public static LocString CONFIRM_DISCONNECT = "disconnected {0} segments";
			// {0} = priority level (int)
			public static LocString PRIORITY_BASIC = "priority {0}";
			public static LocString PRIORITY_EMERGENCY = "emergency priority";
			// Tile cursor: existing work order labels.
			// {0} = priority level (int) when present
			public static LocString DIG_ORDER = "dig order";
			public static LocString DIG_ORDER_PRIORITY = "dig order, priority {0}";
			public static LocString MOP_ORDER = "mop order";
			public static LocString MOP_ORDER_PRIORITY = "mop order, priority {0}";
			public static LocString MARKED_DISINFECT = "marked for disinfect";
			public static LocString MARKED_SWEEP = "marked for sweep";
			public static LocString MARKED_SWEEP_PRIORITY = "marked for sweep, priority {0}";
			public static LocString MARKED_ATTACK = "marked for attack";
			public static LocString MARKED_CAPTURE = "marked for capture";
			public static LocString MARKED_DECONSTRUCT = "marked for deconstruct";
			public static LocString MARKED_DECONSTRUCT_PRIORITY = "marked for deconstruct, priority {0}";
			public static LocString MARKED_EMPTY = "marked for emptying";
			// Disinfect tool: object with disease info.
			// {0} = object name, {1} = disease name, {2} = disease germ count (int)
			public static LocString DISINFECT_OBJECT = "{0}, {1}, {2}";
			// Empty pipe tool: pipe with no contents.
			// {0} = pipe type name (e.g. "liquid", "gas")
			public static LocString PIPE_EMPTY = "{0}, empty";
			// Empty pipe tool: pipe with contents.
			// {0} = pipe type name, {1} = element name, {2} = formatted mass
			public static LocString PIPE_CONTENTS = "{0}, {1}, {2}";
			public static LocString DISCONNECT_TOO_FAR = "adjacent cells only";
			// Filter state change announcements
			public static LocString FILTER_CHANGED = "filter changed";
			public static LocString FILTERED = "filtered";
			public static LocString FILTER_REMOVED = "filter removed";
			public static LocString SELECTION_CLEARED = "selection cleared";
			public static LocString RECT_CLEARED = "rectangle cleared";
			// Tool activation announcements.
			// {0} = tool name, {1} = filter or priority text, {2} = priority text
			public static LocString ACTIVATION_PLAIN = "{0} tool";
			public static LocString ACTIVATION = "{0} tool, {1}";
			public static LocString ACTIVATION_WITH_FILTER = "{0} tool, {1}, {2}";
			public static LocString FALLBACK_LABEL = "tool";
			// {0} = entity name being moved to
			public static LocString MOVE_TO_ACTIVATION = "move to, {0}";
			public static LocString MOVE_TO_CONFIRMED = "destination set";
			public static LocString MOVE_TO_UNREACHABLE = "unreachable";
			// {0} = source building name
			public static LocString COPY_SETTINGS_ACTIVATION = "{0}, copy settings";
			public static LocString COPY_SETTINGS_NO_TARGET = "no matching building";
			public static LocString COPY_SETTINGS_UNAVAILABLE = "no copyable settings";
			// {0} = item being placed
			public static LocString PLACE_ACTIVATION = "place, {0}";
			public static LocString PLACE_CONFIRMED = "placed";
			public static LocString PLACE_INVALID = "invalid location";
			public static LocString DONE = "done";
		}

		// Temperature warning labels on tile cursor
		public class TEMPERATURE {
			public static LocString NEAR_FREEZING = "near freezing";
			public static LocString NEAR_BOILING = "near boiling";
		}

		// Tile cursor glance info: brief summaries of what occupies a tile
		public class GLANCE {
			// {0} = building name being constructed
			public static LocString UNDER_CONSTRUCTION = "constructing {0}";

			// Building port type labels
			public static LocString POWER_INPUT = "power input";
			public static LocString POWER_OUTPUT = "power output";
			public static LocString GAS_INPUT = "gas input";
			public static LocString GAS_OUTPUT = "gas output";
			public static LocString LIQUID_INPUT = "liquid input";
			public static LocString LIQUID_OUTPUT = "liquid output";
			public static LocString SOLID_INPUT = "conveyor input";
			public static LocString SOLID_OUTPUT = "conveyor output";
			public static LocString INPUT_PORT = "input";
			public static LocString OUTPUT_PORT = "output";

			// Work order labels on tiles
			public static LocString ORDER_BUILD = "build";
			public static LocString ORDER_DIG = "dig";
			public static LocString ORDER_MOP = "mop";
			public static LocString ORDER_SWEEP = "sweep";
			public static LocString ORDER_DECONSTRUCT = "deconstruct";
			public static LocString ORDER_HARVEST = "harvest";
			public static LocString ORDER_UPROOT = "uproot";
			public static LocString ORDER_DISINFECT = "disinfect";
			public static LocString ORDER_ATTACK = "attack";
			public static LocString ORDER_CAPTURE = "capture";
			public static LocString ORDER_EMPTY_PIPE = "empty pipe";
			// {0} = order type label (e.g. "dig"), {1} = priority level (int)
			public static LocString ORDER_PRIORITY = "{0} priority {1}";
			// {0} = order label (e.g. "dig priority 5")
			public static LocString ORDER_UNREACHABLE = "unreachable {0}";

			// Point-of-interest marker on a tile
			public static LocString TILE_OF_INTEREST = "P O I";
			// Rocket access/output point labels
			public static LocString ACCESS_POINT = "access point";
			public static LocString OUTPUT_POINT = "output point";

			// Radbolt (radiation bolt) port labels
			public static LocString RADBOLT_INPUT = "radbolt input";
			public static LocString RADBOLT_OUTPUT = "radbolt output";

			// Conduit/network type labels
			public static LocString CONDUIT_LIQUID = "liquid";
			public static LocString CONDUIT_GAS = "gas";
			public static LocString CONDUIT_SOLID = "solid";
			public static LocString WIRE = "wire";
			public static LocString UNKNOWN_ELEMENT = "unknown";

			// Semantic port qualifiers: {0} = base port label (e.g. "gas output")
			public static LocString FILTERED_PORT = "filtered {0}";
			public static LocString OVERFLOW_PORT = "overflow {0}";
			public static LocString PRIORITY_PORT = "priority {0}";

			// {0} = port type label, {1} = port number (int, when building has multiple ports of same type)
			public static LocString NUMBERED_PORT = "{0} {1}";

			// Building extension cells: {0} = building name
			public static LocString INTAKE_PIPE = "{0} intake pipe";
			public static LocString LURE = "{0} lure";

			// Decor overlay value. {0} = sign prefix ("+" or ""), {1} = decor value (int)
			public static LocString OVERLAY_DECOR = "{0}{1} decor";
			// Disease overlay: tile is clean
			public static LocString DISEASE_CLEAR = "clean";
			// {0} = disease name, {1} = formatted germ count
			public static LocString DISEASE_ENTRY = "{0}, {1}";
			// {0} = element name, {1} = formatted mass
			public static LocString ELEMENT_MASS = "{0}, {1}";
			// Conduit connection directions. {0} = comma-joined direction list (e.g. "up, left")
			public static LocString CONNECTS = "connects {0}";
		}

		public class SCANNER {
			// Confirmation
			public static LocString REFRESHED = "scanned";
			public static LocString EMPTY = "no results";
			// Announced when a previously scanned entity no longer exists
			public static LocString INVALID = "gone";

			// Announcement format: {0} = name/label, {1} = distance, {2} = index-of-count
			public static LocString INSTANCE_WITH_DISTANCE = "{0}, {1}, {2}";
			// Announcement format: {0} = name/label, {1} = index-of-count
			public static LocString INSTANCE_NO_DISTANCE = "{0}, {1}";
			// {0} = tile count, {1} = item name
			public static LocString CLUSTER_LABEL = "{0} {1}";
			// {0} = order type, {1} = target name
			public static LocString ORDER_LABEL = "{0} {1}";
			// {0} = tile count, {1} = order type, {2} = target name
			public static LocString ORDER_CLUSTER_LABEL = "{0} {1} {2}";
			// {0} = tile count, {1} = order type (no target)
			public static LocString ORDER_CLUSTER_COUNT = "{0} {1}";
			// {0} = current index (int), {1} = total count (int)
			public static LocString INSTANCE_OF = "{0} of {1}";
			public static LocString MIXED = "mixed";

			// Must match the suffix used by STRINGS.SUBWORLDS.*.NAME (e.g., "Forest Biome")
			public static LocString BIOME_SUFFIX = " Biome";
			public static LocString BOTTLE_PREFIX = "Bottled ";
			public static LocString LOOSE_PREFIX = "Loose ";

			// Direction tokens
			public static LocString DIRECTION_UP = "up";
			public static LocString DIRECTION_DOWN = "down";
			public static LocString DIRECTION_LEFT = "left";
			public static LocString DIRECTION_RIGHT = "right";

			// Distance templates: {0} = tile count, {1} = direction word
			public static LocString DISTANCE_VERTICAL = "{0} {1}";
			public static LocString DISTANCE_HORIZONTAL = "{0} {1}";
			// {0} = vertical distance, {1} = horizontal distance
			public static LocString DISTANCE_BOTH = "{0} {1}";

			// Scanner category names
			public class CATEGORIES {
				public static LocString SOLIDS = "Solids";
				public static LocString LIQUIDS = "Liquids";
				public static LocString GASES = "Gases";
				public static LocString BUILDINGS = "Buildings";
				public static LocString NETWORKS = "Networks";
				public static LocString AUTOMATION = "Automation";
				public static LocString DEBRIS = "Debris";
				public static LocString ZONES = "Zones";
				public static LocString LIFE = "Life";
				public static LocString SEARCH = "Search";
			}

			// Scanner subcategory names within each category
			public class SUBCATEGORIES {
				public static LocString ALL = "all";

				// Solids
				public static LocString ORES = "Ores";
				public static LocString STONE = "Stone";
				public static LocString CONSUMABLES = "Consumables";
				public static LocString ORGANICS = "Organics";
				public static LocString ICE = "Ice";
				public static LocString REFINED = "Refined";
				public static LocString TILES = "Tiles";

				// Liquids
				public static LocString WATERS = "Waters";
				public static LocString FUELS = "Fuels";
				public static LocString MOLTEN = "Molten";
				public static LocString MISC = "Misc";

				// Gases
				public static LocString SAFE = "Safe";
				public static LocString UNSAFE = "Unsafe";

				// Buildings
				public static LocString OXYGEN = "Oxygen";
				public static LocString GENERATORS = "Generators";
				public static LocString FARMING = "Farming";
				public static LocString PRODUCTION = "Production";
				public static LocString STORAGE = "Storage";
				public static LocString REFINING = "Refining";
				public static LocString TEMPERATURE = "Temperature";
				public static LocString WELLNESS = "Wellness";
				public static LocString MORALE = "Morale";
				public static LocString INFRASTRUCTURE = "Infrastructure";
				public static LocString ROCKETRY = "Rocketry";
				public static LocString GEYSERS = "Geysers";

				// Networks
				public static LocString POWER = "Power";
				public static LocString LIQUID = "Liquid";
				public static LocString GAS = "Gas";
				public static LocString CONVEYOR = "Conveyor";
				public static LocString TRANSPORT = "Transport";

				// Automation
				public static LocString SENSORS = "Sensors";
				public static LocString GATES = "Gates";
				public static LocString CONTROLS = "Controls";
				public static LocString WIRES = "Wires";

				// Debris
				public static LocString MATERIALS = "Materials";
				public static LocString FOOD = "Food";
				public static LocString ITEMS = "Items";
				public static LocString BOTTLES = "Bottles";

				// Zones
				public static LocString ORDERS = "Orders";
				public static LocString ROOMS = "Rooms";
				public static LocString BIOMES = "Biomes";

				// Life
				public static LocString DUPLICANTS = "Duplicants";
				public static LocString ROBOTS = "Robots";
				public static LocString TAME_CRITTERS = "Tame Critters";
				public static LocString WILD_CRITTERS = "Wild Critters";
				public static LocString WILD_PLANTS = "Wild Plants";
				public static LocString FARM_PLANTS = "Farm Plants";
			}

			// Auto-move toggle
			public static LocString AUTO_MOVE_ON = "auto-move on";
			public static LocString AUTO_MOVE_OFF = "auto-move off";

			// Search
			public class SEARCH {
				public static LocString PROMPT = "search";
			}

			// Help entries
			public class HELP {
				public static LocString REFRESH = "Refresh scanner";
				public static LocString TELEPORT = "Jump to selected scanner entry";
				public static LocString TOGGLE_AUTO_MOVE = "Toggle auto-move cursor";
				public static LocString CYCLE_CATEGORY = "Scanner: cycle category";
				public static LocString CYCLE_SUBCATEGORY = "Scanner: cycle subcategory";
				public static LocString CYCLE_ITEM = "Scanner: cycle item";
				public static LocString CYCLE_INSTANCE = "Scanner: cycle instance";
				public static LocString SEARCH = "Search scanner";
			}
		}

		// Color names for the Pixel Pack building's decorative color picker.
		// These are approximate labels, not precise color specifications.
		// Translate to the closest natural color word in the target language.
		public class COLORS {
			public static LocString DARK_GRAY = "dark gray";
			public static LocString BLUE = "blue";
			public static LocString DARK_BLUE = "dark blue";
			public static LocString INDIGO = "indigo";
			public static LocString PURPLE = "purple";
			public static LocString MAROON = "maroon";
			public static LocString DARK_RED = "dark red";
			public static LocString BROWN = "brown";
			public static LocString DARK_OLIVE = "dark olive";
			public static LocString DARK_GREEN = "dark green";
			public static LocString FOREST_GREEN = "forest green";
			public static LocString DEEP_GREEN = "deep green";
			public static LocString DARK_TEAL = "dark teal";
			public static LocString BLACK = "black";
			public static LocString GRAY = "gray";
			public static LocString DODGER_BLUE = "dodger blue";
			public static LocString ROYAL_BLUE = "royal blue";
			public static LocString BLUE_VIOLET = "blue violet";
			public static LocString MAGENTA = "magenta";
			public static LocString CRIMSON = "crimson";
			public static LocString RED_ORANGE = "red orange";
			public static LocString ORANGE = "orange";
			public static LocString DARK_GOLD = "dark gold";
			public static LocString GREEN = "green";
			public static LocString MEDIUM_GREEN = "medium green";
			public static LocString SEA_GREEN = "sea green";
			public static LocString TEAL = "teal";
			public static LocString WHITE = "white";
			public static LocString SKY_BLUE = "sky blue";
			public static LocString CORNFLOWER = "cornflower";
			public static LocString VIOLET = "violet";
			public static LocString ORCHID = "orchid";
			public static LocString HOT_PINK = "hot pink";
			public static LocString SALMON = "salmon";
			public static LocString TANGERINE = "tangerine";
			public static LocString GOLD = "gold";
			public static LocString CHARTREUSE = "chartreuse";
			public static LocString BRIGHT_GREEN = "bright green";
			public static LocString SPRING_GREEN = "spring green";
			public static LocString CYAN = "cyan";
			public static LocString MEDIUM_GRAY = "medium gray";
			public static LocString OFF_WHITE = "off-white";
			public static LocString LIGHT_BLUE = "light blue";
			public static LocString LAVENDER = "lavender";
			public static LocString LIGHT_PURPLE = "light purple";
			public static LocString LIGHT_PINK = "light pink";
			public static LocString LIGHT_ROSE = "light rose";
			public static LocString BEIGE = "beige";
			public static LocString CREAM = "cream";
			public static LocString LIGHT_GOLD = "light gold";
			public static LocString LIGHT_LIME = "light lime";
			public static LocString PALE_GREEN = "pale green";
		}

		// Pixel Pack building side screen
		public class PIXEL_PACK {
			public static LocString PALETTE = "color palette";
			public static LocString ACTIVE_COLORS = "active colors";
			public static LocString STANDBY_COLORS = "standby colors";
			// {0} = pixel slot number (1-indexed int)
			public static LocString PIXEL_SLOT = "pixel {0}";
			public static LocString IN_USE = "in use";
			// {0} = number of colors in palette (int)
			public static LocString PALETTE_COUNT = "{0} colors";
		}

		// Door/checkpoint access control side screen
		// Telepad (Printing Pod) side screen badge notifications
		public class TELEPAD {
			public static LocString NEW_ACHIEVEMENTS = "new achievements";
			public static LocString SKILL_POINTS = "skill points available";
		}

		public class ACCESS_CONTROL {
			public static LocString ALLOWED = "allowed";
			public static LocString BLOCKED = "blocked";
			public static LocString LOCKED = "Door locked, access control disabled";
			// Traversal direction labels: named by travel direction
			public static LocString PASS_LEFT = "passing from right to left";
			public static LocString PASS_RIGHT = "passing from left to right";
			public static LocString PASS_UP = "passing from bottom to top";
			public static LocString PASS_DOWN = "passing from top to bottom";
			public static LocString DEFAULT_PASS_LEFT = "default, passing from right to left";
			public static LocString DEFAULT_PASS_RIGHT = "default, passing from left to right";
			public static LocString DEFAULT_PASS_UP = "default, passing from bottom to top";
			public static LocString DEFAULT_PASS_DOWN = "default, passing from top to bottom";
		}

		// Entity details screen (inspecting a selected building, duplicant, or item)
		public class DETAILS {
			public static LocString NO_ERRANDS = "No errands";
			public static LocString SCHEDULE = "Schedule";
			public static LocString CURRENT_TASK = "Current task";
			public static LocString ACTIONS_TAB = "Actions";
			public static LocString NO_ACTIONS = "No actions";
			public static LocString PRIORITY = "Priority";
			public static LocString PIN_RESOURCE = "Pin resource";
			// {0} = duplicant name, {1} = hat/role name, {2} = skills subtitle
			public static LocString DUPE_HAT_SUBTITLE = "{0}, {1}, {2}";
			// {0} = duplicant name, {1} = skills subtitle
			public static LocString DUPE_SUBTITLE = "{0}, {1}";
			// {0} = entity name, {1} = tab display name
			public static LocString ENTITY_TAB = "{0}, {1}";
			// {0} = parent container/context label, {1} = item speech text
			public static LocString PARENT_ITEM = "{0}, {1}";
			// {0} = section header label, {1} = first item speech text
			public static LocString HEADER_ITEM = "{0}, {1}";
		}

		// Table-based screens (priorities, vitals, consumables)
		public class TABLE {
			// {0} = column name
			public static LocString SORT_DESC_FMT = "{0}, sorted high to low";
			public static LocString SORT_ASC_FMT = "{0}, sorted low to high";
			public static LocString SORT_CLEARED_FMT = "{0}, sort cleared";
			// Help entries for table navigation
			public static LocString NAVIGATE_TABLE = "Navigate rows and columns";
			public static LocString JUMP_FIRST_LAST = "Jump to first or last row";
			public static LocString SORT_COLUMN = "Sort by column on header row";
			// Resource storage column header
			public static LocString STORED = "Stored";
		}

		// Priorities table screen
		public class PRIORITY_SCREEN {
			public static LocString HANDLER_NAME = "Priorities table";
			public static LocString TOOLBAR = "Toolbar";
			// {0} = skill level (int) for the chore group
			public static LocString SKILL = "skill {0}";
			// {0} = trait name that disables this chore group
			public static LocString DISABLED_TRAIT = "Disabled, {0}";
			// Announced after adjusting all priorities in a row/column
			public static LocString ROW_INCREASED = "row increased";
			public static LocString ROW_DECREASED = "row decreased";
			public static LocString COLUMN_INCREASED = "column increased";
			public static LocString COLUMN_DECREASED = "column decreased";
			// {0} = priority name (e.g. "Very High")
			public static LocString COLUMN_SET = "column set to {0}";
			public static LocString PROXIMITY_ON = "Proximity, on";
			public static LocString PROXIMITY_OFF = "Proximity, off";
			// {0} = comma-joined list of chore type names
			public static LocString AFFECTED_ERRANDS = "Affected errands: {0}";
			// Help entries
			public static LocString SET_PRIORITY = "Set priority of current cell";
			public static LocString ADJUST_ROW = "Adjust all priorities in row";
			public static LocString ADJUST_COLUMN = "Adjust all priorities in column";
			public static LocString SET_COLUMN = "Set all priorities in column";
		}

		// Vitals table screen (health, stress, calories)
		public class VITALS_SCREEN {
			public static LocString HANDLER_NAME = "Vitals table";
			// Announced when camera focuses on a duplicant
			public static LocString FOCUSED = "focused";
			public static LocString FOCUS_DUPLICANT = "Focus camera on duplicant";
			// {0} = formatted rate of change per time slice (e.g. "-500 kcal/cycle")
			public static LocString CHANGE = "change: {0}";
			// {0} = formatted calories eaten today (e.g. "1,200 kcal")
			public static LocString EATEN_TODAY = "eaten today: {0}";
			public static LocString FULLNESS_HEADER = "Calories remaining before starvation";
			public static LocString DISEASE_HEADER = "Current diseases and time remaining";
		}

		// Consumables permission table screen
		public class CONSUMABLES_SCREEN {
			public static LocString HANDLER_NAME = "Consumables table";
			// Per-duplicant permission states for a food/medicine item
			public static LocString PERMITTED = "permitted";
			public static LocString FORBIDDEN = "forbidden";
			public static LocString RESTRICTED = "restricted";
			// {0} = morale bonus value with sign (e.g. "+3", "-1")
			public static LocString MORALE = "morale {0}";
			// Column header state when some duplicants differ
			public static LocString MIXED = "mixed";
			public static LocString ALL_PERMITTED = "all permitted";
			public static LocString ALL_FORBIDDEN = "all forbidden";
			// Help entries
			public static LocString TOGGLE_ALL = "Toggle all duplicants for this consumable";
			public static LocString TOGGLE_PERMISSION = "Toggle permission";
		}

		// Research screen (browse, queue, and tree tabs)
		public class RESEARCH {
			public static LocString HANDLER_NAME = "Research";
			public static LocString BROWSE_TAB = "Browse";
			public static LocString QUEUE_TAB = "Queue";
			public static LocString TREE_TAB = "Tree";
			// Tech status labels
			public static LocString AVAILABLE = "available";
			public static LocString LOCKED = "locked";
			public static LocString COMPLETED = "completed";
			public static LocString ACTIVE = "active";
			// {0} = comma-joined prerequisite tech names
			public static LocString NEEDS_FMT = "needs {0}";
			// {0} = comma-joined names of buildings/items unlocked
			public static LocString UNLOCKS_FMT = "unlocks {0}";
			// {0} = prerequisite tech name (appended to mark it complete)
			public static LocString PREREQ_COMPLETED = "{0} completed";
			// {0} = comma-joined research point values with type (e.g. "100 Science, 50 Engineering")
			public static LocString BANKED_POINTS_FMT = "banked research points: {0}";
			// {0} = tech name
			public static LocString QUEUED = "{0} queued";
			// {0} = tech name
			public static LocString CANCELED = "{0} canceled";
			public static LocString QUEUE_CLEARED = "queue cleared";
			public static LocString QUEUE_EMPTY = "no research queued";
			public static LocString NO_BANKED_POINTS = "no banked research points";
			public static LocString DEAD_END = "no further techs";
			public static LocString ROOT_NODE = "no prerequisites";
			// Browse tab bucket/section headers
			public static LocString BUCKET_AVAILABLE = "Available";
			public static LocString BUCKET_LOCKED = "Locked";
			public static LocString BUCKET_COMPLETED = "Completed";
			// Help entries
			public static LocString JUMP_TO_TREE_HELP = "Jump to tech in tree view";
			public static LocString QUEUE_CANCEL_HELP = "Select or cancel research";
			public static LocString CANCEL_HELP = "Cancel selected research";
			// {0} = current points (float), {1} = required points (float), {2} = research type name
			public static LocString PROGRESS_ENTRY = "{0} of {1} {2}";
		}

		// Skills screen (duplicant skills, hats, boosters)
		public class SKILLS {
			public static LocString HANDLER_NAME = "Skills";
			public static LocString DUPES_TAB = "Duplicants";
			public static LocString SKILLS_TAB = "Skills";
			public static LocString TREE_TAB = "Tree";

			// {0} = number of available skill points (int)
			public static LocString POINTS = "{0} points";
			// {0} = current morale (float), {1} = morale expectation (float)
			public static LocString MORALE_OF = "morale {0} of {1}";
			// {0} = hat/role name or NO_HAT
			public static LocString HAT_LABEL = "hat: {0}";
			public static LocString NO_HAT = "no hat";
			// {0} = comma-joined skill group interest names
			public static LocString INTERESTS = "interests: {0}";
			public static LocString NO_INTERESTS = "no interests";
			// {0} = current XP toward next point (float), {1} = XP needed for next point (float)
			public static LocString XP_PROGRESS = "{0} of {1} experience to next point";
			// {0} = hat/role name
			public static LocString HAT_QUEUED = "{0} queued";

			// Browse tab section headers
			public static LocString BUCKET_DUPE_INFO = "Dupe Info";
			public static LocString BUCKET_AVAILABLE = "Available";
			public static LocString BUCKET_LOCKED = "Locked";
			public static LocString BUCKET_MASTERED = "Mastered";
			public static LocString BUCKET_BOOSTERS = "Boosters";

			// Skill status labels
			public static LocString AVAILABLE = "available";
			public static LocString MASTERED = "mastered";
			public static LocString LOCKED = "locked";
			// Skill was granted automatically (not learned)
			public static LocString GRANTED = "granted";
			public static LocString MORALE_DEFICIT = "morale deficit";
			// Duplicant has an interest in this skill's group
			public static LocString INTERESTED = "interested";
			public static LocString NO_SKILL_POINTS = "no skill points";
			// {0} = comma-joined prerequisite skill names
			public static LocString NEEDS_FMT = "needs {0}";
			// {0} = skill name, {1} = status text (mastered/available/locked reason)
			public static LocString NAME_STATUS = "{0}, {1}";
			// Attribute modifier line. {0} = modifier description, {1} = sign ("+" or ""), {2} = value (float)
			public static LocString MODIFIER_LINE = "{0} {1}{2}";
			// Attribute total. {0} = attribute name, {1} = total value (float)
			public static LocString HEADER_TOTAL = "{0} {1}";
			// {0} = duplicant name (in Duplicants tab, marked as stored/available)
			public static LocString NAME_STORED = "{0}, stored";
			// {0} = duplicant name, {1} = formatted points string (from POINTS)
			public static LocString NAME_POINTS = "{0}, {1}";
			// {0} = trait name that blocks learning this skill
			public static LocString BLOCKED_BY = "blocked by {0}";
			// {0} = morale cost (int)
			public static LocString MORALE_NEED = "{0} morale need";
			// {0} = number of duplicants who mastered this skill (int)
			public static LocString MASTERED_BY = "mastered by {0}";
			// {0} = skill name
			public static LocString LEARNED = "{0} learned";
			public static LocString CANNOT_LEARN = "cannot learn";
			public static LocString DEAD_END = "no further skills";
			public static LocString ROOT_NODE = "no prerequisites";

			// Booster slot management
			// {0} = assigned slot count (int), {1} = unlocked slot count (int)
			public static LocString BOOSTER_SLOTS = "{0} of {1} booster slots used";
			// {0} = number assigned of this booster type (int)
			public static LocString ASSIGNED = "{0} assigned";
			// {0} = number of unassigned boosters available (int)
			public static LocString BOOSTER_AVAILABLE = "{0} available";
			public static LocString BOOSTER_HINT = "use plus to assign, minus to unassign";
			public static LocString BOOSTER_ASSIGNED = "booster assigned";
			public static LocString BOOSTER_UNASSIGNED = "booster unassigned";
			public static LocString NO_BOOSTERS_AVAILABLE = "no boosters available";
			public static LocString NONE_ASSIGNED = "none assigned";
			public static LocString NO_EMPTY_SLOTS = "no empty slots";

			// {0} = hat/role name
			public static LocString HAT_SELECTED = "{0} selected";
			public static LocString SELECT_HAT = "select hat";

			// Help entries
			public static LocString JUMP_TO_TREE_HELP = "Jump to skill in tree view";
			public static LocString LEARN_HELP = "Learn skill or select hat";
			public static LocString BOOSTER_HELP = "Assign or unassign boosters with +/-";
		}

		// Schedule screen (timetable management)
		public class SCHEDULE {
			public static LocString HANDLER_NAME = "Schedule";
			public static LocString SCHEDULES_TAB = "Schedules";
			public static LocString DUPES_TAB = "Duplicants";
			public static LocString ADD_SCHEDULE = "Add new schedule";
			public static LocString CANNOT_DELETE_LAST = "Cannot delete last schedule";
			public static LocString CANNOT_DELETE_LAST_ROW = "Cannot delete last row";
			public static LocString SCHEDULE_DELETED = "Schedule deleted";
			public static LocString TIMETABLE_ROW_ADDED = "Row added";
			public static LocString TIMETABLE_ROW_DELETED = "Row deleted";
			public static LocString OPTIONS_RENAME = "Rename";
			public static LocString OPTIONS_DUPLICATE = "Duplicate schedule";
			public static LocString OPTIONS_DELETE_SCHEDULE = "Delete schedule";
			public static LocString OPTIONS_ADD_ROW = "Add timetable row";
			public static LocString OPTIONS_DELETE_ROW = "Delete timetable row";
			// {0} = schedule group name (e.g. "Sleep", "Work"), {1} = block/hour number (int, 0-23)
			public static LocString BLOCK_LABEL = "{0}, block {1}";
			// Announced when painting a block that already has the selected type.
			// {0} = block number (int), {1} = schedule group name
			public static LocString BLOCK_ALREADY = "block {0}, already {1}";
			// {0} = brush/schedule group name (e.g. "Bathtime", "Downtime")
			public static LocString BRUSH_ACTIVE = "Brush: {0}";
			// {0} = schedule group name, {1} = start block number (int), {2} = end block number (int)
			public static LocString PAINTED_RANGE = "Painted {0}, blocks {1} through {2}";
			// {0} = row number (1-indexed int)
			public static LocString ROW_LABEL = "row {0}";
			// {0} = schedule name, {1} = row number (1-indexed int)
			public static LocString SCHEDULE_ROW = "{0}, row {1}";
			public static LocString MOVED_UP = "moved up";
			public static LocString MOVED_DOWN = "moved down";
			// Help entries
			public static LocString HELP_NAVIGATE_BLOCKS = "Navigate blocks";
			public static LocString HELP_JUMP_BLOCK = "Jump to first or last block";
			public static LocString HELP_SELECT_BRUSH = "Select brush";
			public static LocString HELP_PAINT = "Paint current block";
			public static LocString HELP_PAINT_MOVE = "Paint and move";
			public static LocString HELP_PAINT_RANGE = "Paint range to start or end";
			public static LocString HELP_REORDER_SCHEDULE = "Move schedule up or down";
			public static LocString HELP_ROTATE = "Rotate blocks left or right";
			public static LocString HELP_OPTIONS = "Open schedule options";
			public static LocString HELP_CHANGE_SCHEDULE = "Change schedule assignment";
		}

		// Daily report screen
		public class REPORT {
			public static LocString HANDLER_NAME = "Daily report";
			public static LocString COLONY_SUMMARY = "Colony summary";
			// {0} = formatted positive value for the report row
			public static LocString ADDED = "added {0}";
			// {0} = formatted negative value (shown as positive) for the report row
			public static LocString REMOVED = "removed {0}";
			// {0} = formatted net value for the report row
			public static LocString NET = "net {0}";
			// {0} = note description text, {1} = formatted note value
			public static LocString NOTE = "{0} {1}";
			public static LocString HELP_CYCLE = "Previous or next cycle";
			public static LocString NO_LATER_CYCLE = "Latest cycle";
			public static LocString NO_EARLIER_CYCLE = "Earliest cycle";
		}

		// In-game notification menu
		public class NOTIFICATIONS {
			// {0} = notification title text, {1} = count of grouped notifications (int)
			public static LocString GROUP_COUNT = "{0} x{1}";
			// Fallback when a notification member has no name. {0} = group title, {1} = member number (1-indexed int)
			public static LocString NUMBERED_ENTRY = "{0} {1}";
			public static LocString MENU_TITLE = "Notifications";
			public static LocString EMPTY = "no notifications";
			public static LocString DISMISSED = "dismissed";
			public static LocString CANNOT_DISMISS = "cannot dismiss";
			public static LocString OPEN_MENU_HELP = "Open notifications menu";
			public static LocString DISMISS_HELP = "Dismiss notification";
			public static LocString MESSAGE_DIALOG = "Message";
			public static LocString NEXT_MESSAGE = "next message";
			public static LocString DONT_SHOW_AGAIN = "don't show again";
			public static LocString PLAY_VIDEO = "play video";
		}

		// In-game encyclopedia (Codex) browser
		public class CODEX {
			public static LocString CATEGORIES_TAB = "Categories";
			public static LocString CONTENT_TAB = "Content";
			public static LocString NO_ARTICLE = "no article selected";
			// Announced for codex entries that haven't been unlocked yet
			public static LocString LOCKED_CONTENT = "locked content";
			// Recipe info labels
			public static LocString REQUIRES = "requires";
			public static LocString PRODUCES = "produces";
			public static LocString TIME = "time:";
			public static LocString MADE_IN = "made in";
			// {0} = number of hyperlinks in the article (int)
			public static LocString LINK_MENU = "{0} links";
			// Help entries
			public static LocString FOLLOW_LINK_HELP = "Follow link";
			public static LocString NO_BACK = "nothing to go back to";
			public static LocString NO_FORWARD = "nothing to go forward to";
			public static LocString HISTORY_BACK_HELP = "Go back";
			public static LocString HISTORY_FORWARD_HELP = "Go forward";
		}

		// Measurement ruler tool on tile cursor
		public class RULER {
			public static LocString PLACED = "ruler set";
			public static LocString CLEARED = "ruler cleared";
			public static LocString HELP_PLACE = "Place ruler at cursor";
			public static LocString HELP_CLEAR = "Clear ruler";
		}

		// Tile cursor bookmarks for quick navigation
		public class BOOKMARKS {
			// {0} = bookmark slot number (1-indexed int)
			public static LocString BOOKMARK_SET = "bookmark {0} set";
			public static LocString NO_BOOKMARK = "no bookmark";
			// Announced when the Printing Pod can't be found
			public static LocString NO_HOME = "no printing pod";
			// Cursor is at the bookmark location. {0} = grid coordinates
			public static LocString ORIENT_HERE = "here. {0}";
			// Cursor is away from the bookmark. {0} = distance/direction text, {1} = grid coordinates
			public static LocString ORIENT_DISTANCE = "{0}. {1}";
			// Help entries
			public static LocString HELP_HOME = "Jump to Printing Pod";
			public static LocString HELP_SET_BOOKMARK = "Set bookmark";
			public static LocString HELP_GOTO_BOOKMARK = "Jump to bookmark";
			public static LocString HELP_ORIENT_BOOKMARK = "Distance to bookmark";
		}

		// Building placement and build menu
		public class BUILD_MENU {
			public static LocString ACTION_MENU = "action menu";
			public static LocString TOOLS_CATEGORY = "Tools";
			public static LocString PLACED = "placed";
			public static LocString PLACED_NO_MATERIAL = "placed, no material available";
			public static LocString NOT_ROTATABLE = "not rotatable";
			public static LocString NOT_BUILDABLE = "not buildable";
			public static LocString CANCELED = "canceled";
			public static LocString CANCEL_CONSTRUCTION = "canceled";
			public static LocString NO_CONSTRUCTION = "nothing to cancel";
			public static LocString MUST_BE_STRAIGHT = "must be a straight line";
			public static LocString INVALID_LINE = "invalid";
			// {0} = number of cells in the line segment (int)
			public static LocString LINE_CELLS = "{0} cells";
			// Wire/pipe start point
			public static LocString START_SET = "start set";
			public static LocString START_CLEARED = "start cleared";
			public static LocString INFO_PANEL = "info";
			public static LocString OBSTRUCTED = "obstructed";
			// Material picker entries. {0} = material name, {1} = available quantity
			public static LocString MATERIAL_ENTRY = "{0}, {1}";
			public static LocString MATERIAL_INSUFFICIENT = "{0}, {1}, insufficient";
			// Building footprint extent. {0} = tile count from center (int)
			public static LocString EXTENT_LEFT = "{0} left";
			public static LocString EXTENT_RIGHT = "{0} right";
			public static LocString EXTENT_UP = "{0} up";
			public static LocString EXTENT_DOWN = "{0} down";
			// {0} = direction name (up/down/left/right)
			public static LocString FACING = "facing {0}";
			// Building orientation direction names
			public static LocString ORIENT_UP = "up";
			public static LocString ORIENT_RIGHT = "right";
			public static LocString ORIENT_DOWN = "down";
			public static LocString ORIENT_LEFT = "left";
			public static LocString ORIENT_VERTICAL = "vertical";
			public static LocString ORIENT_HORIZONTAL = "horizontal";
			// Info panel section headers
			public static LocString EFFECTS = "effects";
			public static LocString REQUIREMENTS = "requirements";
			// Material slot summary. {0} = material category, {1} = selected material name, {2} = available quantity
			public static LocString MATERIAL_SLOT = "{0}: {1}, {2}";
			public static LocString MATERIAL_SLOT_INSUFFICIENT = "{0}: {1}, {2}, insufficient";
			// {0} = comma-joined extent directions (e.g. "2 left, 3 up")
			public static LocString EXTENT_FORMAT = "extends {0}";
			// Help entries
			public static LocString HELP_PLACE = "Place building or set start";
			public static LocString HELP_PLACE_AND_EXIT = "Place building and exit";
			public static LocString HELP_ROTATE = "Rotate building";
			public static LocString HELP_BUILDING_LIST = "Return to building list";
			public static LocString HELP_INFO = "Open info panel";
			public static LocString HELP_CANCEL_CONSTRUCTION = "Cancel construction at cursor";
			public static LocString HELP_OPEN_ACTION_MENU = "Open action menu";
			// Separator between alternative materials (e.g. "Copper or Gold")
			public static LocString MATERIAL_OR = " or ";
			// Info panel detail lines. {0} = comma-joined attribute name-value pairs
			public static LocString ATTRIBUTES_FMT = "attributes: {0}";
			// {0} = comma-joined attribute modifiers from chosen material
			public static LocString MATERIAL_EFFECTS_FMT = "material effects: {0}";
			// {0} = facade/skin name
			public static LocString FACADE_FMT = "facade: {0}";
			// {0} = comma-joined room category labels this building contributes to
			public static LocString CATEGORY_FMT = "category: {0}";
			// {0} = building description text
			public static LocString DESCRIPTION_FMT = "description: {0}";
			// {0} = descriptor type label, {1} = comma-joined descriptor values
			public static LocString DESCRIPTOR_FMT = "{0}: {1}";
			// {0} = building announcement, {1} = pre-build error description
			public static LocString PREBUILD_ERROR = "{0}, {1}";
			// Attribute display. {0} = attribute name, {1} = value
			public static LocString ATTR_VALUE = "{0} {1}";
			// Material attribute modifier. {0} = attribute name, {1} = sign ("+" or ""), {2} = modifier value
			public static LocString ATTR_MODIFIER = "{0} {1}{2}";
			public static LocString FACADE_DEFAULT = "default";
		}

		// Cursor skip (jump to next tile change)
		public class SKIP {
			// {0} = number of tiles skipped (int), {1} = "tile" or "tiles"
			public static LocString COUNT_FORMAT = "{0} {1}";
			public static LocString TILE_SINGULAR = "tile";
			public static LocString TILE_PLURAL = "tiles";
			public static LocString NO_CHANGE_BOUNDARY = "no change till map boundary";
			public static LocString HELP_SKIP = "Skip cursor to next change";
		}

		// Resource browser and pinned resource readout
		public class RESOURCES {
			public static LocString BROWSER_TITLE = "Resources";
			public static LocString NO_PINNED = "no pinned resources";
			public static LocString PINNED = "pinned";
			public static LocString UNPINNED = "unpinned";
			public static LocString ALL_UNPINNED = "all unpinned";
			public static LocString NO_INSTANCES = "none available";
			// Resource amount qualifiers
			public static LocString RESERVED = "reserved";
			public static LocString AVAILABLE = "available";
			// Announced when reserved amount exceeds total
			public static LocString OVERDRAWN = "overdrawn";
			// Resource trend direction
			public static LocString RISING = "rising";
			public static LocString FALLING = "falling";
			// {0} = resource name
			public static LocString DISCOVERED = "Discovered: {0}";
			// {0} = amount, {1} = building name, {2} = coordinates
			public static LocString INSTANCE_IN_BUILDING = "{0} in {1} at {2}";
			// {0} = amount, {1} = coordinates
			public static LocString INSTANCE_LOOSE = "{0} at {1}";
			// Help entries
			public static LocString HELP_PIN = "Pin or unpin resource";
			public static LocString HELP_CLEAR_PINS = "Unpin all resources";
			public static LocString HELP_JUMP = "Jump to instance location";
			public static LocString HELP_OPEN = "Open resource browser";
			public static LocString HELP_READ_PINNED = "Read pinned resources";
		}
	}
}
