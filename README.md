# Seven Seats; CS 417 Open Project Fall 2026 Team 6

## Contributors and their contributions
- Alyssa: 
- Dylan: 
- Jinwon: 
- Osmond: 
- Rishi: 
- Sharyq: 

## Basic rules
1. Make sure you put all your files in `Assets/_<name>/`.
2. Any files that we all need to use go in `Assets/Shared/`. Give the group a heads up before changing anything in there.
3. Don't edit `Assets/Scenes/MainScene.unity` without first posting about it on Discord
4. Make and work on your own scene in your folder.
5. Work on your own branch. Make PRs, always have them approved by whoever is online and available.
6. Always commit `.meta` files along with their assets, or references to those assets break for everyone else.
7. Wrap your scripts in `namespace <name>` so two people can both have classes with the same name. For example, I would put my scripts in `namespace Rishi { }`.
8. Create an empty object at the top of your scene hierarchy that is the parent of everything in your area (e.g. `RishiSceneRoot`). Spawn stuff under and relative to it, so when we merge into one scene we don't have a mess.
10. Resource names are case sensitive strings, so let's use `Spaced Pascal Case` singular nouns for consistency (e.g. `Dubloon`, `Rice`, `Rock Salt`), and mention in Discord when you add a new resource.

## Shared systems (`Assets/Shared`)
- `ResourceManager`: one per scene. Resource names are case sensitive strings. Has `Add`, `Get`, `CanAfford`, `TrySpend`, and an `OnResourceChanged` event (subscribe in `Start`, not `Awake` or `OnEnable`).
- `AchievementManager`: [Sharyq can write a brief about it]
Feel free to propose and then implement other shared systems

```csharp
using Shared;

ResourceManager.Instance.Add("Salmon", 1);
if (ResourceManager.Instance.TrySpend("Salmon", 1)) {
  // put salmon on chopping board
}
```
