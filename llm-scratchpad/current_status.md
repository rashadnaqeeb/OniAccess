# LLM Scratchpad - Current Status

## Working Branch
`claude-mod-cleanup`

## Prompts Completed
1. `prompts/sanity-checks-setup.md` - done
2. `prompts/information-gathering-and-checking.md` - done
3. `prompts/code-directory-construction.md` - done

## Next Prompt
`prompts/input-handling.md` (no files over 2000 lines, so large-file-handling was skipped)

## What Was Done
- Validated CLAUDE.md: all facts verified, fixed KModalScreen lifecycle gotcha (was oversimplified), replaced hardcoded user path with %USERPROFILE%
- Surveyed existing docs: project already has strong documentation (accessibility audit, codebase index, factorio-access design reference, hotkey reference, inspection panels, details screen handler)
- Gathered ONI game reference: controls, mechanics, screens, modding ecosystem
- Created `llm-docs/` with synthesized game reference and CLAUDE.md overview
- Built complete code index: 177 .md files under `llm-scratchpad/code-index/` covering all 177 source files

## Key Findings
- The project has 177 source files (~30k lines total), ~25 screen handlers implemented
- Largest file: ColonySetupHandler.cs at 1325 lines (well under 2000)
- Main documentation gap: no architectural overview of the mod's own systems (HandlerStack, ContextDetector, SpeechPipeline, TileCursor, GridScanner)
- ONI-Decompiled/ serves as a usable API reference (~6,638 decompiled .cs files)
- docs/factorio-access concepts.md is the highest-quality design document - read it for architecture intent

## Scratchpad Files
- `current_status.md` - this file
- `claude_md_validation.md` - CLAUDE.md validation report (kept for reference)
- `existing_docs_survey.md` - survey of all existing documentation (kept for reference)
- `oni_game_info.md` - raw game info (promoted to llm-docs/oni-game-reference.md)
- `code-index/` - 177 .md files mirroring OniAccess/ source tree with class/method declarations and line numbers
