---
name: csharp-async
description: C# async/await best practices covering Task/ValueTask return types, cancellation tokens, parallel execution, common pitfalls, and Godot-specific async patterns including ToSignal, SceneTreeTimer, and async void signal handlers.
---

# C# Async Programming Best Practices

Your goal is to help follow best practices for asynchronous programming in C#.

## Naming Conventions

- Use the `Async` suffix for all async methods.
- Match method names with their synchronous counterparts when applicable (e.g., `GetDataAsync()` for `GetData()`).

## Return Types

- Return `Task<T>` when the method returns a value.
- Return `Task` when the method does not return a value.
- Consider `ValueTask<T>` for high-performance scenarios to reduce allocations.
- Avoid returning `void` for async methods **except** for event handlers and Godot signal callbacks (see Godot Notes below).

## Exception Handling

- Use try/catch blocks around await expressions.
- Avoid swallowing exceptions in async methods.
- Propagate exceptions with `Task.FromException()` instead of throwing in async Task-returning methods when building TAP APIs.

## Performance

- Use `Task.WhenAll()` for parallel execution of multiple independent tasks.
- Use `Task.WhenAny()` for implementing timeouts or taking the first completed task.
- Avoid unnecessary async/await when simply passing through a task result -- return the Task directly.
- Use `CancellationToken` for long-running operations and pass it through to all awaitable calls.

## Common Pitfalls

- Never use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()` in async code -- they block the thread and can deadlock.
- Avoid mixing blocking and async code.
- Always await Task-returning methods; ignoring the returned Task silently swallows exceptions.

## Implementation Patterns

- Use async streams (`IAsyncEnumerable<T>`) for processing sequences asynchronously.
- Use the task-based asynchronous pattern (TAP) for public APIs.

```csharp
// Parallel execution
var results = await Task.WhenAll(FetchUserAsync(id), FetchInventoryAsync(id));

// Timeout with WhenAny
var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
var workTask = DoWorkAsync(token);
if (await Task.WhenAny(workTask, timeoutTask) == timeoutTask)
{
    throw new TimeoutException("Operation timed out");
}

// CancellationToken propagation
public async Task<Data> LoadAsync(CancellationToken cancellationToken = default)
{
    var raw = await FetchAsync(cancellationToken);
    return await ParseAsync(raw, cancellationToken);
}
```

---

## Godot-Specific Notes

### `ToSignal()` is the primary async tool in Godot

`await ToSignal()` is idiomatic Godot async. Prefer it over custom `TaskCompletionSource` patterns for signal-based waiting:

```csharp
// Wait for a timer
await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);

// Wait for an animation to finish
await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

// Wait for a one-frame yield
await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

// Wait for a custom signal
await ToSignal(someNode, Enemy.SignalName.Died);
```

### `async void` is acceptable for Godot signal handlers

The general C# rule against `async void` has an exception in Godot: signal callbacks and Godot lifecycle overrides that need to do async work must be `async void` because the engine calls them without awaiting a Task.

```csharp
// Acceptable: async void signal handler
private async void OnButtonPressed()
{
    await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
    _button.Disabled = false;
}

// Acceptable: async void for a Godot lifecycle method
public override async void _Ready()
{
    await ResourceLoader.LoadThreadedRequestAsync("res://big_scene.tscn");
    _isReady = true;
}
```

Wrap the body in try/catch when using `async void` to avoid unhandled exceptions:

```csharp
private async void OnInteracted()
{
    try
    {
        await PlayCutsceneAsync();
    }
    catch (Exception e)
    {
        GD.PushError($"Cutscene failed: {e.Message}");
    }
}
```

### `ConfigureAwait(false)` is not needed in Godot

Godot's synchronization context automatically marshals continuations back to the main thread. Do not use `ConfigureAwait(false)` in Godot game code -- it is relevant only in library code without a synchronization context.

### Lightweight timeouts with `SceneTreeTimer`

Prefer `SceneTreeTimer` over `Task.Delay` for in-game timeouts, as it respects the scene tree's pause state and time scale:

```csharp
// Respects pause and Engine.TimeScale
await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);

// Task.Delay does NOT respect pause or time scale -- avoid in game logic
await Task.Delay(2000); // ❌ for game logic
```

### Async resource loading

```csharp
private async Task<PackedScene> LoadSceneAsync(string path, CancellationToken token = default)
{
    ResourceLoader.LoadThreadedRequest(path);

    while (ResourceLoader.LoadThreadedGetStatus(path) == ResourceLoader.ThreadLoadStatus.InProgress)
    {
        if (token.IsCancellationRequested)
        {
            return null!;
        }
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    return (PackedScene)ResourceLoader.LoadThreadedGet(path);
}
```
