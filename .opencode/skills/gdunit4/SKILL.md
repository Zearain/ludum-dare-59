---
name: gdunit4
description: GdUnit4 v6.x testing for Godot 4.6 C# projects -- test suites, lifecycle hooks, fluent AssertThat assertions, signal assertions, Godot vector/type assertions, scene runner integration testing, parameterized tests, orphan node management, and Moq integration for mocking C# dependencies.
---

# GdUnit4 Testing for Godot 4.6 C#

GdUnit4 is the embedded unit and integration test framework for this project. It runs via `dotnet test` and integrates with the Godot editor's test inspector.

**Documentation**: https://godot-gdunit-labs.github.io/gdUnit4/latest/

## Running Tests

```bash
# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~PlayerTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~PlayerTests.TakeDamage_ReducesHealth"

# Verbose output
dotnet test -v n

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Setup

- Test project naming: `[ProjectName].Tests`
- Required `using` directives:
  ```csharp
  using GdUnit4;
  using static GdUnit4.Assertions;
  ```

## Test Suite Structure

Decorate the class with `[TestSuite]`. Test classes do not need to inherit from anything.

```csharp
using GdUnit4;
using static GdUnit4.Assertions;

namespace LudumDare59.Tests.Characters;

[TestSuite]
public class PlayerTests
{
    [Before]
    public void SetupSuite()
    {
        // Runs once before all tests in this class
        // Use for read-only shared resources
    }

    [After]
    public void TeardownSuite()
    {
        // Runs once after all tests in this class
    }

    [BeforeTest]
    public void SetupTest()
    {
        // Runs before each individual test
        // Use for mutable per-test state
    }

    [AfterTest]
    public void TeardownTest()
    {
        // Runs after each individual test
    }

    [TestCase]
    public void TakeDamage_ReducesHealth()
    {
        // Arrange
        var player = AutoFree(new Player());

        // Act
        player.TakeDamage(10);

        // Assert
        AssertThat(player.CurrentHealth).IsEqual(90);
    }
}
```

## Lifecycle Hook Execution Order

```
[Before]        → once per TestSuite
  [BeforeTest]  → before each TestCase
  [TestCase]    → the test
  [AfterTest]   → after each TestCase
[After]         → once per TestSuite
```

**Hook selection guide**:
- `[Before]` / `[After]` -- expensive shared resources that tests do not mutate (e.g., loaded scenes, configuration)
- `[BeforeTest]` / `[AfterTest]` -- fresh mutable state per test (e.g., new node instances, mocks)

## Assertions

GdUnit4 uses a fluent `AssertThat()` API that auto-selects the correct assertion type. Always prefer `AssertThat()` over the typed variants unless the typed API offers specific methods needed.

### General Assertions

```csharp
AssertThat(value).IsEqual(expected);
AssertThat(value).IsNotEqual(unexpected);
AssertThat(value).IsNull();
AssertThat(value).IsNotNull();
AssertThat(condition).IsTrue();
AssertThat(condition).IsFalse();
```

### Numeric Assertions

```csharp
AssertThat(value).IsGreater(10);
AssertThat(value).IsGreaterEqual(10);
AssertThat(value).IsLess(100);
AssertThat(value).IsLessEqual(100);
AssertThat(value).IsBetween(1, 99);

// Float with tolerance
AssertThat(1.0f).IsEqualApprox(1.001f, 0.01f);
```

### String Assertions

```csharp
AssertThat(str).IsEqual("expected");
AssertThat(str).IsNotEmpty();
AssertThat(str).Contains("substring");
AssertThat(str).StartsWith("prefix");
AssertThat(str).EndsWith("suffix");
AssertThat(str).HasLength(5);
```

### Collection Assertions

```csharp
AssertThat(list).IsNotEmpty();
AssertThat(list).HasSize(3);
AssertThat(list).Contains(item);
AssertThat(list).ContainsExactly(a, b, c);
AssertThat(list).DoesNotContain(item);
```

### Object / Type Assertions

```csharp
AssertThat(obj).IsInstanceOf<Player>();
AssertThat(obj).IsNotInstanceOf<Enemy>();
AssertThat(obj).IsSame(other);       // reference equality
AssertThat(obj).IsNotSame(other);
```

### Godot Vector Assertions

```csharp
AssertThat(vector2).IsEqual(new Vector2(1, 2));
AssertThat(vector3).IsEqualApprox(new Vector3(1, 0, 0), 0.001f);
AssertThat(vector3).IsNotEqual(Vector3.Zero);
```

### Signal Assertions

Signal assertions require the node to be in the scene tree. Use `[TestCase]` as `async Task` when awaiting signal emission.

```csharp
[TestCase]
public async Task TakeDamage_EmitsHealthChanged()
{
    var player = AutoFree(new Player());
    AddNode(player); // add to scene tree so signals work

    // Monitor the signal
    await AssertSignal(player)
        .IsEmitted(Player.SignalName.HealthChanged)
        .When(() => player.TakeDamage(10));
}

[TestCase]
public async Task Die_EmitsDied()
{
    var player = AutoFree(new Player());
    AddNode(player);

    await AssertSignal(player)
        .IsEmitted(Player.SignalName.Died)
        .When(() => player.TakeDamage(999));
}
```

### Error Assertions

```csharp
// Assert that a method throws
AssertThrown(() => dangerousMethod())
    .IsInstanceOf<ArgumentException>()
    .HasMessage("Value cannot be null");

// Async throw
await AssertThrown(async () => await asyncDangerousMethod())
    .IsInstanceOf<InvalidOperationException>();
```

## Fail-Fast Pattern

GdUnit4 continues executing assertions after a failure by default. Use `IsFailure()` to stop early when later assertions depend on earlier ones:

```csharp
[TestCase]
public void PlayerSetup_IsValid()
{
    var player = AutoFree(new Player());

    AssertThat(player).IsNotNull();
    if (IsFailure()) return;  // stop if player is null

    AssertThat(player.CurrentHealth).IsGreater(0);
    if (IsFailure()) return;

    AssertThat(player.Name).IsNotEmpty();
}
```

## Parameterized Tests

```csharp
// Inline data -- multiple [TestCase] attributes
[TestCase(10, 90)]
[TestCase(50, 50)]
[TestCase(100, 0)]
public void TakeDamage_ReducesHealthByAmount(int damage, int expectedHealth)
{
    var player = AutoFree(new Player());
    player.TakeDamage(damage);
    AssertThat(player.CurrentHealth).IsEqual(expectedHealth);
}

// Custom test names
[TestCase(0, 100, TestName = "ZeroDamage_NoChange")]
[TestCase(100, 0, TestName = "FullDamage_Dies")]
public void TakeDamage_Scenarios(int damage, int expected)
{
    var player = AutoFree(new Player());
    player.TakeDamage(damage);
    AssertThat(player.CurrentHealth).IsEqual(expected);
}

// Dynamic data via DataPoint (C# only)
public static IEnumerable<object[]> DamageTestData =>
[
    [10, 90],
    [50, 50],
    [100, 0],
];

[TestCase]
[DataPoint(nameof(DamageTestData))]
public void TakeDamage_DataDriven(int damage, int expectedHealth)
{
    var player = AutoFree(new Player());
    player.TakeDamage(damage);
    AssertThat(player.CurrentHealth).IsEqual(expectedHealth);
}
```

## Orphan Node Management

Nodes created in tests must be freed or they generate orphan warnings and can cause test pollution. Use `AutoFree()` to register a node for automatic cleanup:

```csharp
// AutoFree registers the node for cleanup after the test
var node = AutoFree(new Node3D());

// For nodes that need to be in the scene tree
var player = AutoFree(new Player());
AddNode(player);  // adds to a temporary scene tree; freed after test

// For child hierarchies, AutoFree the parent -- children are freed with it
var parent = AutoFree(new Node3D());
parent.AddChild(new MeshInstance3D());  // freed when parent is freed
```

**Never** manually call `node.Free()` after `AutoFree()` -- that will double-free.

## Scene Runner (Integration Testing)

`ISceneRunner` loads a full `.tscn` scene and simulates the Godot frame loop, input, and physics. Use this for integration tests that require the scene tree.

```csharp
[TestCase]
public async Task Player_MovesRight_WhenInputPressed()
{
    ISceneRunner runner = ISceneRunner.Load("res://Scenes/Player.tscn");

    // Get scene root and properties
    var player = runner.Scene() as Player;
    AssertThat(player).IsNotNull();

    var initialPosition = player!.GlobalPosition;

    // Simulate input action for 30 frames
    runner.SimulateActionPress("move_right");
    await runner.SimulateFrames(30);
    runner.SimulateActionRelease("move_right");

    // Assert position changed
    AssertThat(player.GlobalPosition.X).IsGreater(initialPosition.X);
}

[TestCase]
public async Task ColorCycle_ChangesColor_After10Frames()
{
    ISceneRunner runner = ISceneRunner.Load("res://Scenes/ColorCycleDemo.tscn");

    ColorRect box = runner.GetProperty<ColorRect>("_box");
    AssertThat(box.Color).IsEqual(Colors.White);

    runner.Invoke("StartColorCycle");
    await runner.SimulateFrames(10);

    AssertThat(box.Color).IsNotEqual(Colors.White);
}

[TestCase]
public async Task Menu_ClickPlay_EmitsStartGame()
{
    ISceneRunner runner = ISceneRunner.Load("res://Scenes/UI/MainMenu.tscn");
    var menu = runner.Scene();

    await AssertSignal(menu)
        .IsEmitted("start_game")
        .When(async () =>
        {
            runner.SimulateMouseButtonPress(MouseButton.Left);
            await runner.SimulateFrames(5);
        });
}
```

### Scene Runner API Reference

```csharp
ISceneRunner runner = ISceneRunner.Load("res://path/to/Scene.tscn");

// Frame simulation
await runner.SimulateFrames(60);               // run 60 frames
await runner.SimulateFrames(60, 16);           // 60 frames at 16ms delta each
runner.SetTimeFactor(5.0);                     // run at 5x speed

// Input actions (InputMap)
runner.SimulateActionPress("jump");
runner.SimulateActionRelease("jump");
await runner.SimulateKeyPress(Key.Space);
await runner.SimulateKeyRelease(Key.Space);

// Mouse input
runner.SimulateMouseMove(new Vector2(100, 200));
runner.SimulateMouseButtonPress(MouseButton.Left);
runner.SimulateMouseButtonRelease(MouseButton.Left);

// Scene access
var root = runner.Scene();
var node = runner.FindChild("HealthBar");
T prop = runner.GetProperty<T>("_propertyName");
runner.SetProperty("_propertyName", value);
runner.Invoke("MethodName", arg1, arg2);

// Signal waiting
await runner.AwaitSignal("signal_name");
await runner.AwaitSignal("signal_name", TimeSpan.FromSeconds(2));

// Window
runner.MoveWindowToForeground();
```

## Mocking with Moq

GdUnit4's native mock/spy API is GDScript-only. For C# tests, use **Moq** to mock dependencies injected into game systems.

```csharp
using Moq;

[TestSuite]
public class InventorySystemTests
{
    private Mock<IItemRepository> _mockRepo = null!;
    private InventorySystem _inventory = null!;

    [BeforeTest]
    public void Setup()
    {
        _mockRepo = new Mock<IItemRepository>();
        _inventory = new InventorySystem(_mockRepo.Object);
    }

    [TestCase]
    public void AddItem_CallsRepository()
    {
        // Arrange
        _mockRepo.Setup(r => r.Save(It.IsAny<Item>())).Returns(true);

        // Act
        _inventory.AddItem(new Item("Sword"));

        // Assert
        _mockRepo.Verify(r => r.Save(It.Is<Item>(i => i.Name == "Sword")), Times.Once);
    }

    [TestCase]
    public void GetItem_ReturnsNull_WhenNotFound()
    {
        _mockRepo.Setup(r => r.Find("missing")).Returns((Item?)null);

        var result = _inventory.GetItem("missing");

        AssertThat(result).IsNull();
    }

    [TestCase]
    public async Task SaveAsync_PropagatesException()
    {
        _mockRepo
            .Setup(r => r.SaveAsync(It.IsAny<Item>()))
            .ThrowsAsync(new InvalidOperationException("Storage full"));

        await AssertThrown(async () => await _inventory.SaveItemAsync(new Item("Shield")))
            .IsInstanceOf<InvalidOperationException>()
            .HasMessage("Storage full");
    }
}
```

### Common Moq Patterns

```csharp
// Setup return value
mock.Setup(m => m.GetValue()).Returns(42);

// Setup async return
mock.Setup(m => m.GetAsync()).ReturnsAsync(42);

// Setup with argument matching
mock.Setup(m => m.Process(It.IsAny<string>())).Returns(true);
mock.Setup(m => m.Process(It.Is<string>(s => s.Length > 5))).Returns(false);

// Setup to throw
mock.Setup(m => m.Dangerous()).Throws<InvalidOperationException>();

// Verify call count
mock.Verify(m => m.Save(It.IsAny<Item>()), Times.Once);
mock.Verify(m => m.Save(It.IsAny<Item>()), Times.Never);
mock.Verify(m => m.Save(It.IsAny<Item>()), Times.Exactly(3));

// Verify no other calls
mock.VerifyNoOtherCalls();

// Access the underlying object
_system = new MySystem(mock.Object);
```

## Skipping Tests

```csharp
// Skip with a reason
[TestCase(Ignore = "Not implemented yet")]
public void PendingFeature() { }

// Timeout override (default is 5 minutes)
[TestCase(Timeout = 2000)]
public async Task MustCompleteQuickly() { }
```

## Test Organization Conventions

- One test class per production class: `Player.cs` → `PlayerTests.cs`
- Namespace mirrors production namespace + `.Tests`: `LudumDare59.Characters` → `LudumDare59.Tests.Characters`
- Test method naming: `MethodName_Scenario_ExpectedBehavior`
- Use `[Before]` for expensive shared read-only setup; `[BeforeTest]` for mutable per-test state
- Always `AutoFree()` any nodes created in tests
- Signal tests must `AddNode()` the node under test so it has a scene tree
