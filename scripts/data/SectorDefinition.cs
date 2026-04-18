namespace LudumDare59.Data;

using Godot;

public sealed class SectorDefinition
{
    public int SectorIndex { get; set; }

    public int Seed { get; set; }

    public Rect2 ArenaBounds { get; set; }

    public Vector2 SpawnPoint { get; set; }

    public Vector2 ObjectivePoint { get; set; }

    public float HazardDensity { get; set; }
}
