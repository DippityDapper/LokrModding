# Issue resolution — in-game test checklist

Actionable tests for the 2026-08-15 two-pass campaign. Playbook:
[`docs/issues/two-pass-resolution.md`](../../issues/two-pass-resolution.md).
Backup before this work:
`/home/dippity/dev/lokr-modding/backups/bepinex-20260815-172132.tar.gz`.

Do not move an issue to `docs/issues/resolved/` until that issue's steps
are confirmed in the running game (Steam / Proton). Shipping a build is
not enough. Checkboxes stay empty until you run the steps.

Log file: `$(GameDir)/BepInEx/LogOutput.log`.

Pass 2 agents append sections as they finish. Verify-only issues (no new
code in Pass 2) are listed first.

**Pass 2 implementation is complete (2026-08-15).** Confirm each section
in the running game before moving the matching file out of
`docs/issues/unresolved/` or `docs/issues/unresolved-tested/`. Versions
at Pass 2: LokrPatch 1.0.10, LokrCharacterLoader 1.1.15, LokrLab 0.12.31.
Later confirms (party stow, inventory GUID) used LokrLab 0.12.32+.
Current LokrLab is 0.12.34.

**Unit tests (2026-08-15):** Layer 1 xUnit suite is in
[`completed/test-suite.md`](../completed/test-suite.md). Issues whose
rule has a passing test live in `docs/issues/unresolved-tested/` with a
**Unit tests** block. That is not an in-game confirm — checkboxes below
stay empty until Steam/Proton.

---

## Verify-only (no Pass 2 code)

### encyclopedia-button-unverified-click

**Plugin:** LokrEncyclopedia (unchanged)
**What changed:** None. Click the unlocked button and record what happens.
**How to test:**
1. Launch through Steam.
2. From the title screen, Continue (or New Game) into the main-menu hub (`UIMainMenu` — Party / Achievements / Arena).
3. Click Encyclopedia.
4. Note whether anything opens, the game no-ops, or it errors.
**Expected:** A real window, a silent no-op, or an error. If no-op: disable the plugin by default or document that the button does nothing. Do not add an Encyclopedia UI.

**Result (2026-08-15): pass.** Button clickable; vanilla **Coming Soon!** popup. Moved to `docs/issues/resolved/encyclopedia-button-unverified-click.md`.

in game: The button appears and is clickable. A 'Coming Soon!' popup appears by the button.

### fxmega-sounds-need-source-hero-group

**Plugin:** LokrCharacterLoader 1.1.11 (already shipped; confirm)
**What changed:** None in Pass 2. Confirm the PlaySound prefix loads missing hero sound groups.
**How to test:**
1. Cold-start the game (new process) so `AudioController.loadedSoundGroups` has not already cached Asra or Orc Cleaver.
2. Confirm Character Loader 1.1.11 patched in `LogOutput.log`.
3. Open Character Lab on Assassin. Do not put Asra or Orc Cleaver in the fight — Sandbox spawns only the current hero plus `BanditRaider`.
4. Start sandbox at a level that has Counterattack, Backstab, and Stealth.
5. Cast Counterattack (`OrcCleaverParryStanceCastFXMega`), Backstab and Stealth (`ShadowStrikeCastFXMega`).
**Expected:** Hear the vanilla clips. Log must not contain `MasterAudio could not find sound: krl_sfx_combatAsra_shadowStrikeCharge` (or the Cleaver parry equivalent). A `VanillaSoundGroups: loaded 'DynamicSoundGroupAsraSounds'` (and OrcCleaver) line is success.

**Result (2026-08-15): pass.** Heard vanilla Assassin clips in sandbox. Moved to `docs/issues/resolved/fxmega-sounds-need-source-hero-group.md`.

in game: I can hear the vanilla clips for the assassins abilities

### ability-kv-parse-empty-filename

**Plugin:** LokrCharacterLoader 1.1.11 (already shipped; confirm)
**What changed:** None in Pass 2. 1.1.11 names contributed TextAssets and logs `ex.ToString()`.
**How to test:**
1. Launch through Steam. Open Lab, then start a sandbox reload (`ReloadScope.All`).
2. Open `LogOutput.log` and find `ERROR PARSING`.
**Expected:** The two failing fragments have real filenames (not `file  `). Use those names to fix or skip the fragments. Assassin abilities still register.

**Result (2026-08-15): pass.** Logging named `assassin_quickstep_qy4z6j/ability.txt`; that Lab file was rewritten. Confirmed: Assassin `ERROR PARSING` is gone. Moved to `docs/issues/resolved/ability-kv-parse-empty-filename.md`. Official Pack source is still malformed; re-import would restore it.

in game: I am not exactly sure what this test is asking for, so i have started the game, went to the lab, loaded assassin, started a sandbox fight, then clicked the start sandbox fight again. The following are some of the logs from the console. All of the assassins abilites work in the sandbox.

[Info   : Unity Log] Start
[Info   : Unity Log] COMBAT: Loading quest: quest_generic_encounter with encount
ers: fighttesterempty
[Info   : Unity Log] TerrainTile: Autoconfiguring
[Info   : Unity Log] HexSize: {0.55, -0.33275}
[Info   : Unity Log] Size: 24 - 24
[Info   : Unity Log] offset: {-3.85, 0.6655}
[Info   :   Console] (start (expression (expression (expression (atom ( (express
ion (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (
word b a s e D a m a g e)))) )))) ))) * (expression (atom (func (funcname (word
s t a t)) ( (expression (atom (stringLiteral # (word b o n u s D a m a g e M u l
 t i p l i e r)))) ))))) + (expression (atom (func (funcname (word s t a t)) ( (
expression (atom (stringLiteral # (word e x t r a D a m a g e)))) ))))))
[Info   :   Console] (start (expression (expression (atom (func (funcname (word
s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m a x)))) )))
) * (expression (atom (func (funcname (word s t a t)) ( (expression (atom (strin
gLiteral # (word h e a l t h _ m a x _ a v a i l a b l e _ p e r c e n t)))) )))
)))
[Info   :   Console] (start (expression (expression (atom (func (funcname (word
s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m a x)))) )))
) - (expression (atom (func (funcname (word s t a t)) ( (expression (atom (strin
gLiteral # (word h e a l t h)))) ))))))
[Info   :   Console] (start (expression (expression (atom (number 1))) - (expres
sion (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stri
ngLiteral # (word h e a l t h)))) )))) / (expression (atom (func (funcname (word
 s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m a x)))) ))
)))))
[Info   :   Console] (start (expression (expression (atom (func (funcname (word
s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m i s sin g))
)) )))) - (expression (atom (func (funcname (word s t a t)) ( (expression (atom
(stringLiteral # (word h e a l t h)))) ))))))
[Info   :   Console] (start (expression (expression (atom (number 1))) - (expres
sion (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stri
ngLiteral # (word h e a l t h)))) )))) / (expression (atom (func (funcname (word
 s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m i s sin g)
))) )))))))
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Ma
p.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font a
sset [NotoSansArabic-Bold SDF].
[Info   : Unity Log] UIFightNavController: changing state from NONE to DEFAULT
[Info   : Unity Log] UIFightNavController: selecting object FightNavProxy (Unity
Engine.GameObject)
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSyste
ms.BaseEventData sel:FightNavProxy
[Info   : Unity Log] Finished loading asset
[Info   :LoKR Character Loader] CustomRigLoader: rig 'assassin_z7v9v1' had no Sp
eak — aliased to Stand so cinematics can play.
[Info   :LoKR Character Loader] CustomRigLoader: built 'assassin_z7v9v1' from 'S
:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrCharact
erLab\assassin_z7v9v1' (19 parts, 21 animations).
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] adding: Toothassassin_lethal_strike_3xvqvf
[Info   : Unity Log] adding: Toothassassin_stealth_2yc92g
[Info   : Unity Log] adding: Toothassassin_counterattack_p00481
[Info   : Unity Log] adding: Toothassassin_backstab_9z9zvn
[Info   : Unity Log] adding: Vaincommon_melee_attack
[Info   : Unity Log] adding: Vainstunning_blow
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 c
lip names.
[Info   : Unity Log] ProjectileViewManager disabled
[Info   : Unity Log] Finished loading localizable strings - Loaded 5261 entries
in 0.1516405 seconds
[Error  : Unity Log] ERROR PARSING: Could not parse kv in file S:/steamapps/comm
on/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrAbilityLab\assassin_abi
lities_c6x8qe\assassin_quickstep_qy4z6j\ability.txt - EXCEPTION
KVLib.KeyValueParsingException: Hit unnamed key while parsing without unnamed ke
ys enabled.
  at KVLib.KeyValues.PenguinParser.ParseAllKeyValues (System.String contents, Sy
stem.Boolean allowunnamedkeys) [0x004ad] in <11e9574c8b034c63879fed334c1c2f7d>:0

  at KVLib.KeyValues.PenguinParser.ParseAll (System.String contents) [0x00000] i
n <11e9574c8b034c63879fed334c1c2f7d>:0
  at Ironhide.Legends.Model.Game.Units.Abilities.AbilitiesDefinitionsPatches.Exe
cuteLoad (Ironhide.Legends.Model.Game.Units.Abilities.AbilitiesDefinitions __ins
tance) [0x000b8] in /home/dippity/dev/lokr-modding/bepinex/LokrCharacterLoader/P
atches/AbilitiesDefinitionsPatches.cs:73
Preview: "assassin_quickstep_qy4z6j"  {         "AbilityBehavior"       "UNIT_TA
RGET | POINT_TARGET |  NEEDS_CLEAR_TERRAIN | FAKE_ACTION | DOESNT_CONSUME_MOVE"
        "AbilityAOETeamFilt
[Info   : Unity Log] AbilitiesDefinition: Loaded in 0.318 with 0.287 of parsing
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 c
lip names.
[Info   :LoKR Character Loader] ContentReloader: reloaded All in 519 ms (0 heroe
s refreshed).
[Info   :  LoKR Lab] Sandbox: starting embedded fight with 'assassin_z7v9v1' at
level 3.
[Info   :  LoKR Lab] EmbeddedFightHost: additive fight with caster 'assassin_z7v
9v1'.
[Info   :  LoKR Lab] EmbeddedSceneHost: additive load of 'KRLegendsFightGameplay
02' from bundle 'scenes'.
[Info   : Unity Log] SpritesheetLoaderAwake - 0
[Info   : Unity Log] SpriteAnchorDB: Loading anchors from anchorDB
[Info   : Unity Log] Best configuration for device: {"dpi":240.0,"rootFolder":"S
pritesheets/iPad/","scalingVariantSuffix":"-hd","pixelsPerMeter":200.0,"minZoomF
actor":0.699999988079071,"maxZoomFactor":1.2999999523162842,"assetsBundleVariant
s":["hd"]}
[Info   : Unity Log] Spritesheet: Loading spritesheet Forest-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Mountain-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Ruins-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Dungeon-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Wasteland-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Tutorial-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Silveroak-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Arena-hd
[Info   : Unity Log] SpriteResourceManager AWAKE
[Info   : Unity Log] ProjectileViewManager enabled
[Info   : Unity Log] FXManager: Preload-Pre time: 0.002563477
[Info   : Unity Log] FXManager: Preload time: 0.08712006
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 c
lip names.
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Ma
p.Screens.UIManager!
[Info   :LoKR Mod API] Scene loaded: krlegendsfightgameplay02 (mode=Additive)
[Info   :  LoKR Lab] EmbeddedSceneHost: scene loaded (krlegendsfightgameplay02).
[Info   : Unity Log] Start
[Info   : Unity Log] COMBAT: Loading quest: quest_generic_encounter with encount
ers: fighttesterempty
[Info   : Unity Log] HexSize: {0.55, -0.33275}
[Info   : Unity Log] Size: 24 - 24
[Info   : Unity Log] offset: {-3.85, 0.6655}
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Ma
p.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font a
sset [NotoSansArabic-Bold SDF].
[Info   : Unity Log] UIFightNavController: changing state from NONE to DEFAULT
[Info   : Unity Log] UIFightNavController: selecting object FightNavProxy (Unity
Engine.GameObject)
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSyste
ms.BaseEventData sel:FightNavProxy
[Info   : Unity Log] Finished loading asset
[Info   :LoKR Character Loader] CustomRigLoader: rig 'assassin_z7v9v1' had no Sp
eak — aliased to Stand so cinematics can play.
[Info   :LoKR Character Loader] CustomRigLoader: built 'assassin_z7v9v1' from 'S
:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrCharact
erLab\assassin_z7v9v1' (19 parts, 21 animations).
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] adding: Toothassassin_lethal_strike_3xvqvf
[Info   : Unity Log] adding: Toothassassin_stealth_2yc92g
[Info   : Unity Log] adding: Toothassassin_counterattack_p00481
[Info   : Unity Log] adding: Toothassassin_backstab_9z9zvn
[Info   : Unity Log] adding: Vaincommon_melee_attack
[Info   : Unity Log] adding: Vainstunning_blow
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 c
lip names.


### alias-unitname-parsed-as-function

**Plugin:** LokrCharacterLoader (existing `$alias` → `#word` rewrite; confirm)
**What changed:** None in Pass 2 unless the Onagro fight still fails.
**How to test:**
1. Launch through Steam. Open Lab. Start an Onagro Stage fight that uses `onagro_mine_games`.
2. Check `LogOutput.log` for `Function onagro_mine_… is not defined` / `Could not load ability onagro_mine_games`.
**Expected:** `onagro_mine_games` loads and the mine spawns. No bare-id function parse error.

**Result (2026-08-15): pass.** Onagro mine works in sandbox (enemy walking on mine). Moved to `docs/issues/resolved/alias-unitname-parsed-as-function.md`. Remaining `assassin_quickstep` parse is a corrupt KV file, not this rewrite.

in game: I loaded onagro in the lab and entered a sandbox fight. I used the mine ability and confirmed it worked by moving the enemy over it. The mine works fine. Here are some of the logs.

tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Enco
unterDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Enco
unterDefinitions
[Info   : Unity Log] SriptFile: Config
[Info   : Unity Log] SriptFile-QuestCustomFunctions: eventPriorities
[Info   : Unity Log] SriptFile-QuestCustomFunctions: obelisks
[Info   : Unity Log] SriptFile-LootTables: AdventureMetagameLootTables
[Info   : Unity Log] Loading LootTable: adventure-store-metagame-2
[Info   : Unity Log] Loading LootTable: adventure-recruit-metagame-2
[Info   : Unity Log] Loading LootTable: adventure-store-metagame-3
[Info   : Unity Log] Loading LootTable: adventure-recruit-metagame-3
[Info   : Unity Log] SriptFile-LootTables: LootTableExample
[Info   : Unity Log] Loading LootTable: pipote
[Info   : Unity Log] Loading LootTable: any-armor
[Info   : Unity Log] SriptFile-LootTables: LootTables
[Info   : Unity Log] Loading LootTable: lowHealing
[Info   : Unity Log] Loading LootTable: midHealing
[Info   : Unity Log] Loading LootTable: highHealing
[Info   : Unity Log] Loading LootTable: lowLoot
[Info   : Unity Log] Loading LootTable: midLoot
[Info   : Unity Log] Loading LootTable: highLoot
[Info   : Unity Log] Loading LootTable: lowPotion
[Info   : Unity Log] Loading LootTable: midPotion
[Info   : Unity Log] Loading LootTable: highPotion
[Info   : Unity Log] Loading LootTable: lowItems
[Info   : Unity Log] Loading LootTable: midItems
[Info   : Unity Log] Loading LootTable: highItems
[Info   : Unity Log] Loading LootTable: buffItem
[Info   : Unity Log] Loading LootTable: arenaHealing
[Info   : Unity Log] Loading LootTable: arenaUpgrade
[Info   : Unity Log] Loading LootTable: maskTable
[Info   : Unity Log] SriptFile-MapModifiers: basicMapModifiers
[Info   : Unity Log] SriptFile-Items: basicItems
[Info   : Unity Log] Initializing savegame manager: disable save:False disable h
istory:True
[Info   :  LoKR Lab] LokrLab Open from 'krlegendsmainmenu' (wasTransitioning=False).
[Info   :  LoKR Lab] HomeWorkstationScene: Loaded character 'onagro_0nzj37'.
[Info   : Unity Log] Finished loading localizable strings - Loaded 5261 entries in 0.1416962 seconds
[Error  : Unity Log] ERROR PARSING: Could not parse kv in file S:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrAbilityLab\assassin_abilities_c6x8qe\assassin_quickstep_qy4z6j\ability.txt - EXCEPTION
KVLib.KeyValueParsingException: Hit unnamed key while parsing without unnamed keys enabled.
  at KVLib.KeyValues.PenguinParser.ParseAllKeyValues (System.String contents, System.Boolean allowunnamedkeys) [0x004ad] in <11e9574c8b034c63879fed334c1c2f7d>:0
  at KVLib.KeyValues.PenguinParser.ParseAll (System.String contents) [0x00000] in <11e9574c8b034c63879fed334c1c2f7d>:0
  at Ironhide.Legends.Model.Game.Units.Abilities.AbilitiesDefinitionsPatches.ExecuteLoad (Ironhide.Legends.Model.Game.Units.Abilities.AbilitiesDefinitions __instance) [0x000b8] in /home/dippity/dev/lokr-modding/bepinex/LokrCharacterLoade
r/Patches/AbilitiesDefinitionsPatches.cs:73
Preview: "assassin_quickstep_qy4z6j"  {         "AbilityBehavior"       "UNIT_TARGET | POINT_TARGET |  NEEDS_CLEAR_TERRAIN | FAKE_ACTION | DOESNT_CONSUME_MOVE"         "AbilityAOETeamFilt
[Info   : Unity Log] AbilitiesDefinition: Loaded in 0.403 with 0.362 of parsing
[Error  : Unity Log] ERROR PARSING: Could not parse kv in file S:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrAbilityLab\assassin_abilities_c6x8qe\assassin_quickstep_qy4z6j\ability.txt - EXCEPTION
KVLib.KeyValueParsingException: Hit unnamed key while parsing without unnamed keys enabled.
  at KVLib.KeyValues.PenguinParser.ParseAllKeyValues (System.String contents, System.Boolean allowunnamedkeys) [0x004ad] in <11e9574c8b034c63879fed334c1c2f7d>:0
  at KVLib.KeyValues.PenguinParser.ParseAll (System.String contents) [0x00000] in <11e9574c8b034c63879fed334c1c2f7d>:0
  at Ironhide.Legends.Model.Game.Units.Abilities.AbilitiesDefinitionsPatches.ExecuteLoad (Ironhide.Legends.Model.Game.Units.Abilities.AbilitiesDefinitions __instance) [0x000b8] in /home/dippity/dev/lokr-modding/bepinex/LokrCharacterLoade
r/Patches/AbilitiesDefinitionsPatches.cs:73
Preview: "assassin_quickstep_qy4z6j"  {         "AbilityBehavior"       "UNIT_TARGET | POINT_TARGET |  NEEDS_CLEAR_TERRAIN | FAKE_ACTION | DOESNT_CONSUME_MOVE"         "AbilityAOETeamFilt
[Info   : Unity Log] AbilitiesDefinition: Loaded in 0.28 with 0.248 of parsing
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 clip names.
[Info   :LoKR Character Loader] ContentReloader: reloaded All in 891 ms (0 heroes refreshed).
[Info   :  LoKR Lab] Sandbox: starting embedded fight with 'onagro_0nzj37' at level 3.
[Info   :  LoKR Lab] EmbeddedFightHost: additive fight with caster 'onagro_0nzj37'.
[Info   : Unity Log] Loading slot 1 status:EMPTY
[Info   :  LoKR Lab] EmbeddedSceneHost: additive load of 'KRLegendsFightGameplay02' from bundle 'scenes'.
[Info   : Unity Log] SpritesheetLoaderAwake - 0
[Info   : Unity Log] SpriteAnchorDB: Loading anchors from anchorDB
[Info   : Unity Log] Best configuration for device: {"dpi":240.0,"rootFolder":"Spritesheets/iPad/","scalingVariantSuffix":"-hd","pixelsPerMeter":200.0,"minZoomFactor":0.699999988079071,"maxZoomFactor":1.2999999523162842,"assetsBundleVari
ants":["hd"]}
[Info   : Unity Log] Spritesheet: Loading spritesheet Forest-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Mountain-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Ruins-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Dungeon-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Wasteland-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Tutorial-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Silveroak-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Arena-hd
[Info   : Unity Log] SpriteResourceManager AWAKE
[Info   : Unity Log] ProjectileViewManager enabled
[Info   : Unity Log] FXManager: Preload-Pre time: 0.3546219
[Info   : Unity Log] FXManager: Preload time: 0.4395981
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 clip names.
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Map.Screens.UIManager!
[Info   :LoKR Mod API] Scene loaded: krlegendsfightgameplay02 (mode=Additive)
[Info   :  LoKR Lab] EmbeddedSceneHost: scene loaded (krlegendsfightgameplay02).
[Info   : Unity Log] Start
[Info   : Unity Log] COMBAT: Loading quest: quest_generic_encounter with encounters: fighttesterempty
[Info   : Unity Log] TerrainTile: Autoconfiguring
[Info   : Unity Log] HexSize: {0.55, -0.33275}
[Info   : Unity Log] Size: 24 - 24
[Info   : Unity Log] offset: {-3.85, 0.6655}
[Info   :   Console] (start (expression (expression (expression (atom ( (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word b a s e D a m a g e)))) )))) ))) * (expression (atom (func (funcname (wo
rd s t a t)) ( (expression (atom (stringLiteral # (word b o n u s D a m a g e M u l t i p l i e r)))) ))))) + (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word e x t r a D a m a g e)))) ))))))
[Info   :   Console] (start (expression (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m a x)))) )))) * (expression (atom (func (funcname (word s t a t)) ( (expression (atom (st
ringLiteral # (word h e a l t h _ m a x _ a v a i l a b l e _ p e r c e n t)))) ))))))
[Info   :   Console] (start (expression (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m a x)))) )))) - (expression (atom (func (funcname (word s t a t)) ( (expression (atom (st
ringLiteral # (word h e a l t h)))) ))))))
[Info   :   Console] (start (expression (expression (atom (number 1))) - (expression (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h)))) )))) / (expression (atom (func (funcname (w
ord s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m a x)))) )))))))
[Info   :   Console] (start (expression (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m i s sin g)))) )))) - (expression (atom (func (funcname (word s t a t)) ( (expression (at
om (stringLiteral # (word h e a l t h)))) ))))))
[Info   :   Console] (start (expression (expression (atom (number 1))) - (expression (expression (atom (func (funcname (word s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h)))) )))) / (expression (atom (func (funcname (w
ord s t a t)) ( (expression (atom (stringLiteral # (word h e a l t h _ m i s sin g)))) )))))))
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Map.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font asset [NotoSansArabic-Bold SDF].
[Info   : Unity Log] UIFightNavController: changing state from NONE to DEFAULT
[Info   : Unity Log] UIFightNavController: selecting object FightNavProxy (UnityEngine.GameObject)
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSystems.BaseEventData sel:FightNavProxy
[Info   : Unity Log] Finished loading asset
[Info   :LoKR Character Loader] CustomRigLoader: built 'onagro_0nzj37' from 'S:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrCharacterLab\onagro_0nzj37' (14 parts, 18 animations).
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] Unit whitout portrait found.
[Info   : Unity Log] adding: Toothonagro_missile_j4kzzk
[Info   : Unity Log] adding: Toothonagro_mine_games_tcwx9f
[Info   : Unity Log] adding: Toothonagro_power_slam_r4j96f
[Info   : Unity Log] adding: Toothonagro_tar_bomb_jvc945
[Info   : Unity Log] adding: Vaincommon_melee_attack
[Info   : Unity Log] adding: Vainstunning_blow
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 clip names.
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSystems.BaseEventData sel:FightNavProxy
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSystems.BaseEventData sel:FightNavProxy
[Info   :LoKR Character Loader] VanillaSoundGroups: loaded 'DynamicSoundGroupBombardierSounds' for 'krl_sfx_combatBombardier_badaboomToss'.
[Warning:LoKR Patch] ApplyModifier: skipped missing modifier 'modifier_onagro_mine_tracker'.
[Info   : Unity Log] UIFightNavController: changing state from DEFAULT to HUD
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSystems.BaseEventData sel:FightNavProxy
[Info   : Unity Log] UIFightNavController: changing state from HUD to DEFAULT
[Info   : Unity Log] UIFightNavController: selecting object FightNavProxy (UnityEngine.GameObject)
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSystems.BaseEventData sel:FightNavProxy
[Info   :LoKR Character Loader] VanillaSoundGroups: loaded 'DynamicSoundGroupArcaneMageSounds' for 'krl_sfx_combatArcaneMage_fireballExplosion'.
[Info   : Unity Log] ACHIEVEMENT-INCREMENT: damage_dealt_count - 0 - 0/1000 - False
[Info   : Unity Log] ACHIEVEMENT-INCREMENT: round_big_damage - 0 - 0/50 - False


### lab-static-panels-not-reset-on-close

**Plugin:** LokrLab (ResetSession already wired from LabClosing; confirm)
**What changed:** None in Pass 2.
**How to test:**
1. Open Character Lab. Open File menu and Island Atlas picker at least once.
2. Close Lab. Re-open Lab.
3. Open File menu and Island Atlas again.
**Expected:** No missing/destroyed widgets, no NRE in `LogOutput.log` from `IslandAtlasPickerPanel` / `MenuBarPanel`.

**Result (2026-08-15): pass.** First session: Close Lab after Slice Atlas, reopen, click Onagro: character does not load (`EditHistoryPanel.Fill` NRE). 0.12.30: character loads. Same reopen → Animator → Slice Atlas: popup missing (`UiModal.Show` NRE). 0.12.31: Slice Atlas twice after Close Lab, no error. Moved to `docs/issues/resolved/lab-static-panels-not-reset-on-close.md`.

in game: I opened the game, went to lab, loaded onagro, went to the animator, pressed slice atlas, picked an image, opened island editor, canceled, closed the lab, entered the lab, and clicked on onagro in the project menu. The character did not load, leaving me on the project menu, and i got this error

:-1 fps:-1 vsync:-1
[Info   : Unity Log] StreammingAssets: S:/steamapps/common/Legends of Kingdom Ru
sh/legends_Data/StreamingAssets
[Info   : Unity Log] Loading AssetBundle: images - from: S:/steamapps/common/Leg
ends of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/images
[Info   : Unity Log] images - Loaded in 0.009075701 seconds
[Info   : Unity Log] Loading AssetBundle: stuff - from: S:/steamapps/common/Lege
nds of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/stuff
[Info   : Unity Log] stuff - Loaded in 0.0209569 seconds
[Info   : Unity Log] Loading AssetBundle: units - from: S:/steamapps/common/Lege
nds of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/units
[Info   : Unity Log] units - Loaded in 0.003325105 seconds
[Info   : Unity Log] Loading AssetBundle: scenario - from: S:/steamapps/common/L
egends of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/scenari
o
[Info   : Unity Log] scenario - Loaded in 0.02879751 seconds
[Info   : Unity Log] Loading AssetBundle: scenes - from: S:/steamapps/common/Leg
ends of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/scenes
[Info   : Unity Log] scenes - Loaded in 0.003074199 seconds
[Info   : Unity Log] Loading AssetBundle: templates - from: S:/steamapps/common/
Legends of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/templa
tes
[Info   : Unity Log] templates - Loaded in 0.0790174 seconds
[Info   : Unity Log] Loading AssetBundle: spritesheets - from: S:/steamapps/comm
on/Legends of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/spr
itesheets
[Info   : Unity Log] spritesheets - Loaded in 0.0007371008 seconds
[Info   : Unity Log] Loading AssetBundle: sounds - from: S:/steamapps/common/Leg
ends of Kingdom Rush/legends_Data/StreamingAssets/AssetBundles/Windows/sounds
[Info   : Unity Log] sounds - Loaded in 0.003019184 seconds
[Warning: Unity Log] cant find a gameobject of instance FadeScreen!
[Info   : Unity Log] Finished loading localizable strings - Loaded 5261 entries
in 0.1515691 seconds
[Info   :LoKR Mod API] Scene loaded: krlegendssplashscreen (mode=Single)
[Info   : Unity Log] VideoWasPlayed - ReallyLoadingAtlas 4.525528
[Warning: Unity Log] [SRDebugger] No EventSystem found in scene - creating a default one.
[Info   :LoKR Mod API] Scene loaded: transitionscene (mode=Single)
[Info   :LoKR Mod API] Scene loaded: krlegendsmainmenu (mode=Single)
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Map.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font asset [NotoSansArabic-Bold SDF].
[Info   :  LoKR Lab] CharacterLab button added under 'MainButtons' (found by name: True, layoutGroup: VerticalLayoutGroup) at anchoredPos=(197.3, -381.5)
[Info   : Unity Log] CacheSlots success
[Info   : Unity Log] slot_1, 43, 1786599184_a4272b85-c9b2-4554-8df0-66ea75138f78, 1786599184
[Info   : Unity Log] slot_2, 2247, 1786832155_2f9ea503-9fca-4d2a-ad14-c05e0206f2ed, 1786832155
[Info   : Unity Log] slot_3, 43, 1750276725_92b795b6-657e-414d-a5ef-137498a11de8, 1750276725
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/QuestsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/EncounterDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/EncounterDefinitions
[Info   : Unity Log] SriptFile: Config
[Info   : Unity Log] SriptFile-QuestCustomFunctions: eventPriorities
[Info   : Unity Log] SriptFile-QuestCustomFunctions: obelisks
[Info   : Unity Log] SriptFile-LootTables: AdventureMetagameLootTables
[Info   : Unity Log] Loading LootTable: adventure-store-metagame-2
[Info   : Unity Log] Loading LootTable: adventure-recruit-metagame-2
[Info   : Unity Log] Loading LootTable: adventure-store-metagame-3
[Info   : Unity Log] Loading LootTable: adventure-recruit-metagame-3
[Info   : Unity Log] SriptFile-LootTables: LootTableExample
[Info   : Unity Log] Loading LootTable: pipote
[Info   : Unity Log] Loading LootTable: any-armor
[Info   : Unity Log] SriptFile-LootTables: LootTables
[Info   : Unity Log] Loading LootTable: lowHealing
[Info   : Unity Log] Loading LootTable: midHealing
[Info   : Unity Log] Loading LootTable: highHealing
[Info   : Unity Log] Loading LootTable: lowLoot
[Info   : Unity Log] Loading LootTable: midLoot
[Info   : Unity Log] Loading LootTable: highLoot
[Info   : Unity Log] Loading LootTable: lowPotion
[Info   : Unity Log] Loading LootTable: midPotion
[Info   : Unity Log] Loading LootTable: highPotion
[Info   : Unity Log] Loading LootTable: lowItems
[Info   : Unity Log] Loading LootTable: midItems
[Info   : Unity Log] Loading LootTable: highItems
[Info   : Unity Log] Loading LootTable: buffItem
[Info   : Unity Log] Loading LootTable: arenaHealing
[Info   : Unity Log] Loading LootTable: arenaUpgrade
[Info   : Unity Log] Loading LootTable: maskTable
[Info   : Unity Log] SriptFile-MapModifiers: basicMapModifiers
[Info   : Unity Log] SriptFile-Items: basicItems
[Info   : Unity Log] Initializing savegame manager: disable save:False disable history:True
[Info   :  LoKR Lab] LokrLab Open from 'krlegendsmainmenu' (wasTransitioning=False).
[Info   :  LoKR Lab] HomeWorkstationScene: Loaded character 'onagro_0nzj37'.
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Warning: Unity Log] Sprite Tiling might not appear correctly because the Sprite used is not generated with Full Rect. To fix this, change the Mesh Type in the Sprite's import setting to Full Rect
[Info   : Unity Log] Finished loading asset
[Info   :LoKR Character Loader] CustomRigLoader: built 'editor-preview' from 'S:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrCharacterLab\onagro_0nzj37' (14 parts, 18 animations).
[Info   :  LoKR Lab] RigEditorScene: Preview built from S:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrCharacterLab\onagro_0nzj37 (18 animation(s) total). It loops the active clip.
[Info   :  LoKR Lab] RigEditorScene: Loaded 14 part(s) from S:/steamapps/common/Legends of Kingdom Rush/legends_Data\Mods\LokrLab\LokrCharacterLab\onagro_0nzj37 (14 restored from rig.json, 17 clip(s)). Drag to position; use the Scene Tre
e and timeline below.
[Info   :  LoKR Lab] RigEditorScene: Detected 30 pixel island(s) in 'Onagro Spritesheet.png'.
[Info   :  LoKR Lab] LokrLab CloseTo 'krlegendsmainmenu'.
[Warning: Unity Log] [SRDebugger] No EventSystem found in scene - creating a default one.
[Info   :LoKR Mod API] Scene loaded: transitionscene (mode=Single)
[Info   : Unity Log] Loading slot 1 status:EMPTY
[Info   :LoKR Mod API] Scene loaded: krlegendsmainmenu (mode=Single)
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Map.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font asset [NotoSansArabic-Bold SDF].
[Info   :  LoKR Lab] CharacterLab button added under 'MainButtons' (found by name: True, layoutGroup: VerticalLayoutGroup) at anchoredPos=(197.3, -381.5)
[Info   : Unity Log] CacheSlots success
[Info   : Unity Log] slot_3, 43, 1750276725_92b795b6-657e-414d-a5ef-137498a11de8, 1750276725
[Info   : Unity Log] slot_2, 2247, 1786832155_2f9ea503-9fca-4d2a-ad14-c05e0206f2ed, 1786832155
[Info   : Unity Log] slot_1, 43, 1786599184_a4272b85-c9b2-4554-8df0-66ea75138f78, 1786599184
[Info   :  LoKR Lab] LokrLab Open from 'krlegendsmainmenu' (wasTransitioning=True).
[Info   :  LoKR Lab] HomeWorkstationScene: Loaded character 'onagro_0nzj37'.
[Error  : Unity Log] NullReferenceException
Stack trace:
SimpleUI.UiElement`1[TSelf].Visible (System.Boolean visible) (at /home/dippity/dev/lokr-modding/bepinex/SimpleUI/UiElement.cs:92)
LokrLab.Editor.EditHistoryPanel.Fill (SimpleUI.UiStack list, SimpleUI.UiLabel emptyLabel, System.Boolean closeAfterPick) (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Character/Editor/EditHistoryPanel.cs:96)
LokrLab.Editor.EditHistoryPanel.Refresh () (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Character/Editor/EditHistoryPanel.cs:74)
LokrLab.Editor.EditHistoryPanel.BuildInto (UnityEngine.Transform parent) (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Character/Editor/EditHistoryPanel.cs:44)
LokrLab.Shell.LabShell.RebuildBottomPanels (LokrLabApi.ProjectTypeRegistration type) (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Shell/LabShell.cs:385)
LokrLab.Shell.LabShell.RebuildWorkspaceTabs () (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Shell/LabShell.cs:303)
LokrLab.Shell.LabShell.Refresh () (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Shell/LabShell.cs:170)
LokrLab.CharacterLabScene.SwitchToShell () (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/CharacterLabScene.cs:455)
LokrLab.Shell.ProjectBrowser.BeginSession (System.Func`1[TResult] factory) (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Shell/ProjectBrowser.cs:556)
LokrLab.Shell.ProjectBrowser.OpenRow (LokrLab.Shell.ProjectBrowser+BrowserRow row) (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Shell/ProjectBrowser.cs:536)
LokrLab.Shell.ProjectBrowser+<>c__DisplayClass20_0.<BuildRow>b__0 () (at /home/dippity/dev/lokr-modding/bepinex/LokrLab/Shell/ProjectBrowser.cs:177)
UnityEngine.Events.InvokableCall.Invoke () (at <cdcf33a8ae0b4acc827004fe792520c8>:0)
UnityEngine.Events.UnityEvent.Invoke () (at <cdcf33a8ae0b4acc827004fe792520c8>:0)
UnityEngine.UI.Button.Press () (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.UI.Button.OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData) (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.EventSystems.ExecuteEvents.Execute (UnityEngine.EventSystems.IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.EventSystems.ExecuteEvents.Execute[T] (UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.ExecuteEvents+EventFunction`1[T1] functor) (at <2f492fc75d2b47b4b654d391f4ba25f4
>:0)
UnityEngine.EventSystems.EventSystem:Update()

---

## Pass 2 implementations

Sections below are added when each implementation agent finishes.

### loot-anyof-chance-always-fires

**Plugin:** LokrPatch
**What changed:** `LootItemGeneratorAnyOf.AddItems` now rolls `UnityEngine.Random.Range(0f, 1f)` instead of int `Range(0, 1)`.
**How to test:**
1. Build and launch via Steam / Proton.
2. Open a vanilla chest or quest reward whose `anyOf` rows omit `chance` or set `chance = 1`, and confirm those rows still drop.
3. Install or author a tiny loot Lua whose `type = "anyOf"` has one child with `chance = 0` and one with `chance = 0.3`. Collect that reward about 10 times.
4. Check `BepInEx/LogOutput.log` for loot exceptions.
**Expected:** `chance = 1` (or omitted) still always drops. `chance = 0` never drops. `chance = 0.3` drops on some rolls and skips on others, not every time. No loot exceptions in the log.

**Result (2026-08-15): not tested.** Needs a authored `anyOf` loot Lua sample; vanilla chests alone do not show the `chance` roll bug.

in game: Im not sure how to author lua, so i do not know how to test this.

### dialog-first-no-fallback

**Plugin:** LokrPatch
**What changed:** `Dialog.Start` / `HandleReply` / `HandleContinue` use `FirstOrDefault` and call `ExitDialog` when no child passes.
**How to test:**
1. Build and launch via Steam / Proton.
2. Play a vanilla conversation (root child always passes) and confirm it still advances and can exit normally.
3. Trigger a mod/Lab/quest graph whose Start children all fail `CheckCondition`.
4. Repeat for a reply whose children all fail, and a continue node with no `type == Dialog` child that passes.
5. Confirm `LogOutput.log` has the warning and no dialog crash.
**Expected:** Vanilla dialogs unchanged. Gated-all-children graphs close (`Exit == true`) with a LokrPatch warning like `Dialog '…' node '…': Start|HandleReply|HandleContinue had no child passing CheckCondition; exiting instead of throwing.` No `InvalidOperationException`.

**Result (2026-08-15): partial.** Vanilla dialogue advances. All-children-fail graphs not triggered. Leave unresolved.

in game: vanilla dialgue work. I am not sure how to trigger the failures.

### fight-started-empty-initiative-nre

**Plugin:** LokrPatch + LokrLab
**What changed:** LokrPatch skips `ActiveUnit` calls when initiative is empty. LokrLab spawns the roster in a priority-600 `Stage.StartFight` prefix (before fightStarted), with OnFightStarted as fallback.
**How to test:**
1. Build and launch via Steam / Proton.
2. Character Lab → Sandbox → Start sandbox on `fighttesterempty` (no leftover party if possible).
3. Ability Lab Stage with the same empty template.
4. Confirm `LogOutput.log` has no `FightStartedHandler` / `StartFight` NRE.
5. Start a vanilla campaign fight and confirm initiative / first turn unchanged.
6. If a LokrPatch warning about empty ActiveUnit still appears in Lab, spawn-before-StartFight did not run.
**Expected:** No NRE aborting `FightStartedEvent`. Hero and enemy appear; first turn HUD/skills work in both Sandbox and Ability Lab Stage. Vanilla campaign fights unchanged.

**Result (2026-08-15): pass.** Sandbox and Stage: no FightStarted NRE. Campaign fight overlay that blocked the first campaign check later dismissed (see [`campaign-fight-loading-stuck.md`](../../issues/resolved/campaign-fight-loading-stuck.md)). Progression-help Next later confirmed ([`progression-help-popup-index-oor.md`](../../issues/resolved/progression-help-popup-index-oor.md)). Moved to `docs/issues/resolved/fight-started-empty-initiative-nre.md`.

in game: sandbox and stage fight work fine, no nre. When entering a save file, and starting an adventure, I get this error on index out of range. I was still able to continue and go to a fight node. Upon entering a fight node, their is an infinite loading screen. I did hear a characters voiceline when the combat started. Here is the log from the error and the most recent logs when combat node was entered.

[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Ques
tsDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Enco
unterDefinitions
[Warning: Unity Log] ScriptLoading: Duplicate script dummy in path: Balance/Enco
unterDefinitions
[Info   : Unity Log] SriptFile: Config
[Info   : Unity Log] SriptFile-QuestCustomFunctions: eventPriorities
[Info   : Unity Log] SriptFile-QuestCustomFunctions: obelisks
[Info   : Unity Log] SriptFile-LootTables: AdventureMetagameLootTables
[Info   : Unity Log] Loading LootTable: adventure-store-metagame-2
[Info   : Unity Log] Loading LootTable: adventure-recruit-metagame-2
[Info   : Unity Log] Loading LootTable: adventure-store-metagame-3
[Info   : Unity Log] Loading LootTable: adventure-recruit-metagame-3
[Info   : Unity Log] SriptFile-LootTables: LootTableExample
[Info   : Unity Log] Loading LootTable: pipote
[Info   : Unity Log] Loading LootTable: any-armor
[Info   : Unity Log] SriptFile-LootTables: LootTables
[Info   : Unity Log] Loading LootTable: lowHealing
[Info   : Unity Log] Loading LootTable: midHealing
[Info   : Unity Log] Loading LootTable: highHealing
[Info   : Unity Log] Loading LootTable: lowLoot
[Info   : Unity Log] Loading LootTable: midLoot
[Info   : Unity Log] Loading LootTable: highLoot
[Info   : Unity Log] Loading LootTable: lowPotion
[Info   : Unity Log] Loading LootTable: midPotion
[Info   : Unity Log] Loading LootTable: highPotion
[Info   : Unity Log] Loading LootTable: lowItems
[Info   : Unity Log] Loading LootTable: midItems
[Info   : Unity Log] Loading LootTable: highItems
[Info   : Unity Log] Loading LootTable: buffItem
[Info   : Unity Log] Loading LootTable: arenaHealing
[Info   : Unity Log] Loading LootTable: arenaUpgrade
[Info   : Unity Log] Loading LootTable: maskTable
[Info   : Unity Log] SriptFile-MapModifiers: basicMapModifiers
[Info   : Unity Log] SriptFile-Items: basicItems
[Info   : Unity Log] Initializing savegame manager: disable save:False disable h
istory:True
[Info   : Unity Log] Loading slot 2 status:VALID
[Info   : Unity Log] Loading slot 2 status:VALID
[Warning: Unity Log] [SRDebugger] No EventSystem found in scene - creating a def
ault one.
[Info   :LoKR Mod API] Scene loaded: transitionscene (mode=Single)
[Info   :LoKR Mod API] Scene loaded: krlegendsatlasscreen (mode=Single)
[Info   : Unity Log] FULLMETAGAME:  INIT
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Ma
p.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font a
sset [NotoSansArabic-Bold SDF].
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.Model.M
etagame.Achievements.AchievementListener!
[Error  : Unity Log] NullReferenceException: Object reference not set to an inst
ance of an object
Stack trace:
Ironhide.Legends.View.Metagame.Screens.Achievements.UIAchievements.Start () (at
<11e9574c8b034c63879fed334c1c2f7d>:0)

[Info   : Unity Log] Unkown achievement (it doesn't exist in Steam): migration_e
asy_mode
[Error  : Unity Log] NullReferenceException: Object reference not set to an inst
ance of an object
Stack trace:
FullMetagameSessionData+<>c.<CheckAchievements>b__9_2 (Ironhide.Legends.Model.Me
tagame.Achievements.AchievementInstance instance) (at <11e9574c8b034c63879fed334
c1c2f7d>:0)
System.Linq.Enumerable.Count[TSource] (System.Collections.Generic.IEnumerable`1[
T] source, System.Func`2[T,TResult] predicate) (at <55b3683038794c198a24e8a1362b
fc61>:0)
FullMetagameSessionData.CheckAchievements () (at <11e9574c8b034c63879fed334c1c2f
7d>:0)
FullMetagameSessionData+<Start>d__7.MoveNext () (at <11e9574c8b034c63879fed334c1
c2f7d>:0)
UnityEngine.SetupCoroutine.InvokeMoveNext (System.Collections.IEnumerator enumer
ator, System.IntPtr returnValueAddress) (at <cdcf33a8ae0b4acc827004fe792520c8>:0
)

[Info   : Unity Log] Unkown achievement (it doesn't exist in Steam): seen_progre
ssion_popup
[Info   : Unity Log] DummyAnalytics: startedAdventure - adventureId=forest|legen
d=Gerald
[Error  : Unity Log] ArgumentOutOfRangeException: Index was out of range. Must b
e non-negative and less than the size of the collection.
Parameter name: index
Stack trace:
System.ThrowHelper.ThrowArgumentOutOfRangeException (System.ExceptionArgument ar
gument, System.ExceptionResource resource) (at <a1e9f114a6e64f4eacb529fc802ec93d
>:0)
System.ThrowHelper.ThrowArgumentOutOfRangeException () (at <a1e9f114a6e64f4eacb5
29fc802ec93d>:0)
Ironhide.Legends.View.Metagame.Screens.ProgressionHelp.UIProgressionHelpPopup.Sh
owPage (System.Int32 index) (at <11e9574c8b034c63879fed334c1c2f7d>:0)
Ironhide.Legends.View.Metagame.Screens.ProgressionHelp.UIProgressionHelpPopup.Ne
xt () (at <11e9574c8b034c63879fed334c1c2f7d>:0)
UnityEngine.Events.InvokableCall.Invoke () (at <cdcf33a8ae0b4acc827004fe792520c8
>:0)
UnityEngine.Events.UnityEvent.Invoke () (at <cdcf33a8ae0b4acc827004fe792520c8>:0
)
UnityEngine.UI.Button.Press () (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.UI.Button.OnPointerClick (UnityEngine.EventSystems.PointerEventData
eventData) (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.EventSystems.ExecuteEvents.Execute (UnityEngine.EventSystems.IPointe
rClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) (at <2f
492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.EventSystems.ExecuteEvents.Execute[T] (UnityEngine.GameObject target
, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.Exe
cuteEvents+EventFunction`1[T1] functor) (at <2f492fc75d2b47b4b654d391f4ba25f4>:0
)
UnityEngine.EventSystems.EventSystem:Update()

[Warning: Unity Log] [SRDebugger] No EventSystem found in scene - creating a def
ault one.
[Info   :LoKR Mod API] Scene loaded: transitionscene (mode=Single)
[Info   :LoKR Mod API] Scene loaded: krlegendsmap_roguelike (mode=Single)
[Info   : Unity Log] New Map Detected
[Info   : Unity Log] ROAD-ASSIGNER
Road (Ruta1) - woods1,woods2,woods4,woods5,woods6,woods7,woods8
Assigned: C,C,N,C,C,N,C
[Info   : Unity Log] ROAD-ASSIGNER
Road (Ruta2) - plains1,plains2,plains4,plains5,plains7,plains8
Assigned: C,N,C,C,N,C
[Info   : Unity Log] Processing Node start
[Info   : Unity Log] base hide
[Warning: Unity Log] The character used for Underline is not available in font a
sset [NotoSansArabic-Bold SDF].
[Info   : Unity Log] UIMapNavController: changing state from NONE to COMIC
[Info   : Unity Log] Push slot_2 success
[Info   : Unity Log] OnSlotSaved: slot 2 success
[Info   : Unity Log] Push slot_2 failed with error code: KREQ_ERR_CLOUD_REMOTE_C
HANGED
[Info   : Unity Log] OnSlotSaved: slot 2 failed. Considreing options.
[Info   : Unity Log] LOCAL_NEWER: will catch up in the next save (etag cached).


### ability-kv-pointmagnitude-constructs-pointmult

**Plugin:** LokrPatch
**What changed:** AbilityParser ctor postfix maps `pointMagnitude` to `FunctionPointMagnitude`.
**How to test:**
1. Build LokrPatch and launch through Steam / Proton.
2. Ability Lab: New ability with a number field `pointMagnitude(pointSub(unitPosition(%TARGET), unitPosition(%CASTER)))` (or a Hit/condition that uses it).
3. Save and sandbox-reload.
4. Confirm LogOutput has no `FunctionPointMult Function needs 2 parameters` and the ability id is in `AbilitiesDefinition: Loaded`.
5. In fight, confirm the value is a length (distance), not a scaled point.
6. Confirm a two-arg `pointMult(P, 2)` skill still scalar-multiplies.
**Expected:** Ability loads; one-arg `pointMagnitude` returns `Vector3.magnitude`; `pointMult` is unchanged.

**Result (2026-08-15): not tested.** The log pasted under this heading is the campaign fight-node / progression-help session, not a `pointMagnitude` Ability Lab check.

node entered log:

29fc802ec93d>:0)
Ironhide.Legends.View.Metagame.Screens.ProgressionHelp.UIProgressionHelpPopup.Sh
owPage (System.Int32 index) (at <11e9574c8b034c63879fed334c1c2f7d>:0)
Ironhide.Legends.View.Metagame.Screens.ProgressionHelp.UIProgressionHelpPopup.Ne
xt () (at <11e9574c8b034c63879fed334c1c2f7d>:0)
UnityEngine.Events.InvokableCall.Invoke () (at <cdcf33a8ae0b4acc827004fe792520c8
>:0)
UnityEngine.Events.UnityEvent.Invoke () (at <cdcf33a8ae0b4acc827004fe792520c8>:0
)
UnityEngine.UI.Button.Press () (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.UI.Button.OnPointerClick (UnityEngine.EventSystems.PointerEventData
eventData) (at <2f492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.EventSystems.ExecuteEvents.Execute (UnityEngine.EventSystems.IPointe
rClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) (at <2f
492fc75d2b47b4b654d391f4ba25f4>:0)
UnityEngine.EventSystems.ExecuteEvents.Execute[T] (UnityEngine.GameObject target
, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.Exe
cuteEvents+EventFunction`1[T1] functor) (at <2f492fc75d2b47b4b654d391f4ba25f4>:0
)
UnityEngine.EventSystems.EventSystem:Update()

[Warning: Unity Log] [SRDebugger] No EventSystem found in scene - creating a def
ault one.
[Info   :LoKR Mod API] Scene loaded: transitionscene (mode=Single)
[Info   :LoKR Mod API] Scene loaded: krlegendsmap_roguelike (mode=Single)
[Info   : Unity Log] New Map Detected
[Info   : Unity Log] ROAD-ASSIGNER
Road (Ruta1) - woods1,woods2,woods4,woods5,woods6,woods7,woods8
Assigned: C,C,N,C,C,N,C
[Info   : Unity Log] ROAD-ASSIGNER
Road (Ruta2) - plains1,plains2,plains4,plains5,plains7,plains8
Assigned: C,N,C,C,N,C
[Info   : Unity Log] Processing Node start
[Info   : Unity Log] base hide
[Warning: Unity Log] The character used for Underline is not available in font a
sset [NotoSansArabic-Bold SDF].
[Info   : Unity Log] UIMapNavController: changing state from NONE to COMIC
[Info   : Unity Log] Push slot_2 success
[Info   : Unity Log] OnSlotSaved: slot 2 success
[Info   : Unity Log] Push slot_2 failed with error code: KREQ_ERR_CLOUD_REMOTE_C
HANGED
[Info   : Unity Log] OnSlotSaved: slot 2 failed. Considreing options.
[Info   : Unity Log] LOCAL_NEWER: will catch up in the next save (etag cached).
[Info   : Unity Log] UIMapNavController: changing state from COMIC to BUSY
[Info   : Unity Log] base Show with action
[Info   : Unity Log] UIMapNavController: changing state from BUSY to DEFAULT
[Info   : Unity Log] UIMapNavController: selecting object PortraitTeamPrefab(Clo
ne) (UnityEngine.GameObject)
[Info   : Unity Log] UIMapNavController: changing state from DEFAULT to DIALOG
[Info   : Unity Log] UIMapNavController: selecting object DialogResponseMap(Clon
e)(Clone) (UnityEngine.GameObject)
[Info   : Unity Log] ADVENTURE INDEX: 1
[Info   : Unity Log] base hide
[Info   : Unity Log] UIMapNavController: changing state from DIALOG to BUSY
[Info   : Unity Log] CinematicFinished
[Info   : Unity Log] step taken: 0
[Info   : Unity Log] !!!!! UIMapNavProxy.Update selecting pending node:
[Info   : Unity Log] UIMapNavController: changing state from BUSY to DEFAULT
[Info   : Unity Log] UIMapNavController: selecting object PortraitTeamPrefab(Clo
ne) (UnityEngine.GameObject)
[Info   : Unity Log] Push slot_2 success
[Info   : Unity Log] OnSlotSaved: slot 2 success
[Info   : Unity Log] healingHandler
[Info   : Unity Log] Healing from 8 to 8 by 0
[Info   : Unity Log] Healing from 4 to 4 by 0
[Info   : Unity Log] Healing from 3 to 3 by 0
[Info   : Unity Log] Total Healed: 0
[Info   : Unity Log] UIMapNavController: changing state from DEFAULT to BUSY
[Info   : Unity Log] Processing Node start-plains1
[Info   : Unity Log] Push slot_2 success
[Info   : Unity Log] OnSlotSaved: slot 2 success
[Info   : Unity Log] Processing Node plains1
[Info   : Unity Log] CinematicFinished
[Info   : Unity Log] EnterCombatCinematic Finished
[Warning: Unity Log] [SRDebugger] No EventSystem found in scene - creating a def
ault one.
[Info   :LoKR Mod API] Scene loaded: transitionscene (mode=Single)
[Info   : Unity Log] SpritesheetLoaderAwake - 0
[Info   : Unity Log] SpriteAnchorDB: Loading anchors from anchorDB
[Info   : Unity Log] Best configuration for device: {"dpi":240.0,"rootFolder":"S
pritesheets/iPad/","scalingVariantSuffix":"-hd","pixelsPerMeter":200.0,"minZoomF
actor":0.699999988079071,"maxZoomFactor":1.2999999523162842,"assetsBundleVariant
s":["hd"]}
[Info   : Unity Log] Spritesheet: Loading spritesheet Forest-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Mountain-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Ruins-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Dungeon-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Wasteland-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Tutorial-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Silveroak-hd
[Info   : Unity Log] Spritesheet: Loading spritesheet Arena-hd
[Info   : Unity Log] SpriteResourceManager AWAKE
[Info   : Unity Log] ProjectileViewManager enabled
[Info   : Unity Log] FXManager: Preload-Pre time: 0.3488159
[Info   : Unity Log] FXManager: Preload time: 0.4304199
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 c
lip names.
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Ma
p.Screens.UIManager!
[Info   :LoKR Mod API] Scene loaded: krlegendsfightgameplay02 (mode=Single)
[Info   : Unity Log] Start
[Info   : Unity Log] COMBAT: Loading quest: OrcAmbush with encounters: combat_or
cAmbush_4
[Info   : Unity Log] TerrainTile: Autoconfiguring
[Info   : Unity Log] HexSize: {0.55, -0.33275}
[Info   : Unity Log] Size: 24 - 24
[Info   : Unity Log] offset: {-3.85, 0.6655}
[Warning: Unity Log] cant find a gameobject of instance Ironhide.Legends.View.Ma
p.NewMapManagerComponent!
[Warning: Unity Log] The character used for Underline is not available in font a
sset [NotoSansArabic-Bold SDF].
[Info   : Unity Log] adding: Nightshadecommon_melee_attack
[Info   : Unity Log] adding: Nightshadeswarm_ai
[Info   : Unity Log] adding: Nightshadewalk_to_closest_ranged_ai
[Info   : Unity Log] adding: Shadowcommon_ranged_attack
[Info   : Unity Log] adding: ShadowkeepDistance_ai
[Info   : Unity Log] adding: Shadowget_in_range_ai
[Info   : Unity Log] adding: Shadowwalk_to_closest_ranged_ai
[Info   : Unity Log] adding: Hauntcommon_melee_attack
[Info   : Unity Log] adding: Hauntswarm_ai
[Info   : Unity Log] adding: Hauntwalk_to_closest_ranged_ai
[Info   : Unity Log] adding: Edgesnapvine_attack
[Info   : Unity Log] adding: Edgedisable_default_walk_ai
[Info   : Unity Log] adding: RandomName-93snapvine_attack
[Info   : Unity Log] adding: RandomName-93disable_default_walk_ai
[Info   : Unity Log] adding: RandomName-13common_ranged_attack
[Info   : Unity Log] adding: RandomName-13keepDistance_ai
[Info   : Unity Log] adding: RandomName-13get_in_range_ai
[Info   : Unity Log] adding: RandomName-13walk_to_closest_ranged_ai
[Info   : Unity Log] adding: RandomName-82common_melee_attack
[Info   : Unity Log] adding: RandomName-82swarm_ai
[Info   : Unity Log] adding: RandomName-82walk_to_closest_ranged_ai
[Info   : Unity Log] adding: Snakegerald_swing
[Info   : Unity Log] adding: Snakeshield_of_retribution
[Info   : Unity Log] adding: Xranger_precise_shot
[Info   : Unity Log] adding: Xranger_poison_arrow
[Info   : Unity Log] adding: Duskarcane_ray
[Info   : Unity Log] adding: Duskarcane_magic_missile
[Info   : Unity Log] adding: RandomName-75no_attack_ability
[Info   : Unity Log] adding: RandomName-75disable_default_walk_ai
[Info   : Unity Log] Push slot_2 success
[Info   : Unity Log] OnSlotSaved: slot 2 success
[Info   : Unity Log] UIFightNavController: changing state from NONE to DEFAULT
[Info   : Unity Log] UIFightNavController: selecting object FightNavProxy (Unity
Engine.GameObject)
[Info   : Unity Log] !!!!! UIFightNavProxy.OnSelect event:UnityEngine.EventSyste
ms.BaseEventData sel:FightNavProxy
[Info   :LoKR Character Loader] CustomFxLoader: 1 sprite FX, 1 projectiles, 80 c
lip names.


### ability-aoe-missing-center-keys-nre

**Plugin:** LokrPatch
**What changed:** ParseAbility prefix injects `0` for missing `AbilityAOECenterOnCaster` / `AbilityAOEAffectsCaster` when Behavior includes `AOE`.
**How to test:**
1. Build LokrPatch and launch through Steam / Proton.
2. Add a Lab or `NewAbilities` skill with `AbilityBehavior` containing `AOE`, plus Kind/Range/TeamFilter, and omit both center/affects keys.
3. Open Lab, sandbox-reload.
4. Confirm LogOutput has no NullRef / `Could not load ability` for that id and `AbilitiesDefinition: Loaded` includes it.
5. Cast in sandbox: circle is centered on the selected target (not caster); with AffectsCaster default false the caster is not in `affectedUnits` when standing in the AoE.
6. Re-save a normal Lab point-AoE template and confirm authored `0`/`1` keys still win.
**Expected:** Hand-authored AOE without those keys loads; defaults behave as false; Lab-written keys are unchanged.

### ability-ai-retreat-if-week-typo

**Plugin:** LokrPatch
**What changed:** AbilityParser ctor postfix registers `RetreatIfWeakAI` as an alias; `RetreatIfWeekAI` stays.
**How to test:**
1. Build LokrPatch and launch through Steam / Proton.
2. Confirm a vanilla troll with `retreat_if_weak_troll_ai.txt` still loads (typo key).
3. Ability Lab: New ability, OnThink (or AI block) with `Type` / action `RetreatIfWeakAI` and the same fields as the vanilla file (`Unit`, `MaxDistance`, `BrainId`, …).
4. Save, sandbox-reload.
5. Confirm LogOutput has no `Could not parse action: RetreatIfWeakAI` and the ability registers.
6. In fight, confirm the unit still produces retreat candidates (name `RetreatIfWeakAI` / comment `RETREAT`) when walkSpeedUsed is 0.
**Expected:** Both spellings parse; vanilla typo files still load; English spelling no longer drops the parent ability.

### ability-ai-per-affected-not-action

**Plugin:** LokrPatch
**What changed:** ParseActionList prefix removes `PerAffectedAI` children (warns) and ctor postfix unregisters the type; no fake AbilityAction.
**How to test:**
1. Build LokrPatch and launch through Steam / Proton.
2. Ability Lab: New ability with a real Hit (or Delay). Save it, then hand-drop `PerAffectedAI { }` into that `ability.txt` on disk.
3. Sandbox-reload.
4. Confirm LogOutput warns that `PerAffectedAI` was skipped (`PerAffectedAI is an AIEvaluator, not an AbilityAction; skipped.`), there is no `InvalidCastException` / `Could not load ability`, and the Hit still registers.
5. Confirm vanilla abilities (none use this type) still load.
**Expected:** Ability loads with the Hit intact; `PerAffectedAI` is skipped, not a parse-killer.

**LokrLab:** Save is blocked if any card (including opaque) is `PerAffectedAI`; it stays off the Add-card menu.
**How to test (Lab):**
1. Ability Lab: New ability with a real Hit (or Delay) plus an opaque/OnThink `PerAffectedAI { }`.
2. Save.
**Expected (Lab):** Save is blocked with “PerAffectedAI is not an AbilityAction; the loader skips it. Use a real OnThink action (GetInRangeAI, KeepDistanceAI2, RetreatIfWeekAI, …).” Add action does not list PerAffectedAI.

### ability-callfunction-empty-filter-throws

**Plugin:** LokrPatch
**What changed:** Prefixes on the six CallFunction `Execute` methods skip-and-log when the named filter matches nobody.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, open Ability Lab.
2. New sandbox skill, CallFunction = `ClosestTargetPreferNoFlip` with a UnitFilter that matches nobody; cast — skill continues, log has `ClosestTargetPreferNoFlip: skipped empty UnitFilter.`, no `IndexOutOfRangeException`.
3. Repeat for `KrumSelectTargets` (empty UnitFilter), `WBFIrizaTeleportTarget` (empty HeroFilter), and `WBFOverseerSelectTentacleSpawn` with empty markers and/or empty HeroFilter.
4. Confirm a vanilla named fight that uses one of these helpers (Krum / Iriza / Overseer) still selects targets when units/markers exist.
**Expected:** Empty-filter Lab casts skip `Actions` and warn; vanilla fights with units on the board still run original `Execute`.

**LokrLab:** Status warns when CallFunction is one of the six empty-filter helpers; the picker still lists them.
**How to test (Lab):**
1. New sandbox skill, CallFunction = `ClosestTargetPreferNoFlip` (and repeat for `KrumSelectTargets`, `WBFIrizaTeleportTarget`, `WBFOverseerSelectTentacleSpawn`).
2. Save and read the status line.
**Expected (Lab):** Status warns that those functions throw if the filter matches nobody. The six names remain in the CallFunction picker.

### ability-ai-empty-brain-divide-by-zero

**Plugin:** LokrPatch
**What changed:** `AIDecisionScoreEvaluator.Eval` prefix returns 0 and warns when considerations are null or empty.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, Ability Lab.
2. Add AIConfigB with empty `Considerations { }`, Weight 1, plus an OnThink action (`GetInRangeAI` or similar); sandbox vs an AI unit.
3. First think: no `DivideByZeroException`; log has `AIDecisionScoreEvaluator.Eval: empty considerations; returning 0.`; unit does not pick that brain as a sure thing.
4. Confirm a vanilla AI unit still evaluates brains with real considerations.
**Expected:** Empty brains score 0 instead of crashing; vanilla brains unchanged.

**LokrLab:** Status warns on AIConfigB / AIBrain* blocks whose InnerKv is empty or whose Considerations list has no real children.
**How to test (Lab):**
1. Add AIConfigB (empty InnerKv), or paste the empty-Considerations KV from the issue.
2. Save/load and read the status line.
**Expected (Lab):** Status warns that empty Considerations divides by zero on think. Save is still allowed.

### ability-equal-null-lhs-nre

**Plugin:** LokrPatch
**What changed:** `FunctionEqualsObjectExpression.GetFloat` prefix uses null-safe `object.Equals`.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, Ability Lab sandbox.
2. Conditional or expression using `equal(%MISSING, %TARGET)` — evaluates to 0, no NRE.
3. `equal(%TARGET, %TARGET)` still 1 when TARGET is set.
4. `isNull(%MISSING)` still works.
5. Cast a vanilla skill that uses `equal(%DEAD, %TARGET)` or `equal(activeUnit(), %HITTARGET)` and confirm behavior unchanged when both sides are non-null.
**Expected:** Missing LHS is false (0); both-null is true (1); vanilla non-null equals unchanged.

### ability-each-in-list-actions-if-empty-inverted

**Plugin:** LokrPatch
**What changed:** `EachInListAction.Execute` prefix runs `ActionsIfEmpty` only when `list.Count == 0`.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, Ability Lab Advanced/opaque EachInList.
2. Empty List + non-empty ActionsIfEmpty (e.g. PlaySound) — fallback fires, no Actions loop.
3. Non-empty List + ActionsIfEmpty — iterator Actions run, fallback does not.
4. Confirm a vanilla skill that uses EachInList without ActionsIfEmpty is unchanged.
**Expected:** Fallback matches the parse-key name; shipped KV that omits `ActionsIfEmpty` is unchanged.

### ability-tooltip-missing-var-returns-999

**Plugin:** LokrPatch
**What changed:** Prefixes on AbilityInstance / ModifierInstance `ResolveFloatVariable` return 0 (and warn) when the key is missing.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, Ability Lab.
2. Skill loc uses `{missingVar}` with no AbilitySpecial of that name; hover tooltip in sandbox (and encyclopedia if the skill is on a hero) shows 0, not 999.
3. Confirm LogOutput has `SkillTooltip: missing variable 'missingVar' … returning 0.`
4. A real AbilitySpecial `{myVar}` with value 5 still shows 5 (including a configured 999).
5. Vanilla skill tooltips that resolve shipped keys still match live numbers.
**Expected:** Missing keys display 0; real values including 999 are untouched.

### activity-interface-point-target-nre

**Plugin:** LokrPatch
**What changed:** Prefixes null-check `targetFilter` on IsPossibleTargetIgnoringPosition, GetValidTargets, GetValidTargetsIgnoringRange, and SetCenter.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, Ability Lab.
2. Envelope `MELEE | POINT_TARGET`, select the skill in sandbox — no NRE, no unit candidates (empty).
3. POINT_TARGET skill whose OnThink has GetCloseToUnitAI — think does not NRE.
4. Vanilla melee UNIT_TARGET and pure POINT_TARGET AOE (point_aoe template) still target as before.
**Expected:** POINT_TARGET+MELEE is a no-op for unit targeting instead of a crash; vanilla targeting unchanged.

**LokrLab:** Status warns on MELEE+POINT_TARGET and on GetCloseToUnitAI when Behavior includes POINT_TARGET.
**How to test (Lab):**
1. Envelope `MELEE | POINT_TARGET`, save, read status.
2. POINT_TARGET skill whose OnThink has GetCloseToUnitAI, save, read status.
**Expected (Lab):** Status warns that melee select NullRefs without the patch, and that GetCloseToUnitAI under POINT_TARGET NullRefs.

### stats-apply-modifier-missing-stat-throws

**Plugin:** LokrPatch
**What changed:** New `Stats.ApplyModifier` prefix skip-and-logs missing *stat keys*; remaining keys on the same modifier still apply.
**How to test:**
1. Build LokrPatch, launch via Steam / Proton, Ability Lab.
2. Modifier PropertiesAdd with a typo stat id plus a valid key (e.g. `#health`); apply in sandbox — no `could not find stat with key` throw; log has `Stats.ApplyModifier: skipped missing stat '…'`; the valid key still applies.
3. Vanilla modifier that adds `#health` / `#armor` still changes those stats.
4. ApplyModifier with a missing *modifier id* still hits `ApplyModifierMissingPatch` (`ApplyModifier: skipped missing modifier '…'`), not this patch.
**Expected:** Typo'd stat keys are skipped; other keys on that modifier apply; missing modifier ids still use the existing action patch.

**LokrLab:** Status warns on PropertiesAdd / PropertiesMult keys and SetStat Stat fields that are not in StatRefs.
**How to test (Lab):**
1. Modifier PropertiesAdd with a typo stat id; save.
2. SetStat with an unknown Stat; save.
**Expected (Lab):** Status warns that the unknown key is not a known stat. Save is still allowed.

### animator-pose-leaks-across-frames

**Plugin:** LokrLab
**What changed:** Clip/frame switches cancel the viewport drag after committing the old context, so mouse-up cannot write into the new frame.
**How to test:**
1. Launch through Steam / Proton, open Character Lab Animator, Mass Edit off, two frames in one clip plus a second clip.
2. Move tool: drag a part on frame 1 and release on the frame 2 chip.
3. Same drag, release on a different clip in the Node Tree.
4. Hold a drag and tap `]` / `[`.
5. Repeat with Inspector Pos fields (0.12.7 path) and confirm Pivot is still rest-wide.
6. Mass Edit on: one drag still propagates across the clip while playback can run.
**Expected:** Frame 1 keeps the edit; frame 2 / the other clip do not. `[` / `]` leave the edit on the frame where the drag began. Inspector Pos still drops a late commit. Mass Edit playback does not snap the dragged part every tick.

### sandbox-forfeit-confirm-behind-settings

**Plugin:** LokrLab
**What changed:** While Forfeit Yes/No is visible, Lab mutes `UIOptions` (alpha 0, no raycasts) and restores it after dismiss.
**How to test:**
1. Character Lab Sandbox, start fight.
2. Open settings, press Forfeit.
3. Click Yes (forfeit) and, in a second run, click No.
4. After No, use settings audio / close.
**Expected:** Yes/No is visible and clickable in the hole. Yes forfeits; No returns to a working settings sheet. LogOutput has one `EmbeddedSceneHudFitter: forfeit confirm visible.` line with type/parent/canvas fields.

### ability-hit-closed-tag-whitelist

**Plugin:** LokrLab
**What changed:** New ranged template writes `#PROJECTILE, #TARGETED`; the Hit Tags picker and save check use the ValidateTags list.
**How to test:**
1. Build LokrLab, launch through Steam / Proton, open Ability Lab.
2. File → New Ability → Ranged projectile, save, sandbox-reload.
3. Open the Hit Tags picker.
4. Type `#RANGED` into Tags and Save.
5. Fire the new ranged skill in sandbox.
**Expected:** No `HitAction is using invalid Tags` in LogOutput; ability registers. Picker lists `#PROJECTILE` / `#TARGETED` / `#MELEE` / … and not `#RANGED`, `#SKULL`, `#NON_TARGETABLE`, or `#TowerCultist1`. Save is blocked on `#RANGED`. Projectile-hit VFX / `hasTags(..., #PROJECTILE, #TARGETED)` still treat it as a projectile. Existing `#RANGED` files rewrite to `#PROJECTILE` on load.

### ability-aoe-range-cone-empty

**Plugin:** LokrLab
**What changed:** Envelope AOE dropdown omits RANGE_CONE for new picks and warns if a file already has it.
**How to test:**
1. Build, launch via Steam/Proton, Ability Lab Envelope with AOE on.
2. Open the AOE kind dropdown on a new/circle ability.
3. Hand-set or load a file with `AbilityAOEKind RANGE_CONE`.
4. Compare RANGE_CIRCLE / RANGE_TUNNEL in sandbox.
**Expected:** Dropdown does not offer RANGE_CONE for a new/circle ability. Loaded cone keeps the value, status warns that combat never fills cone hexes, sandbox preview/hits stay empty. RANGE_CIRCLE / RANGE_TUNNEL still fill hexes.

### ability-events-never-dispatched

**Plugin:** LokrLab
**What changed:** Add-event menus hide names with no fire site; loaded dead hats still show and warn.
**How to test:**
1. Build, launch via Steam/Proton, Ability Lab.
2. Add event hat — confirm OnAttackStart / OnAttackAction / OnAttacked are absent.
3. Add modifier event — confirm the longer dead list is absent.
4. Paste KV with `OnAttackStart`, load the file.
5. Cast a melee sandbox skill that uses OnAbilityAction.
**Expected:** Dead names are not offered. Pasted `OnAttackStart` loads, hat shows, status warns, sandbox never runs that hat. OnAbilityAction still fires on a melee sandbox skill.

### portrait-patches-self-parent

**Plugin:** LokrCharacterLoader
**What changed:** Deleted the no-op `SetParent(self)` at the end of `ReplaceWithFlatImage`.
**How to test:**
1. Launch through Steam (Proton). Equip a hero with `Portraits/<id>/<id>_MAP.png` and `_CHALLENGE.png`.
2. On the adventure map, check the hero-bar MAP portrait.
3. Open a challenge/reward portrait and the buff store; check CHALLENGE flats.
4. Check `BepInEx/LogOutput.log` after those screens.
**Expected:** Flat PNGs stay in the frame, not detached. No Unity “parented to itself” error.

### portrait-patches-hardcoded-hierarchy

**Plugin:** LokrCharacterLoader
**What changed:** MAP `SetHero` now uses `portraitData` and skip-and-logs if it is null.
**How to test:**
1. Launch through Steam. Use a hero with `<id>_MAP.png`.
2. Adventure-map hero bar: confirm the flat MAP portrait is in the mask/frame.
3. Open Team Manage / hero manage so `SetHero` runs again.
4. Check `LogOutput.log` for NREs from `Find` / `.gameObject`.
**Expected:** Portrait still appears after manage. No `Find` NRE. A missing `portraitData` would log `portraitData is null — skip MAP flat image` and leave vanilla exo.

### portrait-patches-buff-store-index

**Plugin:** LokrCharacterLoader
**What changed:** Buff-store postfix bounds-checks `GetAllHeroes()` before indexing `heroPosition`.
**How to test:**
1. Launch through Steam. Start a run with the usual three heroes. Open the map buff store.
2. Confirm CHALLENGE flats for heroes that have `<id>_CHALLENGE.png`; vanilla exo for those that do not.
3. Buy a buff (vanilla re-calls `SetItem`) and confirm no throw.
4. Check `LogOutput.log` for `IndexOutOfRangeException` from this postfix.
**Expected:** Portraits still show. No index exception. A stale index would log `heroPosition N out of range` and leave vanilla’s already-applied exo.

### find-part-index-unvalidated

**Plugin:** LokrCharacterLoader
**What changed:** Skip-and-log when `FindPartIndex` is `-1` in `ReplacePart` / `UpdateHeroes`; `SetFlagVisibility` skips that unit.
**How to test:**
1. Launch through Steam. Adventure map with a vanilla party: tokens, banner hide/show, and movement.
2. Give a custom hero a `_MAPMINI.png` and a `unitOnMap` name that is not on the party-token template; enter map / add a unit.
3. Use a custom rig as a party token without `Asst_Party_Banner`; load the map.
4. Check `LogOutput.log` for skip warnings and NREs.
**Expected:** Vanilla tokens still work. Missing `unitOnMap` keeps the template silhouette (warning). Missing banner: map loads, `SetFlagVisibility` does not NRE (`Asst_Party_Banner not found` once per exo).

### exo-skeleton-null-unitdefinition

**Plugin:** LokrCharacterLoader
**What changed:** Null-guard `unitDefinition`/`metaExo`; seed-then-original is unchanged when a definition exists.
**How to test:**
1. Launch through Steam. Vanilla hero on the map and in a fight.
2. Custom-rig hero (`metaExo` matching a `rig/rig.json` folder): map hero bar and combat view.
3. Check `LogOutput.log` during map load and one fight start.
**Expected:** Vanilla exo unchanged (custom-rig path does not run). Custom-rig hero still swaps. No NRE from `HeroExoSkeletonPatches` / `UnitViewExoSkeletonPatches`.

### reload-data-missing-sprite-nre

**Plugin:** LokrCharacterLoader
**What changed:** `CustomRigLoader.Build` pre-filters JSON like `LoadParts`; no placeholder meshes; failed builds are not cached.
**How to test:**
1. Launch through Steam. Character Lab: Preview a known-good rig (`rig.json` names match `sprites/*.png`).
2. Duplicate a Lab character, rename one PNG so it no longer matches a `parts[].name` (or add a bogus part name). Preview / live reload.
3. A rig whose every part name misses: Preview again.
4. Confirm a frame that named only the missing part does not crash later in `ExoSkeletonRenderer.LateUpdate`.
**Expected:** Good rig still builds. Mismatch: `Cant find sprite named: …`, no `NullReferenceException` on `sprite.vertices`, other parts still draw. All-miss: `has no parts matching packed sprites — build failed`, Lab preview does not crash, `builtRigsById` does not cache a broken asset.

### invisibility-exit-fires-every-turn

**Plugin:** LokrCharacterLoader
**What changed:** INVISIBLE exit/enter fire only on on→off / off→on edges; tint is the unit’s own `"Graphic"` renderer.
**How to test:**
1. Launch through Steam. Fight with Assassin plus at least one other hero and one enemy.
2. Apply Assassin invisibility.
3. End turns on non-Assassin units while Assassin is still invisible.
4. When Assassin’s INVISIBLE expires at turn end (or after a stealth-breaking action that clears it before postfix), check opacity.
5. Repeat a fight with no Assassin.
**Expected:** Only Assassin’s combat mesh goes translucent. Assassin stays translucent while others end turns. Opacity returns to 1 on Assassin only. No unit is tinted in a no-Assassin fight. Log is not doing a global `FindObjectsOfType` every turn.

### skills-bar-five-slot-cap

**Plugin:** LokrCharacterLoader
**What changed:** Campaign-wide Harmony trim of `match.skills` to five hexes (not only Lab `EmbeddedFightHost`).
**How to test:**
1. Build and launch via Steam / Proton. Confirm `LoKR Character Loader v1.1.12 loaded`.
2. Campaign fight with a custom hero whose base `skills` list has six or more interactive abilities.
3. Same hero in Character Lab Sandbox (base list of six, not only progression).
4. Vanilla hero at max level: all five slots.
5. Click / hotkeys Skill1–Skill5.
**Expected:** Bar shows five, no `ArgumentOutOfRangeException`, extras omitted (`SkillsBar: unit '…' has N extra interactive skill(s) beyond 5 hex slots`). Lab sandbox still five. Vanilla five slots unchanged. Hotkeys match visible icons.

### save-sanitize-drops-unknown-ids

**Plugin:** LokrPatch
**What changed:** `Sanitize` logs unknown adventure/hero/quest/item ids and leaves the run; Hero/Inventory/Map Load stows unknowns and Save appends them back.
**How to test:**
1. Steam/Proton: start an adventure with a custom hero, a custom item in inventory, and (if you have one) a custom quest still on the map; save and quit to desktop.
2. Disable or uninstall that content pack (or Lab-reload so the ids are unregistered), launch, load the slot.
3. Re-enable the pack, load the same slot again.
4. Repeat with only a custom `currentAdventure` missing.
5. Load and save a vanilla-only slot (Gerald/Ranger/ArcaneMage, stock items).
**Expected:** Step 2: run still in progress (not an empty `CreateEmpty` run), slot stays `VALID`, log has `Sanitize: unknown … leaving run intact` and matching `….Load: stowed unknown …` lines. Step 3: custom hero, item, and quest ids are still in the save. Step 4: run is not discarded (`Sanitize: unknown currentAdventure`). Step 5: no extra unknown-id warnings and no party/inventory change.

**Result (2026-08-15): pass (hero path).** Onagro in party + Krum'thak, quit, hide folder, load: run intact, Continue available. Restore folder, load: Onagro still in the save. Item / quest / missing-adventure / vanilla-slot steps not run. Slot compact and ghost third portrait filed as [`party-stow-shifts-remaining-into-wrong-slots.md`](../../issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md) — do not treat that as a Sanitize failure.

### save-party-reset-to-vanilla-trio

**Plugin:** LokrPatch
**What changed:** `HeroRosterManager.Load` keeps known uniqueIds in save order, stows the rest, and no longer resets when `Count != 3`.
**How to test:**
1. Steam/Proton: put four registered custom heroes in the party, save, quit, load.
2. Three custom heroes in the party; disable one definition; launch and load.
3. Re-enable that definition and load the same slot again.
4. Load and save a vanilla Gerald/Ranger/ArcaneMage slot.
**Expected:** Step 1: party still has those four uniqueIds; log has no trio reset. Step 2: the two known ids remain; log has `HeroRosterManager.Load: stowed unknown party uniqueId '…'`; live party is not Gerald/Ranger/ArcaneMage. Step 3: the missing id is back in the party. Step 4: still exactly that trio. (Sanitize without this patch, or this patch without Sanitize, still loses a mixed/4-hero party — both must be in this build.)

**Result (2026-08-15): partial.** Steps 2–3 via the Onagro hide/restore above: known ids kept (not Gerald/Ranger/ArcaneMage reset); Onagro returned. Step 1 (four custom heroes) and step 4 (vanilla trio load/save) not run. Compact-into-legend-slot / ghost portrait is [`party-stow-shifts-remaining-into-wrong-slots.md`](../../issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md), not a trio-reset regression.

### inventory-additem-never-sets-id

**Plugin:** LokrPatch
**What changed:** `AddItem` postfix assigns a GUID when `ItemInstance.id` is null/empty; loaded ids are not remapped.
**How to test:**
1. Steam/Proton: new run; buy or loot an item that was not in the starting inventory; save.
2. Inspect that inventory row in the slot JSON (or load again and save).
3. Grant a second copy of the same archetype.
4. Grant an auto-consume item.
5. Load a vanilla starting-inventory slot and save.
**Expected:** Step 2: that row’s `id` is a non-empty GUID. Step 3: quantity increments and the existing id is kept. Step 4: no inventory row, no error. Step 5: ids from the save file unchanged.

**Result (2026-08-15): pass.** Mystery elixir (new row) had a GUID in the save. Tent stack 2→3 kept the existing id. Moved to `docs/issues/resolved/inventory-additem-never-sets-id.md`.

### hero-update-skills-unknown-id-nre

**Plugin:** LokrPatch
**What changed:** `UpdateSkills` skips a missing ability after the vanilla log and clamps the unused `{1,2,3}` / `{0,1,2,3}` indexers.
**How to test:**
1. Steam/Proton: recruit or load a custom hero whose `skills` list contains a typo / unregistered ability.
2. Character Lab live-reload that drops an ability the hero still lists; open the map/hero room.
3. Level-up a vanilla hero 1→2→3.
4. Load vanilla Gerald.
**Expected:** Step 1: no NRE; log shows `HERO: Can't find skill` plus `Hero.UpdateSkills: skipped unknown skill '…'`; other real skills still apply. Step 2: map/hero room still opens. Step 3: progression picks unchanged. Step 4: no extra skip warnings.

### hero-progress-window-unknown-uniqueid-nre

**Plugin:** LokrPatch
**What changed:** `ShowHeroProgress` drops bars for unknown uniqueIds; unlock anim is skipped when the ability is null, and `gainedLevel` still sets.
**How to test:**
1. Steam/Proton: win a fight with a custom hero still registered.
2. Win after Lab-reload / pack uninstall left that uniqueId on `HeroManager` but not in `DefinitionsByUnique`.
3. Win with the vanilla trio when a rank-up grants a known skill.
4. Rank-up a custom hero whose unlock skill id is missing from `abilities`.
**Expected:** Step 1: XP bars and rank-up skill unlock anim play as vanilla. Step 2: victory window opens; that hero’s bar is omitted; other party bars animate; log has `HeroProgressWindow.ShowHeroProgress: skipped unknown uniqueId '…'`. Step 3: three bars, unlock anim when a rank-up grants a known skill. Step 4: rank-up still completes, unlock icon anim skipped (`skipped unlock skill animation`), no NRE.

### map-start-unknown-starting-hero-nre

**Plugin:** LokrPatch
**What changed:** empty-party `StartingHeroes` skip-and-log unknown uniqueIds; `startingHeroes` on disk is not rewritten to the vanilla trio.
**How to test:**
1. Steam/Proton: new custom adventure whose `StartingHeroes` are all registered.
2. Same adventure with one uniqueId typo / unregistered.
3. Adventure whose starting uniqueIds are all unknown.
4. Vanilla campaign new run.
**Expected:** Step 1: map starts with those heroes. Step 2: map still starts; known heroes are added; log has `NewMapManagerComponent.Start: skipped unknown StartingHeroes uniqueId '…'`; save still lists the unknown id under `startingHeroes`. Step 3: no NRE aborting the scene; log has `no spawnable StartingHeroes uniqueIds; live party left empty`; party is not Gerald/Ranger/ArcaneMage. Step 4: unchanged party spawn.

### map-hud-unknown-modifier-config-nre

**Plugin:** LokrPatch
**What changed:** map hero bar, initiative portrait, unit-detail, and map-reward UI skip a null `HeroModifiersAsset` config instead of reading `overheadIcon` / `modifierIcon`.
**How to test:**
1. Steam/Proton: campaign map with a custom map modifier that has no `HeroModifiersAsset` row; open the hero bar, initiative portrait, and unit-detail panel.
2. Confirm a vanilla modifier with a config row still shows left/right overhead icons and tooltips.
3. Trigger a map reward that applies a custom modifier with no config.
4. Vanilla map HUD with stock modifiers only.
**Expected:** Step 1: those panels open without NRE; log names the missing key (`MapHeroBarPortraitModifiers` / `PortraitInitiativeMapModifiers` / `UnitDetailMapModifiers: skipped missing HeroModifiersAsset key '…'`); other modifiers still show icons. Step 2: vanilla icons/tooltips unchanged. Step 3: rewards UI does not crash (`RewardViewComponent.SetReward: skipped missing …`). Step 4: no extra skip warnings.

### lab-alias-loc-keys-not-expanded

**Plugin:** LokrCharacterLoader 1.1.13 + LokrLab 0.12.29
**What changed:** `$alias` expand keeps `_NAME_0001` / `_LORE` suffixes; Lab writes `UNIT_<uniqueId>_*` stems.
**How to test:**
1. `dotnet build` so Character Loader 1.1.13 and Lab 0.12.29 are deployed.
2. Cold-start Steam / Proton. New game (fresh save).
3. Open the hero roster.
4. Check Assassin (`assassin_z7v9v1`) and Musketeer (`musketeer_c3awgr`) cards.
5. Open each card's lore / detail if the roster shows it.
6. Confirm Onagro still says Onagro.
**Expected:** Assassin and Musketeer show "Assassin" / "Musketeer" (and Musketeer lore), not `UNIT_ASSASSIN_Z7V9V1_NAME` / `UNIT_MUSKETEER_C3AWGR_NAME`. Vanilla locked Musketeer is still a separate card. Log has no loc-key miss for those stems.

---

## Follow-ups from 2026-08-15 in-game notes

### lab-static-panels-not-reset-on-close (retest after 0.12.31)

**Plugin:** LokrLab 0.12.31
**What changed:** `MenuBarPanel.EnsurePopups` rebuilds Slice Atlas / Save / Import when the modal GameObject is Unity-destroyed. `IslandAtlasPickerPanel.Open` rebuilds if needed.
**How to test:**
1. `dotnet build` so LokrLab 0.12.31 is deployed. Restart the game.
2. Lab → load Onagro → Animator → Slice Atlas → pick image → Island editor → Cancel → Close Lab.
3. Reopen Lab. Click Onagro in the Project Browser (must load).
4. Open Animator. File → Slice Atlas: popup must appear. Cancel. Pick Islands from that popup still opens.
**Expected:** No `UiModal.Show` / `EditHistoryPanel` NRE. Slice Atlas popup shows after reopen.

**Result (2026-08-15): pass.** No more errors opening Slice Atlas twice after Close Lab. Moved to resolved.

### ability-kv-parse-empty-filename (retest after disk fix)

**Plugin:** none (Lab `ability.txt` rewritten on disk)
**How to test:** Open Lab, load Assassin, start sandbox. Check `LogOutput.log` for `ERROR PARSING` on `assassin_quickstep_qy4z6j`.
**Expected:** That path is gone. Other Assassin skills still load. Do not re-import Official Pack Assassin until the pack line is fixed.

**Result (2026-08-15): pass.** Assassin `ERROR PARSING` gone. Moved to resolved.

### progression-help-popup-index-oor

**Plugin:** LokrPatch 1.0.6
**What changed:** `ProgressionHelpPopupPatch` clamps `ShowPage` / `Next` against `titles` and `pages`.
**How to test:** Continue a save, start an adventure, click Next on the progression-help popup.
**Expected:** After a LokrPatch: no `ArgumentOutOfRangeException` from `UIProgressionHelpPopup.ShowPage`. Until then this is the known throw; you can still reach the map.

**Result (2026-08-15): pass.** Next through the popup; no `ShowPage` / `Next` index exception. Moved to `docs/issues/resolved/progression-help-popup-index-oor.md`.

### campaign-fight-loading-stuck

**Plugin:** none yet (filed)
**How to test:**
1. Cold start, never open Lab: Continue → Forest fight node. Does the overlay clear?
2. Same process after Lab Close: same fight node.
**Expected:** Combat is playable (skills, end turn). Overlay dismisses after `krlegendsfightgameplay02` loads. Compare A vs B before patching FadeScreen.

**Result (2026-08-15): pass.** Loading overlay over the fight dismisses. Moved to `docs/issues/resolved/campaign-fight-loading-stuck.md`.

### party-stow-shifts-remaining-into-wrong-slots

**Plugin:** LokrPatch 1.0.10 + LokrCharacterLoader 1.1.15
**What changed:** Empty core slots use `DEFAULT_MINI`; unused fourth recruit slot is hidden; run brief still ignores roster.
**How to test:**
1. Restart so log shows Patch `1.0.10` and Character Loader `1.1.15`.
2. Adventure continue with Onagro missing: gold slot shows DEFAULT placeholder (not white); fourth slot hidden unless a fourth run hero exists.
3. Title-screen save card: same. Equip another legend: brief still does not show that legend in the run.

**Result (2026-08-15): pass.** Empty gold slot uses DEFAULT_MINI; fourth recruit slot hidden; save card matches; Onagro returns to the legend slot on restore. Moved to `docs/issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md`.


