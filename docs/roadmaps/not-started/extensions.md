# Later extension plugins

Custom scripting stays a later plugin. Encounter Creator and Custom
Adventures are later **project types in the LokrLab suite**, not new
BepInEx plugins — see [lab-suite-merge.md](../completed/lab-suite-merge.md).
Each still layers on something that already stands on its own:

| Extension | Builds on | What it adds |
|---|---|---|
| **Custom scripting for abilities** | Ability Lab overhaul Lua card ([ability-lab-overhaul.md](../completed/ability-lab-overhaul.md) Phase 7, shipped) | A later plugin for fully custom ability logic beyond the in-lab Lua card and `AbilityLabAPI.RegisterActionCard`. In-engine `Lua` actions and `CallFunction` already exist; Ability Lab now authors Lua as a real Advanced card. This plugin is extra surface, not a second VM. `LokrAbilityLab`'s own data shape stays the "80% case, no code" path. |
| **Encounter Creator** | Sandbox Encounter v1 (as a **loader**, not the editor) | **Own roadmap:** [encounter-creator.md](../started/encounter-creator.md). Separate project type. Phase 13 terrain catalog in 0.12.64. |
| **Custom Adventures** | Encounter Creator | Chains multiple authored encounters (plus presumably map/progression structure) into a full custom adventure — the largest and last extension, depending on the Encounter Creator existing and being solid first. |

The dependency order matters: Custom Adventures depends on Encounter
Creator (v1 done) **and** on vanilla encounter import being solid
enough to remix a room. **One-room campaign override** (guarded load
hook when a Lab project claims `combat_banditambush`) lives on
[vanilla-encounter-edit.md](../started/vanilla-encounter-edit.md), not here.
Adventures is quest chains / maps. Custom Scripting can start now that
the [Ability Lab overhaul](../completed/ability-lab-overhaul.md) is
complete (Lua card + `RegisterActionCard`). It is a later plugin on
top of that card, not a rewrite of Ability Lab.

Before Adventures, research vanilla edit for all three labs:
[vanilla-character-edit.md](../completed/vanilla-character-edit.md),
[vanilla-ability-edit.md](../completed/vanilla-ability-edit.md),
[vanilla-encounter-edit.md](../started/vanilla-encounter-edit.md).

