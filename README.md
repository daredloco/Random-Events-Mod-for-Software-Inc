# Random Events Mod for Software Inc

Random Events adds random and real-world events to **Software Inc.** Events can affect company finances, products, employees, fans, reputation, and the stock market.

> This mod targets Software Inc. BETA 1 and its modding API.

## Download

Install the mod from the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2139500907), or follow the manual installation instructions below.

The repository also includes the [supported effects and conditions](content.txt).

## Features

- Random events that occur during gameplay.
- Historical world events from 1970–2020.
- Configurable event modes:
  - **Random Events**
  - **World Events**
  - **World and Random Events**
- Optional suppression of random events while fast-forwarding.
- Developer console commands for testing events.

## Manual installation

1. Clone or download this repository.
2. Copy the contents of the `src` folder to the game's `DLLMods\RandomEvents` folder:
   - `Conditions\`
   - `Effects\`
   - `Models\`
   - `UI\`
   - `EventsHandlerV2.cs`
   - `Helpers.cs`
   - `MyModMeta.cs`
   - `Utils.cs`
   - `meta.tyd`
   - `random_events.tyd`
   - `world_events.tyd`
3. Enable **Random Events** in the game's mod options.

If you build the project in Visual Studio, copy the resulting `RandomEvents.dll` together with `meta.tyd`, `random_events.tyd`, and `world_events.tyd` instead of deploying the C# source files.

## Console commands

The mod registers these commands while it is active:

- `RANDOM_EVENT_LIST` and `CALL_RANDOM_EVENT <index>`
- `WORLD_EVENT_LIST` and `CALL_WORLD_EVENT <index>`
- `EVENT_DEBUG Y/N`

Event indexes are zero-based and follow the current loaded list order.

## Development

Open `src\RandomEvents.sln` in Visual Studio to build the mod. The project targets .NET Framework 4.8 and references assemblies from the local Software Inc. installation.
