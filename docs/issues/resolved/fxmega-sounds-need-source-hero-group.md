# FXMega sounds need the source hero's DynamicSoundGroup

Area: LokrCharacterLoader
Status: resolved

Vanilla combat SFX live in per-hero MasterAudio groups
(`DynamicSoundGroupAsraSounds`, `DynamicSoundGroupOrcCleaverSounds`,
…). `Stage.AddUnit` / `LevelManager` only load the spawned unit's own
`soundConfig.assetId`. An imported Assassin that reuses
`ShadowStrikeCastFXMega` / `OrcCleaverParryStanceCastFXMega` therefore
logs `MasterAudio could not find sound: krl_sfx_combatAsra_shadowStrikeCharge`
(and the Cleaver parry clip) in sandbox — those groups were never
instantiated.

This is the same in a campaign fight if Asra / Orc Cleaver are not in
the party and have not been loaded earlier in the session
(`AudioController.loadedSoundGroups` is a process-lifetime cache).

Intended fix: before `MasterAudio.PlaySound`, if the group is missing,
load the matching `DynamicSoundGroup*` asset from the `sounds` bundle
(token from `krl_sfx_combatAsra_*` → `Asra`, plus
`DynamicSoundGroupGenericSkillSounds` for `combatGeneric`). Confirm in
sandbox that counterattack / backstab / stealth play their vanilla
clips without the "could not find sound" warning.

1.1.10 added that prefix but HarmonyX never applied it: the baked
`Type[]` used `string` as the sixth argument, and the real method is
`PlaySound(string, float, float?, float, string, double?, bool, bool)`.
`PatchAll` threw `Undefined target method` in `Awake`, so Character
Loader logged no "loaded — N method(s) patched" line. 1.1.11 retargets
by method name and applies each patch class on its own so a future miss
cannot abort the rest of the plugin. Still unresolved until the clips
are heard in-game.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** verify-only
**Approach:** No further C# unless the in-game session still misses clips. 1.1.11 already prefixes `MasterAudio.PlaySound` via `AccessTools.DeclaredMethod(typeof(MasterAudio), "PlaySound")` (first arg `sType`, matching DarkTonic) and calls `VanillaSoundGroups.EnsureLoaded`. `OwnerToken` maps `krl_sfx_combatAsra_*` / `krl_va_combatAsra_*` to `Asra` and `krl_sfx_combatOrcCleaver_*` to `OrcCleaver`; `combatGeneric` loads `DynamicSoundGroupGenericSkillSounds`. `TryLoad` uses vanilla `AudioController.LoadDynamicGroupAsset` (`AssetBundleIds.SOUNDS` = `"sounds"`). `FXMegaComponentAction.PlaySound` is that same `PlaySound` call. Isolated `PatchAll` so a future miss cannot abort the plugin.
**Exact change:** None if sandbox hears the clips. Confirm 1.1.11 applied (`LogOutput.log` has `LoKR Character Loader v1.1.11 loaded — N method(s) patched` and no `PlaySound was not found`). If the prefix still never binds, pin `TargetMethod` to the 8-arg signature with sixth type `typeof(double?)`, not `typeof(string)`. If it binds but `SoundGroupExists` stays false after `TryLoad`, the remaining change is to load the group earlier (same `LoadDynamicGroupAsset` from `FXManager.LoadFXMega` / `Stage.AddUnit` when `CastFXId` is a foreign hero FXMega) so Instantiation is not racing the same `PlaySound` call.
**Do not:** Re-bake a `Type[]` with `string` as argument six (1.1.10). Do not reimplement MasterAudio or preload every `DynamicSoundGroup*` at boot. Do not treat "patch applied" as resolved without hearing the clips.
**In-game verify:** 1. Cold-start the game (new process) so `AudioController.loadedSoundGroups` has not already cached Asra or Orc Cleaver. 2. Confirm Character Loader 1.1.11 patched in `$(GameDir)/BepInEx/LogOutput.log`. 3. Open Character Lab on Assassin (File → Import Legacy Pack from `Mods/Assassin` if it is not already a Lab project). Do not put Asra or Orc Cleaver in the fight — Sandbox spawns only the current hero plus `BanditRaider`. 4. Start sandbox at a level that has Counterattack, Backstab, and Stealth. 5. Cast Counterattack (`OrcCleaverParryStanceCastFXMega`), Backstab and Stealth (`ShadowStrikeCastFXMega`). Hear the vanilla clips. Log must not contain `MasterAudio could not find sound: krl_sfx_combatAsra_shadowStrikeCharge` or the Cleaver parry equivalent; a `VanillaSoundGroups: loaded 'DynamicSoundGroupAsraSounds'` (and OrcCleaver) line is success.
**Risk:** On-demand group load can instantiate extra MasterAudio prefabs in a session that never spawned that hero; combat balance is unchanged (same vanilla clips). Failed-clip cache is per process, so a first-frame miss would stay silent until restart — that is why the sandbox must be a cold start.

Resolved: 2026-08-15

Resolution: Confirmed in-game on Lab Assassin sandbox: vanilla clips
play for Assassin abilities that reuse Asra / Orc Cleaver FXMega.
LokrCharacterLoader 1.1.11 `VanillaSoundGroups.EnsureLoaded` on
`MasterAudio.PlaySound` is enough; no further C#.
