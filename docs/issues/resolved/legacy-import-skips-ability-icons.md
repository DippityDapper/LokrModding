# LegacyModImporter: ability icons are not copied

Area: LokrCharacterLab (`Editor/General/LegacyModImporter.cs`)
Status: resolved

As of 2026-08-14: import now copies `AbilityIcons/<id>.png`,
`AbilityIcons/<Icon>.png`, and nested `AbilityIcons/<id>/*.png` into
`<abilityId>/icons/` when those files exist. Do not invent names.

Resolved: 2026-08-15

Resolution: Official Pack import confirmed in-game. Icons land in the
Ability Lab folder; the result modal does not tell the user to place
them by hand. See
[`../../roadmaps/completed/legacy-pack-port.md`](../../roadmaps/completed/legacy-pack-port.md).
