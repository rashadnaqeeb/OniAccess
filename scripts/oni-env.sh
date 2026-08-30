# oni-env.sh - Locate the Mac game's Managed directory and export ONI_MANAGED.
# Sourced by build.sh and test.sh. An ONI_MANAGED already in the environment
# wins; otherwise every Steam library folder is checked.
if [ -z "${ONI_MANAGED:-}" ]; then
	STEAM="$HOME/Library/Application Support/Steam"
	LIBRARIES=("$STEAM")
	VDF="$STEAM/steamapps/libraryfolders.vdf"
	if [ -f "$VDF" ]; then
		while IFS= read -r path; do
			LIBRARIES+=("$path")
		done < <(sed -n 's/^[[:space:]]*"path"[[:space:]]*"\(.*\)".*/\1/p' "$VDF")
	fi
	for lib in "${LIBRARIES[@]}"; do
		candidate="$lib/steamapps/common/OxygenNotIncluded/OxygenNotIncluded.app/Contents/Resources/Data/Managed"
		if [ -d "$candidate" ]; then
			export ONI_MANAGED="$candidate"
			break
		fi
	done
	if [ -z "${ONI_MANAGED:-}" ]; then
		echo "ERROR: Could not find ONI. Set ONI_MANAGED to" >&2
		echo "  <SteamLibrary>/steamapps/common/OxygenNotIncluded/OxygenNotIncluded.app/Contents/Resources/Data/Managed" >&2
		exit 1
	fi
fi
