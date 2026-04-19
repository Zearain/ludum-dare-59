namespace LudumDare59.Data;

using Godot;

public sealed class HazardSpawnData
{
    public string HazardType { get; set; } = string.Empty;

    public Vector2 Position { get; set; }

    public float Radius { get; set; }

    public float DamagePerSecond { get; set; }

    public float ActiveDurationSeconds { get; set; }

    public float CooldownDurationSeconds { get; set; }

    public Vector2 DriftOffset { get; set; }

    public float DriftSpeed { get; set; }
}
