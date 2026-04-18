namespace LudumDare59.Interfaces;

using Godot;

public interface IDamageSource
{
    Vector2 DamageOrigin { get; }

    float DamagePerSecond { get; }

    float DamageRadius { get; }
}
