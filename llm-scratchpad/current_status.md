# LLM Scratchpad - Current Status

## Working Branch
`claude-mod-cleanup`

## Prompts Completed
1. `prompts/sanity-checks-setup.md` - done
2. `prompts/information-gathering-and-checking.md` - done
3. `prompts/code-directory-construction.md` - done
4. `prompts/input-handling.md` - done (no refactoring needed, architecture is mature)
5. `prompts/string-builder.md` - done (not a string builder mod, ~8% string building, well-organized)

## Next Prompt
`prompts/high-level-cleanup.md`

## What Was Done
- Validated CLAUDE.md: all facts verified, fixed KModalScreen lifecycle gotcha (was oversimplified), replaced hardcoded user path with %USERPROFILE%
- Surveyed existing docs: project already has strong documentation
- Gathered ONI game reference: controls, mechanics, screens, modding ecosystem
- Created `llm-docs/` with synthesized game reference and CLAUDE.md overview
- Built complete code index: 177 .md files under `llm-scratchpad/code-index/`
- Assessed input/UI system: mature handler stack with clean hierarchy, no refactoring needed
- Assessed string building: ~8% of codebase, concentrated in purpose-built helpers and section pipeline

### Low-Level Cleanup (15 items, all completed)
1. **ConduitSection base class** — replaced 4 identical conduit section files with parameterized `ConduitSection`
2. **GridUtil.ValidateCluster** — extracted shared helper for scanner backend ValidateEntry methods
3. **SideScreenWalker TryAddCategoryCore** — deduped TryAddCategoryContainer and TryAddSelectionCategoryContainer
4. **Sound helper consolidation** — moved PlayHoverSound/PlayWrapSound to BaseScreenHandler, removed 7 duplicates
5. **ShowDispatch helper** — extracted shared Show/OnShow postfix body for 9 lifecycle patches
6. **Dead GetVisibleBuildings** — removed unused method from BuildMenuData
7. **BuildToolHandler method merges** — merged duplicate placement and exit method pairs
8. **PlayOpenSound/PlayCloseSound/PlayNegativeSound** — centralized in BaseScreenHandler, removed 10 duplicates
9. **Pixel-pack color loops** — extracted AddColorSlotGroup for active/standby color slots
10. **BuildWidgetText helper** — extracted speech-text+tooltip pattern repeated 4 times
11. **Dead DebrisRouter material loop** — removed unreachable _materialTags code
12. **ContextDetector registration loop** — collapsed 6 identical OptionsMenuHandler registrations
13. **Dead TypeAheadSearch Escape case** — removed unreachable switch case
14. **Dead NestedMenuHandler HandleKeyDown** — removed pure passthrough override
15. **ScreenLifecyclePatches comment trimming** — removed redundant per-class XML docs

## Key Findings
- The project has 177 source files (~30k lines total), ~25 screen handlers implemented
- Largest file: ColonySetupHandler.cs at 1325 lines (well under 2000)
- Input system: HandlerStack + KeyPoller + ModInputRouter + ContextDetector — clean and complete
- String building: helper classes (ResearchHelper, SkillsHelper, ScheduleHelper) + ICellSection pipeline + parts-list joins — well organized
- Main documentation gap: no architectural overview of the mod's own systems

## Scratchpad Files
- `current_status.md` - this file
- `claude_md_validation.md` - CLAUDE.md validation report
- `existing_docs_survey.md` - survey of all existing documentation
- `oni_game_info.md` - raw game info (promoted to llm-docs/oni-game-reference.md)
- `code-index/` - 177 .md files mirroring OniAccess/ source tree
