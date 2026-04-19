namespace LudumDare59.Data;

using System.Collections.Generic;

using Godot;

public sealed class SectorDefinition
{
    public int SectorIndex { get; set; }

    public int Seed { get; set; }

    public Rect2 ArenaBounds { get; set; }

    public Vector2 SpawnPoint { get; set; }

    public Vector2 ObjectivePoint { get; set; }

    public float HazardDensity { get; set; }

    public float SignalRange { get; set; }

    public float NoiseStrength { get; set; }

    public float EchoStrength { get; set; }

    public int DistortionSeed { get; set; }

    public bool IsFinalSector { get; set; }

    public List<HazardSpawnData> HazardSpawns { get; } = new();
}
