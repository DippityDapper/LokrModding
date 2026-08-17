# LokrEncyclopedia: unlocked button click is unverified

Area: LokrEncyclopedia
Status: resolved

As of 2026-08-14: the plugin makes `UIMainMenu.encyclopediaButton`
visible and interactable. A full search of the decompiled base game
finds only that field (used to cache a layout position) — no window
class, click handler, or content list. What happens on click is
unverified; it may be a serialized scene event, or it may do nothing.
Documented in `docs/capabilities-and-gaps.md` §2.4 and
`LokrEncyclopedia/docs/overview.md`.

Suggested fix: click the button in-game on the main menu. If it opens
nothing, either ship the plugin disabled by default or document that
the button is a no-op. Do not add an Encyclopedia UI unless the base
game already has one to unlock.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** verify-only
**Approach:** No Harmony change this pass. `LokrEncyclopedia` already postfixes `UIMainMenu.Start` to set `encyclopediaButton.enabled` and `gameObject.SetActive(true)`. Decompiled `UIMainMenu` only caches that button's Y for layout; there is no window class, `OnClick` handler, or content list in C#. What happens on click is a serialized UnityEvent (or nothing). Confirm that in a running session before any code.
**Exact change:** None until the click is observed. After that session: if a vanilla window opens, leave the postfix as-is and move this issue only after that confirm. If the click is a no-op, either skip `PatchAll` unless a default-false `Config.Bind` Enabled is on, or document the no-op in `LokrEncyclopedia/docs/overview.md` and `docs/capabilities-and-gaps.md` §2.4 — pick one, do not invent UI.
**Do not:** Design or ship an Encyclopedia window, list, or click handler. Do not patch `UIMainScreen` (boot title); the field lives on `UIMainMenu` (hub after Continue / New Game). Do not treat "button is visible" as resolved.
**In-game verify:** 1. Launch through Steam. 2. From the title screen, Continue (or New Game) into the main-menu hub (`UIMainMenu` — Party / Achievements / Arena). 3. Click Encyclopedia. 4. Note whether anything opens, the game no-ops, or it errors. 5. If no-op, follow Exact change (disable-by-default or document); do not add UI.
**Risk:** None for verify. Disable-by-default only hides a shipped-disabled control. A new Encyclopedia UI would be a vanilla rewrite and is out of scope.

Resolved: 2026-08-15

Resolution: Clicking Encyclopedia on the main-menu hub shows the vanilla
**Coming Soon!** popup next to the button. No C# window exists to unlock;
the postfix stays as-is. Documented in `LokrEncyclopedia/docs/overview.md`
and `docs/capabilities-and-gaps.md` §2.4. Do not invent Encyclopedia UI.
