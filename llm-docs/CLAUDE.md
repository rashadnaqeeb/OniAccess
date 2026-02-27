# llm-docs — Synthesized Reference Documentation

This directory contains reference documentation gathered and synthesized for LLM consumption. These documents complement the design docs in `docs/` by providing game-level context that helps an LLM reason about what the mod needs to do.

## Files

- `oni-game-reference.md` — Comprehensive ONI game reference: control scheme, tile grid/space model, core mechanics (duplicants, resources, oxygen, temperature, power, plumbing, research, skills, food, disease, schedules, priorities, rooms, automation, rocketry), enumeration of all game screens with class-to-handler mappings, modding ecosystem details, key data sources for speech output, and common OniAccess interaction patterns.

## Relationship to Other Docs

- `docs/oni_accessibility_audit.md` catalogs every UI element that needs accessibility coverage. The game reference here explains the mechanics behind those UI elements.
- `docs/CODEBASE_INDEX.md` helps find specific classes. The game reference here explains what those classes represent in gameplay terms.
- `docs/factorio-access concepts.md` describes the mod's design philosophy. The game reference here provides the ONI-specific context that philosophy operates in.
