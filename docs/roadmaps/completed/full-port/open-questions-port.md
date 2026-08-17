# 9.6 Open questions specific to a full port


- ~~**What does `Model` (`unitDefinition.kind`) actually do?**~~
  **Resolved 2026-08-14.** `UnitViewManager.InstantiateUnitView` does
  `FindPrefab(unit.kind)` against the `units` bundle — Model is the
  combat spawn prefab, not a cosmetic label. The Official Pack's
  distinct values are that prefab's animation controllers (and often a
  matching `Exoskeletons/<Model>.png` sheet). `MetaExo` / the Lab rig is
  the mesh, swapped onto the prefab; Onagro's `ObeliskLvl4` is expected.
  Do not remove the field. New characters default `HumanArcher`. See
  [legacy-pack-port.md](../legacy-pack-port.md)#model-combat-prefab-not-the-mesh.
- ~~**Does achievement-gated unlock need new base-game hook work, or just a
  new field?**~~ **Resolved 2026-08-12**: just a field.
  `HeroRosterManager.cs` (old system) confirms `unlockAchievement` is read
  generically via `achievementManager.IsCompleted(id)` — any achievement
  id, not a hardcoded set. See §9.3.A.
- ~~**Does "shared ability" ownership need to be a first-class concept, or
  stay convention-only?**~~ **Resolved 2026-08-11**: abilities get their
  own plugin (`LokrAbilityLab`, §6), not a `CharacterProfile`-adjacent
  convention — a shared, mod-wide library is that plugin's native storage
  model, not a special case bolted onto per-character folders. See §6's
  "Why a separate plugin, not a workstation" for the reasoning.
- ~~**Do `EnemiesDefinitions`/summons get their own authoring surface, or
  ride inside Ability Creator as a spawn-effect type?**~~ **Resolved
  2026-08-11**: neither, exactly — an enemy/summon is an *entity*, authored
  with General/the Animator the same way a hero is (§9.3.A); an ability
  effect that spawns one just references it by id, the same way it'd
  reference anything else `LokrAbilityLab` doesn't itself own.

