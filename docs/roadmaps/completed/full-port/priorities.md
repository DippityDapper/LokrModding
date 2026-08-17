# 9.5 Suggested priority


Extends §10's own phasing rather than replacing it —
this is where a "full port" effort should slot in:

1. ~~**Cheap, mechanical General-workstation gaps first**: achievement-gated
   unlock (small, self-contained) and multi-locale localization (bigger,
   but still additive to existing fields, no new workstation concepts).~~
   Both implemented and verified in-game 2026-08-12.
2. **Roster icon/background resolver next** — highest-frequency gap found
   (16/16 in the survey), and already tracked as a known General-workstation
   TODO; this just raises its priority with real evidence.
3. ~~**Fold §9.3.A's stats/level-chain/states/soundConfig extensions into
   whatever General's Stats sub-feature ends up being**, not after — the
   same "expensive to retrofit once a data model exists and characters
   depend on it" lesson §10 already states for `CharacterAPI`'s own
   history applies here too, just to General instead of `LokrAbilityLab`.
   This also covers entity-type support (Hero vs. Enemy/Summon), since
   that's General's own scope now (§9.3.A).~~ Entity-type support
   implemented and verified in-game 2026-08-12 — the rest of this
   item was already resolved 2026-08-11.
4. **Stand up `LokrAbilityLab` as a real plugin** — even a minimal v1
   (the form-based editor over the existing `AbilitiesDefinitions`
   backend, per §6's own scope) immediately closes the shared-library gap
   by construction, since that's just what the plugin's storage model is,
   not a feature to build separately.
5. **Shared mod-wide resources** — a design decision more than an urgent
   build; low risk to defer, but worth deciding deliberately rather than
   accreting convention by accident.
6. ~~**`Model` field investigation**~~ **Resolved 2026-08-14** — spawn
   prefab + per-prefab combat clip names. Do not delete. See
   [legacy-pack-port.md](../legacy-pack-port.md).
7. **Importer** — selection sheet, vanilla exo + pack reskin, split
   multi-entity files (OnagroMine / SulfurBomb). Same doc as item 6.

