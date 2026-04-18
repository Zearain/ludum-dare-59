# Testing Guidelines

## Framework

GdUnit4 v5 (`gdUnit4.api 5.0.0`) via `dotnet test`. Tests only compile in the `Debug` configuration (see `.csproj`).

## Test Location

Mirror the source structure under `Tests/`:

```
Tests/
├── AsteroidField/   → AsteroidField/
├── Combat/          → Combat/
├── Components/      → Components/
└── Core/            → Core/
```

## Test Naming

Three-part underscore convention: `MethodName_Condition_ExpectedOutcome`

```csharp
public void Activate_WhenReady_EmitsSignalAndStartsTimer() { }
public void TakeDamage_WhenShieldAbsorbs_HullRemainsUnchanged() { }
public void TransitionTo_InvalidState_ThrowsInvalidOperationException() { }
```

## Pure C# Tests (no scene tree)

Use when the class under test is a plain C# object with no Godot node lifecycle.

```csharp
using GdUnit4;
using static GdUnit4.Assertions;

namespace VoidlineProspector.Tests.Combat;

[TestSuite]
public class DamageCalculatorTest
{
    [TestCase]
    public void Calculate_ArmorAbsorbsFlat_ReducesDamageCorrectly()
    {
        var result = DamageCalculator.Calculate(new DamageInfo(100, DamageType.Kinetic), armor: 20);
        AssertThat(result).IsEqual(80);
    }
}
```

## Godot Node Tests (requires scene tree)

Add `[RequireGodotRuntime]` when the class under test calls `_Ready`, emits signals, or otherwise needs the Godot engine.

```csharp
[TestSuite]
public class BoostComponentTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void Activate_WhenReady_SetsIsBoosting()
    {
        // 1. Allocate — AutoFree ensures cleanup even on test failure
        var comp = AutoFree(new BoostComponent());

        // 2. Inject private fields via Godot reflection (avoids exposing internals)
        comp.Set("_cooldown", 5.0f);
        comp.Set("_duration", 1.0f);

        // 3. Add to tree to trigger _Ready()
        AddNode(comp);

        // 4. Act
        comp.Activate();

        // 5. Assert
        AssertThat(comp.IsBoosting).IsTrue();
    }
}
```

## Key Assertion Patterns

```csharp
// Equality
AssertThat(value).IsEqual(42);
AssertThat(flag).IsTrue();
AssertThat(flag).IsFalse();
AssertThat(obj).IsNull();
AssertThat(obj).IsNotNull();

// Floating point (use tolerance)
AssertThat(result).IsEqualApprox(expected, 0.001f);

// Exceptions
AssertThrown(() => sm.TransitionTo(State.Invalid))
    .IsInstanceOf<InvalidOperationException>();
```

## Signal Testing

Subscribe with a lambda before the act, check the flag after.

```csharp
[TestCase]
[RequireGodotRuntime]
public void Activate_WhenReady_EmitsBoostActivated()
{
    var comp = AutoFree(new BoostComponent());
    AddNode(comp);

    var fired = false;
    comp.BoostActivated += () => fired = true;

    comp.Activate();

    AssertThat(fired).IsTrue();
}
```

## Private Field Injection

Use `node.Set("_fieldName", value)` to configure private Godot-serialised fields without exposing them as public API. This also works for `[Export]` references that would normally be set in the inspector.

```csharp
comp.Set("_speedMultiplier", 3.0f);
comp.Set("_cooldown", 0.0f);  // pre-expire cooldown so Activate() proceeds
```
