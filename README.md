<div align="center">

# Spriteman

**A Stardew Valley 1.6+ mod for players who want new crops, artisan processing, paper making, cigarette crafting, a modern vape crafting line, and a custom smoking action.**

<img src="assets/crops.png" width="512" alt="Betel nut crop growth stages" />

<img src="assets/objects.png" width="512" alt="Spriteman seeds, produce, artisan goods, paper, cigarette, and vape sprites" />

![Content Pack](https://img.shields.io/badge/content_pack-v1.3.0-4f8a4c)
![Smoking Mod](https://img.shields.io/badge/smoking_mod-v1.1.0-b55d4c)
![Stardew Valley](https://img.shields.io/badge/Stardew_Valley-1.6%2B-6b8e23)
![SMAPI](https://img.shields.io/badge/SMAPI-4.0.0%2B-d97b29)
![License](https://img.shields.io/badge/license-Apache--2.0-blue)

</div>

Spriteman adds a complete farm-to-craft loop: grow betel nut and tobacco, use a Dehydrator to make premium goods, press Fiber and Sap into Paper, roll Cigarettes, assemble Vape Juice Pods and E-Cigarettes, and smoke Cigarettes through a dedicated SMAPI-powered action.

## Features

- Two seed-shop crops with custom growth sprites: renewable betel nut and scythe-harvested tobacco.
- Dehydrator recipes for Premium Betel Nut and Dried Tobacco.
- A craftable Paper Press that turns Fiber and Sap into Paper.
- A Paper + Dried Tobacco cigarette recipe.
- Vape Juice Pod and E-Cigarette recipes that use native Truffle Oil and Battery Packs.
- A custom smoking animation with sound, smoke puffs, energy recovery, a health cost, and a temporary buff for Cigarettes and E-Cigarettes.
- English and Simplified Chinese localization files.

## Requirements

| Dependency | Version | Purpose |
| --- | --- | --- |
| Stardew Valley | 1.6+ | Base game targeted by the content pack |
| SMAPI | 4.0.0+ | Loads the smoking code mod |
| Content Patcher | No minimum version declared | Loads the crops, objects, recipes, shops, and machines |
| .NET SDK | 6.0 | Needed only when building the smoking module from source |

## Installation

The repository contains the content-pack source and smoking-module source. To install the current code directly:

1. Clone the repository and enter it.

   ```bash
   git clone https://github.com/JuhaoChen666/Spriteman.git
   cd Spriteman
   ```

2. Build the smoking module, overriding `StardewValleyPath` with the folder that contains the game DLLs.

   ```bash
   dotnet build smapi/Spriteman.Smoking/Spriteman.Smoking.csproj -c Release -p:StardewValleyPath="<path-to-Stardew-Valley>"
   ```

3. In the game's `Mods` directory, create `[CP] Spriteman` and copy these repository items into it: `manifest.json`, `content.json`, `assets/`, and `i18n/`.

4. Create `Spriteman Smoking` beside it. Copy `smapi/Spriteman.Smoking/manifest.json` and `smapi/Spriteman.Smoking/bin/Release/net6.0/Spriteman.Smoking.dll` into that folder, then launch Stardew Valley through SMAPI.

The two installed folders should look like this:

```text
Mods/
|-- [CP] Spriteman/
|   |-- assets/
|   |-- i18n/
|   |-- content.json
|   `-- manifest.json
`-- Spriteman Smoking/
    |-- manifest.json
    `-- Spriteman.Smoking.dll
```

## Quick Start

1. Buy Betel Nut Saplings or Tobacco Seeds from the Seed Shop for `80g` each.
2. Grow and harvest the crop, then process the harvest in a Dehydrator if the recipe calls for it.
3. Craft a Paper Press, make Paper, and combine Paper with Dried Tobacco to craft a Cigarette.
4. Hold the Cigarette and press the action button to smoke it.

## Crops

| Crop | Seasons | Growth | Harvest | Regrowth | Seed price |
| --- | --- | --- | --- | --- | ---: |
| Betel Nut | Spring, Summer, Fall, Winter | 15 days (`3 / 4 / 4 / 4 / 0` phases) | Grab; 1-2 nuts, with configured bonus-harvest chances | 3 days | `80g` |
| Tobacco | Spring, Summer, Fall | 11 days (`3 / 4 / 4 / 0` phases) | Scythe; 2-4 Fresh Tobacco | None | `80g` |

## Processing and Recipes

### Artisan Processing

| Machine | Input | Output | Time |
| --- | --- | --- | --- |
| Dehydrator | 5 Betel Nuts | 1 Premium Betel Nut | 5 days |
| Dehydrator | 3 Fresh Tobacco | 1 Dried Tobacco | 2 days |
| Paper Press | 5 Fiber + 1 Sap | 5 Paper | 1 day |

### Crafting

| Recipe | Ingredients | Output |
| --- | --- | --- |
| Paper Press | 50 Wood (`388`) + 2 Iron Bars (`335`) + 20 Fiber (`771`) | 1 Paper Press |
| Cigarette | 1 Paper + 1 Dried Tobacco | 1 Cigarette |
| Vape Juice Pod | 1 Fresh Tobacco + 1 Truffle Oil (`432`) | 1 Vape Juice Pod |
| E-Cigarette | 1 Vape Juice Pod + 1 Battery Pack (`787`) | 1 E-Cigarette |

All crafting recipes in the table are registered as default recipes by the content pack.

## Items and Effects

| Item | Sell price | Configured gameplay effect |
| --- | ---: | --- |
| Betel Nut | `120g` | Edibility `15`; `+1 Speed` buff with duration value `180` |
| Premium Betel Nut | `1,440g` | Edibility `90`; `+6 Speed` buff with duration value `180` |
| Fresh Tobacco | `50g` | Not edible; processed in a Dehydrator |
| Dried Tobacco | `200g` | Not edible; used to craft Cigarettes |
| Paper | `20g` | Not edible; used to craft Cigarettes |
| Cigarette | `250g` | Consumed by the custom smoking action |
| Vape Juice Pod | `350g` | Not edible; used to assemble E-Cigarettes |
| E-Cigarette | `650g` | Not edible; load a Vape Juice Pod for 20 uses |

### Smoking a Cigarette

With a Cigarette selected, press the normal action button while the player is free. The smoking module then:

1. Consumes one Cigarette, stops movement, faces the farmer downward, and plays an eight-frame animation with a furnace sound.
2. Spawns smoke puffs during the `1,080 ms` action.
3. Removes `5 Health` without reducing health below `1`.
4. Restores `50 Energy` without exceeding maximum stamina.
5. Applies **Nicotine Rush** for `150 seconds`, granting `+2 Mining` and `+2 Speed`.

### Vaping an E-Cigarette

With an E-Cigarette selected, press the normal action button while the player is free. If the rod has no loaded pod, the mod consumes one Vape Juice Pod from the inventory and stores `20` uses in the item's persistent mod data. Press the action button again to vape. Each use replays the same animation and smoke puffs as a Cigarette, costs `2 Health`, restores `50 Energy`, and applies **Light Vape Rush** for `150 seconds`, granting `+1 Mining` and `+1 Speed`. The E-Cigarette rod remains in the inventory when the pod reaches zero uses and can be refilled with another pod.

## Project Structure

```text
Spriteman/
|-- assets/                         # Crop, object, tobacco, and machine sprite sheets
|-- i18n/                           # English and Simplified Chinese text
|-- smapi/Spriteman.Smoking/
|   |-- ModEntry.cs                 # Cigarette and refillable E-Cigarette interactions
|   |-- i18n/                       # SMAPI HUD messages for loading and using pods
|   |-- Spriteman.Smoking.csproj    # .NET 6 build configuration and game references
|   `-- manifest.json               # SMAPI module metadata and dependency declaration
|-- content.json                    # Content Patcher objects, crops, shops, recipes, and machines
|-- manifest.json                   # Content pack metadata
`-- LICENSE                         # Apache License 2.0
```

## Development and Testing

Build the code module with the Stardew Valley install path supplied explicitly:

```bash
dotnet build smapi/Spriteman.Smoking/Spriteman.Smoking.csproj -c Release -p:StardewValleyPath="<path-to-Stardew-Valley>"
```

There is currently no automated test project or checked-in CI workflow. Changes should be tested manually through SMAPI:

- Confirm both manifests load without SMAPI or Content Patcher errors.
- Check that both seed types appear in the Seed Shop at `80g`.
- Verify crop seasons, growth, harvest method, yield, and betel regrowth.
- Run each Dehydrator and Paper Press recipe to completion.
- Craft a Cigarette, Vape Juice Pod, and E-Cigarette, verifying native Truffle Oil and Battery Pack inputs are consumed correctly.
- Use a Cigarette with the action button and verify item consumption, animation, health, energy, and buff behavior.
- Select an empty E-Cigarette, press the action button to load a pod, then press it again to vape; verify the pod starts at 20 uses, decreases once per animation, and the rod remains after the twentieth use.

When changing player-facing content, keep `i18n/default.json` and `i18n/zh.json` aligned with the values in `content.json`.

## License

Spriteman is licensed under the [Apache License 2.0](LICENSE).
