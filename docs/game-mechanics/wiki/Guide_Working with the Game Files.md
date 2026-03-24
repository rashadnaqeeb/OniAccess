# Guide/Working with the Game Files

Looking at the game files can help provide valuable insight into game mechanics, as well as providing higher-quality art for use on
wiki
pages. The game files can be found through Steam, by right-clicking on the game in your Library and following Properties, Local Files tab, Browse Local Files...
You can also easily access the folder containing your save files from the main menu by going into Options -> Feedback -> Browse Save Files.
Code
To look at the code, you need a C# decompiler. Some options are
dotPeek
,
ILSpy
, and
dnSpy
(each have their strengths and weaknesses, you may want to use multiple). Once you have a decompiler, you can decompile the
OxygenNotIncluded_Data/Managed/Assembly-CSharp.dll
file, which contains most of the game code. Some more engine-related code is also in the adjacent
Assembly-CSharp-firstpass.dll
file.
Note that decompiled code is not as easy to read as normal source code, so it may be difficult to follow some parts (for example, local variable names are inferred by the decompiler and can be pretty unhelpful, and all comments are lost). Here are some tips about the organization of the code:
STRINGS
is often a good starting point for looking at the code, because you can first search for the in-game name of an entity, and then the variable name will tell you what the code calls it. For example, "Wheezewort" leads to "Coldbreather".
TUNING contains a lot of balance information, but can't always be linked to the code that uses it due to compiler optimizations.
Most game code is in the root namespace.
Most entities have two objects for their code, a main object, which typically handles their state machine information (how they control their time-based behavior), and a Config object, which sets up most of their components and assigns values; the main object is useful for understanding behavior, while the Config is useful for understanding balance.
Sometimes you will encounter a message being sent as a long number. These are hashes; you can look for the more informative name in GameHashes, SimHashes, SimMessageHashes, etc.
Sometimes dotPeek will have trouble decompiling some code and show it as a lot of compiler-generated fields. dnSpy often handles these sections better.
Config
Most of Config files (like that for world-gen) and some simple balance can be found in subfolders of
OxygenNotIncluded_Data\StreamingAssets
. The data format of most files is
YAML
.
codex
: The in-game codex.
dlc
: Templates and world-gen dedicated for the DLC
elements
: Elements properties, including their
localizationID
(defined in
strings
), specific heat capacity, light absorption factor and more.
strings
: Translation of entities' name. They are also helpful for English uses as they matches context (used in code, like
STRINGS.BUILDINGS.PREFABS.LADDER.DESC
) with id (English name, like
"(That means they climb it.)"
).
templates
: In-game structure,
worldgen
.
worldgen
: World generation.
Assets
Some tuning and all of the art is in the asset bundles, which are the
.assets
files directly inside of
OxygenNotIncluded_Data
. These can be opened with
Unity Asset Bundle Extractor
or
AssetStudio
. One tip for the Asset Bundle Extractor: you can box-select files to extract in bulk by dragging from the right of the listing area, or using
Ctrl+Shift
combined with the left mouse button or arrows for multiselection.
The bundle extractor can extract textures as
.png
images, which is useful for getting material for the Wiki. However, you may find that most images consist of components of the corresponding objects. These components can be used to assemble more complicated textures and replay the animation with the help of the TextAsset files,
*_anim
and
*_build
.
An animation tool
generates
.scml
files which can be opened with Spriter (essential edition is available for free in Don't Stave Mod Tools) and exported as
.png
files of rendered images. It is said that
.gif
animations can be exported if a pro version of Spriter is available
[
1
]
.
*_anim
and
*_build
is likely to be a Klei format, and on inspection appears to be similar to the one they used in
Don't Starve
(
Together
). There may be more information in the
KAnim
files in
Assembly-CSharp-firstpass.dll
, and
the tools built for Don't Starve
may be a good starting point for our own tool for working with these anims.
Note that some textures have non-obvious names, like
cuprite
for copper ore and
CookedEgg
for omelette. You can discover the internal name by creating the desired item or substance in game, saving it as a template, and then looking up the "id" field in the YAML file. Also, note that capital letters sort before lowercase letters
and
that capitalization of the id field might be different than the asset name.  For instance, the id for an omelette is
CookedEgg
but the texture name starts with "cookedegg".
Many properties of the elements, including their material properties and freezing/melting points, can be found in the
sharedassets2.assets
file, as text assets named "Solid", "Liquid" and "Gas", with the solid, liquid, and gaseous forms of each element defined. Other advanced/optional properties, like the element's decor, live in the
LegacyModMain
class.
For DLC, the assets file is
StreamingAssets/expansion1_bundle
(without
.assets
extension) inside game folder.
Save Files
Save files can be found in multiple locations
PC - C:\Users\%USERNAME%\Documents\Klei\OxygenNotIncluded\save_files
Mac - ~/Library/Application Support/unity.Klei.Oxygen Not Included/save_files
Linux - ~/.config/unity3d/Klei/Oxygen Not Included/save_files
Save files can be edited with save file editors like Duplicity (outdated). Editing the save file with unsuitable methods can corrupt it, therefore it is recomended to not replace your save file with the edited one. Save files contain all the data inside your colonies (cycle, Duplicant data, world configs, etc).
Reference
↑
As shown in a post from a Chinese community
https://tieba.baidu.com/p/6414881287?pid=129133347554#129133347554
