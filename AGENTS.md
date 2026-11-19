# AGENTS.md

## Project overview

Random Events is a C#/.NET Framework 4.8 mod for **Software Inc.** (game assembly version/API dependent). It adds two kinds of events:

- **Random events**: evaluated on each game hour and selected probabilistically.
- **World events**: dated historical events evaluated when a game month passes; they are written to the in-game newspaper and automatically choose their first option.

The repository contains the Visual Studio project under `src\`. The root `README.md` describes the original manual installation flow, and `src\content.txt` is the reference list of supported conditions and effects.

## Repository layout

- `src\RandomEvents.sln`, `src\RandomEvents.csproj`: Visual Studio solution/project. The project produces `RandomEvents.dll`.
- `src\EventsHandlerV2.cs`: mod entry point and event scheduler. Loads `.tyd` files, registers console commands, and subscribes to `TimeOfDay` hour/month events.
- `src\Models\Event.cs`: `Event` and `EventOption`; condition checks, event firing, option selection, notifications, and cooldown state.
- `src\Conditions\`: condition parser and evaluators.
  - `ConditionExpression.cs` parses a condition into name, operator, and value.
  - `MainConditionEvaluator.cs` produces candidate game objects or evaluates scalar/date conditions.
  - `SubConditionEvaluator.cs` filters candidates returned by a main condition.
  - `VariableCondition.cs` combines a main condition with subconditions using `&`.
- `src\Effects\`: effect model and dispatchers for company, product, employee, market, and miscellaneous effects.
- `src\UI\EventWindow.cs`: popup UI for random events and newspaper behavior for world events.
- `src\Helpers.cs`, `src\Utils.cs`: Unity/game UI and general utility helpers.
- `src\MyModMeta.cs`: mod options screen and persisted settings.
- `src\random_events.tyd`: random event definitions.
- `src\world_events.tyd`: dated world event definitions.
- `src\meta.tyd`, `src\content.txt`, `src\Todo.txt`: mod metadata, supported vocabulary/reference notes, and outstanding work.
- `src\bin\` and `src\obj\`: generated build output; do not edit or use as source.

## Runtime flow

1. The game constructs `EventsHandlerV2` as a `ModBehaviour`.
2. `Start()` loads persisted settings:
   - `EventMode`: `1` random only, `2` world only, `3` both.
   - `SkipFastForward`: whether hourly random events are suppressed while fast-forwarding.
   - `Cooldown`: hourly cooldown after an option is selected (the default in code is 24 hours).
3. `OnActivate()` clears old definitions, loads `random_events.tyd` and `world_events.tyd`, registers console commands, and subscribes to `TimeOfDay.OnHourPassed` and `TimeOfDay.OnMonthPassed`.
4. On an hour, `AutoEvent()` shuffles random events, observes the running-event and cooldown guards, rolls a 1-100 chance, and calls `Event.Fire()` for a selected event.
5. On a month, `AutoWorldEvent()` finds events whose `Date.Year` and `Date.Month` match the current game date and fires one. `ignoreWorldChance` is currently `true` by default, so the selected world event's chance is ignored.
6. `Event.Fire()` attaches each option to its parent, checks conditions unless explicitly bypassed, and creates an `EventWindow`.
7. `EventWindow` generates random effect values, substitutes `{NAME}` and numbered `{0}`, `{1}`, ... placeholders in descriptions, and builds one-to-four option buttons. World events skip the popup, write a newspaper story, and select option zero.
8. `EventOption.Select()` fires effects in order, expands notification placeholders (`{NAME}`, `{0}`, and `${0}` for currency formatting), displays the notification, closes the window, clears `hasEventRunning`, and starts the cooldown.
9. `OnDeactivate()` clears definitions, removes console commands, and unsubscribes handlers. Any new event subscription must also be removed there.

## Event definition format

`.tyd` files are parsed by `EventsHandlerV2.LoadTyd()`. Each top-level `Event` requires `Title`, `Description`, `Chance`, `Conditions[]`, and `Options[]`. Each option normally contains:

```text
{
	ButtonText "..."
	Title "..."
	Description "..."
	Notification "..."
	NotificationSound Good # Good, Issue, Warning, or Neutral
	Effects[
		{
			IsVisible 1
			EffectName "Human-readable label"
			Effect "company_money"
			EffectValue "1000"
			# Optional range instead of a fixed value:
			# EffectMinValue -10
			# EffectMaxValue 20
		}
	]
}
```

World events additionally require `Date "month.year"`. The game uses zero-based months, and the historical year is represented in the existing data convention (for example `0.80` is January 1980 and `11.80` is December 1980). Keep the existing convention when adding dates.

Important authoring behavior:

- `EffectValue` is stored as text and parsed by the selected effect handler. Use numeric text for numeric effects.
- If `EffectMaxValue != 0`, the UI generates an integer random value in the `[EffectMinValue, EffectMaxValue)` Unity range before display and selection.
- Effect values are generated for **all** options when the popup opens, not only the selected option.
- `IsVisible` only controls the tooltip summary; it does not disable the effect.
- The UI has explicit layouts for one, two, three, and four options. Do not add a fifth option without updating `EventWindow`.
- Event titles are used by `call_event`; keep titles unique if events are chained.
- Empty or malformed required Tyd nodes can throw during loading. Validate new definitions against nearby examples.
- This project uses the Software Inc. fork of TyDSharp (`khornel/TyDSharp`), not a permissive generic parser. In a list such as `Effects[ ... ]`, each effect table must use multiline fields (or semicolon-separated fields). Do not write an inline table with whitespace-separated fields like `{ IsVisible 1 EffectValue 8 EffectName "..." Effect "..." }`; TyDSharp parses that as one string value, so `LoadTyd()` later sees missing `EffectName`/`Effect` nodes and can throw `NullReferenceException`.
- After changing `.tyd` files, validate them with TyDSharp when possible and inspect the parsed node structure, not only bracket balance. Every event must have `Title`, `Description`, `Date` for world events, `Conditions`, `Options`, and `Chance`; every option must have `ButtonText`, `Title`, `Description`, `Notification`, `NotificationSound`, and `Effects`; every effect must have `IsVisible`, `EffectName`, and `Effect`.
- For market effects, `stockmarket_loss` expects a positive percentage because `MarketEffects` passes `subtract = true` to `Utils.GetPercentage`; negative values would increase the market. `stockmarket_win` also uses positive percentages.

## Conditions DSL

A condition is a string such as `has_game&is_sequel` or `has_money>10000`. `ConditionExpression` recognizes operators in this order: `<=`, `>=`, `!=`, `<`, `>`, `=`, `!`. A `!` suffix means false/not for supported boolean conditions; comparison conditions use the numeric value after the operator.

The first segment is the **main condition**. Additional segments are subconditions separated by a single `&` (not `&&`), and each subcondition filters the candidate objects returned by the main condition. The selected candidate becomes `Event.EventObject`, which supplies `{NAME}` and is used by object-targeted effects.

Main condition families currently implemented include:

- Products: `has_software`, `has_game` and `has_game{rpg|adventure|simulation|sports|rts|fps}`, `has_os` and `has_os{computer|console|phone}`, `has_2deditor`, `has_3deditor`, `has_audiotool`, `has_office`, `has_antivirus`.
- Employees/objects: `has_staff`, `has_employee`, `has_leader`, `has_designer`, `has_programmer`, `has_artist`, `has_service`, `has_stockmarket`, `has_supportwork`, `has_activedeal`, `has_softwareworkitem`, `has_softwarealpha`, `has_designdocument`.
- Scalar/state checks: `has_founder_at_work`, `has_multiple_founders`, `player_has_car`, `is_year`, `is_month`, `has_fans`, `has_money`, `has_snow`, `has_stars`, `has_rain`, and `has_popularity...`.

Subconditions currently implemented include `has_sequel`, `is_sequel`, `is_alpha`, `is_beta`, `from_playercompany`, `has_addon`, `release_date`, `has_bugs`, `has_offsales`, `has_onsales`, `sick_days`, `is_female`, `is_working`, `is_founder`, `is_crunching`, `is_stressed`, `is_satisfied`, and `salary`. `years_in_company` currently returns no candidates and must not be treated as working support.

When adding a condition, update the appropriate evaluator and `src\content.txt`; preserve the candidate type because effects depend on `EventObject` being an `Actor`, `SoftwareProduct`, `SoftwareAlpha`, `DesignDocument`, or `StockMarket`.

## Effect vocabulary and dispatch

`EffectDispatcher` tries handlers in this order: company, product, employee, market, then miscellaneous. A handler returning `true` means the effect is considered handled.

- Company: `company_money`, `company_bill`, `company_deals`, `pay_debt`, `change_businessrep`.
- Products/market categories: `software_offsales`, `software_onsales`, `software_refunds`, `change_bugs`, `add_marketing`, `fans_change` and its category variants, `popularity_change_...`.
- Employees: `add_exp`, role-specific `add_leaderexp`, `add_designerexp`, `add_programmerexp`, `add_artistexp`, `add_serviceexp`, `change_employee_satisfaction`, `change_employee_stress`, `goes_home`, `fire_employee`, `quit_employee`, `change_salary`.
- Stock markets: `stockmarket_loss`, `stockmarket_win`, `stockmarket_singleloss`, `stockmarket_singlewin`.
- Miscellaneous: `call_event` looks up a random event by exact title.

Object-targeted effects silently have no target in some handlers, so pair them with a main condition that returns the required object. Before introducing a new effect name, search all effect handlers and add it to `src\content.txt`.

## Console/debugging

The mod registers these developer console commands while active:

- `CALL_RANDOM_EVENT <index>` and `RANDOM_EVENT_LIST`
- `CALL_WORLD_EVENT <index>` and `WORLD_EVENT_LIST`
- `EVENT_DEBUG Y/N`

The debug flag logs scheduling, condition checks, and failed event attempts. Console indices are zero-based and are the current loaded-list order. `CALL_WORLD_EVENT` passes `IgnoreConditions = true`; normal scheduling still evaluates conditions through `Event.Fire()`.

## Build and deployment

Build from Visual Studio using `src\RandomEvents.sln` or `src\RandomEvents.csproj`. The project targets .NET Framework 4.8 and references the game's managed Unity and `Assembly-CSharp` DLLs through the relative Steam path configured in the project file:

`..\..\..\Apps\Steam\steamapps\common\Software Inc\Software Inc_Data\Managed\`

If those references are missing, configure the paths for the local installation rather than committing game binaries or changing source to accommodate one machine. There are no automated tests in the repository; the minimum validation for code changes is a successful project build followed by an in-game smoke test when possible.

For manual deployment, copy the built mod files/DLL and the required `.tyd`/metadata files into the game's `DLLMods\RandomEvents` directory as described by `README.md`. Do not edit generated `bin\` or `obj\` files as a substitute for source changes.

## Change guidance

- Keep game API calls compatible with the referenced `Assembly-CSharp.dll`; game updates can break compilation or runtime behavior.
- Use existing Tyd parsing and Unity/game helpers instead of adding a second parser or UI system.
- Keep event handler registration and cleanup symmetrical.
- Preserve global state semantics (`hasEventRunning`, `iscoolingdown`, `EventObject`) unless the change explicitly targets them.
- For new event content, prefer `.tyd` changes; for new vocabulary or behavior, update the corresponding evaluator/dispatcher and `src\content.txt` together.
- Avoid broad exception swallowing. `LoadTyd()` already has a fallback from the local mod directory to the Steam Workshop directory; preserve useful warning output when changing loading behavior.
