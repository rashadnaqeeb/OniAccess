# Missing Screen Handlers

Screens a blind player can encounter that have no handler registered.

## Critical (blocks save flow)

- [x] **SaveScreen** -- Save game dialog (KModalScreen). Pause > Save. Every session.
- [x] **FileNameDialog** -- Name a new save (KModalScreen). First save of a colony.

## Important (blocks gameplay)

- [x] **CustomizableDialogScreen** -- Multi-button dialog variant (KModalScreen). DLC toggles, mod warnings.
- [x] **GameOverScreen** -- Colony death screen (KModalScreen). All dupes die.
- [x] **VictoryScreen** -- Colony win screen (KModalScreen). Achievement completion.

## Blocking walls (invisible to blind player, blocks input until dismissed)

- [ ] **WattsonMessage** -- Tutorial messages (KScreen). Early game popups.
- [ ] **ComicViewer** -- Story comics (KScreen). New game start, story events.
- [ ] **StoryMessageScreen** -- Story event popups (KScreen). Story trait events.
- [x] **PatchNotesScreen** -- Post-update notes (KModalScreen). After game updates.

## Deferred (later phases)

- [ ] **ImmigrantScreen** -- Printing Pod selection (CharacterSelectionController). Every 3 cycles. Phase 8 scope, but MinionSelectHandler may already work if registered.
