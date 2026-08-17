# 9.4 A concrete per-character port checklist


What actually taking one old character from zip to "as complete as it was
in the old system" requires, split by what's possible today vs. blocked
on §9.3's gaps:

**Possible today (mechanical, per §9.2):**
1. Build the rig for real in the Animator (the only step that was always
   going to take real authoring time, port or not). Pack reskins should
   become a reconstruct+atlas step on
   [legacy-pack-port.md](../legacy-pack-port.md) Phase 3.
2. Hand-merge identity/stats/skills/skillProgression/states/soundConfig
   into `definition/rlheroes.txt`, applying the three gotchas in §9.2.
3. Copy abilities/ability-icons/enemy-definitions into the flat
   mod-wide conventions, with per-character filename prefixes.
4. Copy sounds/portraits into their per-character subfolders.
5. Merge English localization strings (name/lore + every `SKILL_*`/
   `COMBAT_MODIFIER_*` line the abilities need).

**Blocked on §9.3 today, currently either impossible or a manual
workaround:**
6. ~~Achievement-gated unlock~~ — **resolved 2026-08-12**, see §9.3.A.
7. ~~Non-English localization — currently dropped entirely.~~
   **Implemented and verified in-game 2026-08-12**, see §9.3.A.
8. Custom roster icon/background/map-token — currently must reuse an
   existing base-game hero's assets, same workaround the old system
   itself used.
9. ~~`Model` field~~ — **resolved 2026-08-14**: combat spawn prefab +
   clip-name set. Copy it on import; do not force `HumanArcher`. See
   [legacy-pack-port.md](../legacy-pack-port.md).

