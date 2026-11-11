# Resource System

I'd like to propose a replacement of the string-based ResourceManager with this asset-based version. Let me know what you think.

- `Resource.cs` stores a resource's quantity, limits, optional prefab, and change event.
- `ResourceManager2.cs` provides extension methods. No manager GameObject is needed.

## Setup

1. In the Project window, right-click > **Create > Scriptable Objects > Resource**.
2. Name the asset `Salmon`, `Rice`, etc., following the project's folder and naming rules.
3. Set **Initial Value**, **Min Value**, and **Max Value** in the Inspector. For example: `10`, `0`, `100`. Keep Min <= Max.
4. Optionally drag an existing prefab into **Prefab**, or leave it as None.
5. Drag the resource asset into your script's Resource field. Everyone sharing a quantity must reference the same asset.

Inspector settings override the script's field defaults. `Salmon.asset` and `Rice.asset` are included as examples.

## Usage

Import `Shared`, and put your script in your contributor namespace (e.g. `Dylan`). Declare and assign these fields:

```csharp
[SerializeField] private Resource salmon;
[SerializeField] private Resource rice;
```

Inside your methods:

```csharp
float amount = salmon.Value;
salmon.Add(3f);
if (salmon.TrySpend(1f)) Debug.Log("Cooked a salmon.");
```

For a recipe, also import `System.Collections.Generic`:

```csharp
var costs = new Dictionary<Resource, float>
{
    { salmon, 1f },
    { rice, 2f }
};

bool canCook = costs.CanAfford();
if (costs.TrySpend()) Debug.Log("Cooked salmon with rice.");
```

All value changes are clamped to Min/Max. `TrySpend` checks first and leaves quantities unchanged if a cost cannot be afforded. Spending cannot go below Min; zero or negative costs do nothing and succeed. Batch spending checks all costs before deducting them; event listeners should not change those resources during deduction.

For UI updates, subscribe to the resource's event and unsubscribe when disabled:

```csharp
private void OnEnable()
{
    salmon.OnResourceChanged += Refresh;
    Refresh(salmon.Value);
}

private void OnDisable() => salmon.OnResourceChanged -= Refresh;
private void Refresh(float value) => Debug.Log($"Salmon: {value}");
```

The optional prefab is available as `salmon.Prefab`. Check for null before spawning it; spawning does not change the quantity.

## Old to new API

Here, `old` means `ResourceManager.Instance`.

| Old                                 | New                                           |
| ----------------------------------- | --------------------------------------------- |
| `old.Get("Salmon")`                 | `salmon.Value`                                |
| `old.Add("Salmon", 3)`              | `salmon.Add(3f)`                              |
| `old.CanAfford("Salmon", 2)`        | `salmon.CanAfford(2f)`                        |
| `old.TrySpend("Salmon", 2)`         | `salmon.TrySpend(2f)`                         |
| `old.CanAfford(costs)`              | `costs.CanAfford()`                           |
| `old.TrySpend(costs)`               | `costs.TrySpend()`                            |
| `Dictionary<string, int>` costs     | `Dictionary<Resource, float>` costs           |
| Global event: name, quantity, delta | Per-resource event: new quantity only         |
| `old.All`                           | No equivalent; keep a resource list if needed |

## Why change?

- Asset references are more intuitive than resource-name strings.
- Initial settings and prefab references are easy to change in the Inspector.
- The same resource assets can be referenced across scenes, without a manager GameObject.

The system currently uses `float` in case some resource attributes need floating-point calculations. We can switch back to `int` if only whole numbers are needed.
