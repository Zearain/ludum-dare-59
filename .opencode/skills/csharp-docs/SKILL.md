---
name: csharp-docs
description: XML documentation conventions for C# public APIs -- summary, remarks, param, returns, typeparam, value, and exception tags, including guidance for Godot exported properties and signal delegates.
---

# C# Documentation Best Practices

- Public members should be documented with XML comments.
- It is encouraged to document internal members as well, especially if they are complex or not self-explanatory.
- Godot `[Export]` properties and `[Signal]` delegates should always have `<summary>` tags -- they are part of the public surface visible in the editor.

## Guidance for All APIs

- Use `<summary>` to provide a brief, one-sentence description of what the type or member does. Start with a present-tense, third-person verb.
- Use `<remarks>` for additional information: implementation details, usage notes, or relevant context.
- Use `<see langword>` for language-specific keywords like `null`, `true`, `false`, `int`, `bool`, etc.
- Use `<c>` for inline code snippets.
- Use `<example>` for usage examples.
  - Use `<code>` for code blocks inside `<example>`. Add the `language` attribute: `<code language="csharp">`.
- Use `<see cref>` to reference other types or members inline (within a sentence).
- Use `<seealso>` for standalone references to other types or members in a "See also" section.
- Use `<inheritdoc/>` to inherit documentation from base classes or interfaces, unless there is a meaningful behavior change.

## Methods

- Use `<param>` to describe method parameters.
  - The description should be a noun phrase that does not specify the data type.
  - Begin with an introductory article ("The", "A", "An").
  - For a flag enum: "A bitwise combination of the enumeration values that specifies...".
  - For a non-flag enum: "One of the enumeration values that specifies...".
  - For a Boolean: "`<see langword="true" />` to ...; otherwise, `<see langword="false" />`."
  - For an `out` parameter: "When this method returns, contains .... This parameter is treated as uninitialized."
- Use `<paramref>` to reference parameter names in documentation.
- Use `<typeparam>` to describe type parameters in generic types or methods.
- Use `<typeparamref>` to reference type parameters in documentation.
- Use `<returns>` to describe what the method returns.
  - The description should be a noun phrase that does not specify the data type.
  - Begin with an introductory article.
  - For a Boolean return type: "`<see langword="true" />` if ...; otherwise, `<see langword="false" />`."

## Constructors

- The `<summary>` should read: "Initializes a new instance of the `<ClassName>` class [or struct]."

## Properties

- The `<summary>` should start with:
  - "Gets or sets..." for a read-write property.
  - "Gets..." for a read-only property.
  - "Gets [or sets] a value that indicates whether..." for Boolean properties.
- Use `<value>` to describe the value of the property.
  - The description should be a noun phrase without a data type.
  - If the property has a default value, include it: "The default is `<see langword="false" />`."
  - For Boolean: "`<see langword="true" />` if ...; otherwise, `<see langword="false" />`. The default is ...".

## Exceptions

- Use `<exception cref>` to document exceptions thrown by constructors, properties, indexers, methods, operators, and events.
- Document all exceptions thrown directly by the member.
- For exceptions thrown by nested members, document only the ones users are most likely to encounter.
- The description states the condition under which the exception is thrown -- omit "Thrown if..." at the start.

## Godot-Specific Documentation

### Exported Properties

```csharp
/// <summary>
/// Gets or sets the movement speed in units per second.
/// </summary>
/// <value>The movement speed. The default is <c>5.0</c>.</value>
[Export]
private float _speed = 5.0f;

/// <summary>
/// Gets or sets the maximum health of this character.
/// </summary>
[ExportGroup("Combat")]
[Export]
private int _maxHealth = 100;
```

### Signal Delegates

```csharp
/// <summary>
/// Occurs when the character's health changes.
/// </summary>
/// <param name="current">The current health value.</param>
/// <param name="maximum">The maximum health value.</param>
[Signal]
public delegate void HealthChangedEventHandler(int current, int maximum);

/// <summary>
/// Occurs when the character dies.
/// </summary>
[Signal]
public delegate void DiedEventHandler();
```

### Node Scripts

```csharp
/// <summary>
/// Controls player movement, input handling, and health management.
/// </summary>
/// <remarks>
/// Requires a <see cref="Godot.CollisionShape3D"/> child node named "CollisionShape3D".
/// Attach to a <see cref="Godot.CharacterBody3D"/> node.
/// </remarks>
public partial class Player : CharacterBody3D
{
    /// <summary>
    /// Gets the current health of the player.
    /// </summary>
    /// <value>The current health. Always between <c>0</c> and <see cref="MaxHealth"/>.</value>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Applies damage to the player, reducing current health.
    /// </summary>
    /// <param name="amount">The amount of damage to apply. Must be greater than zero.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is less than or equal to zero.
    /// </exception>
    public void TakeDamage(int amount)
    {
        // ...
    }
}
```
