# Full port plan


*What it actually takes to bring one of the old, pre-BepInEx mod system's
characters (see `../../../lokr-modding`) into this tool with nothing left as
a hand-edited text file or an asset manually dropped into the right
folder — the "solve the reskin problem, make an interface instead of raw
txt files" goal this whole tool exists for. Grounded in two real data
points: Onagro (a from-scratch custom-rig character, hand-converted
2026-08-11) and a full survey of the "Official Pack" (a `Mods/` tree of
16 real shipped old-system characters — Arcane Archer, Archmage,
Assassin, Blasteon, Cleric, Demon Lord, Enchantress, General, Gnome
Saleman, Goblinbomber, Grawl, Musketeer, Necromancer, Paladin, Shadow
Archer, Trollzerker — plus 3 shared, non-character utility folders
(`Resources`, `Empty Units`, `new_heroes_lib`)), which between them cover
nearly every content shape the old system produced. This section is
additive to §4–§8, not a replacement — it's what those workstations still
need in order to close the gap this survey found, organized by which of
them owns each gap.*


## In this folder

- [Content surface](content-surface.md) — old vs new system status table
- [Port mechanics](port-mechanics.md) — what a port gets for free + Onagro gotchas
- [Gaps](gaps.md) — what's still missing
- [Port checklist](port-checklist.md) — step-by-step per character
- [Priorities](priorities.md) — suggested order for closing gaps
- [Port open questions](open-questions-port.md) — leftover questions (`Background`; `Model` resolved 2026-08-14)

Importer work that this survey did not build (picker, exo+reskin, pack
root) lives in
[legacy-pack-port.md](../legacy-pack-port.md).
