namespace LudumDare59.Systems;

using Godot;

using LudumDare59.Data;

public sealed class SectorGenerator
{
    public SectorDefinition GenerateSector(int seed, int sectorIndex, bool isFinalSector)
    {
        RandomNumberGenerator rng = new();
        rng.Seed = (ulong)(uint)seed;

        float arenaWidth = rng.RandfRange(1500.0f, 2000.0f);
        float arenaHeight = rng.RandfRange(950.0f, 1250.0f);
        Rect2 bounds = new(-arenaWidth * 0.5f, -arenaHeight * 0.5f, arenaWidth, arenaHeight);

        float yPadding = 120.0f;
        Vector2 spawnPoint = new(bounds.Position.X + 160.0f, rng.RandfRange(bounds.Position.Y + yPadding, bounds.End.Y - yPadding));
        Vector2 objectivePoint = new(bounds.End.X - 220.0f, rng.RandfRange(bounds.Position.Y + yPadding, bounds.End.Y - yPadding));

        SectorDefinition definition = new()
        {
            SectorIndex = sectorIndex,
            Seed = seed,
            ArenaBounds = bounds,
            SpawnPoint = spawnPoint,
            ObjectivePoint = objectivePoint,
            HazardDensity = isFinalSector ? 0.78f : 0.55f,
            IsFinalSector = isFinalSector,
        };

        int hazardCount = isFinalSector ? 5 : 3;
        float corridorHalfWidth = isFinalSector ? 120.0f : 150.0f;

        for (int i = 0; i < hazardCount; i++)
        {
            Vector2 hazardPosition = Vector2.Zero;
            float radius = rng.RandfRange(70.0f, 125.0f);
            bool foundPosition = false;

            for (int attempt = 0; attempt < 50; attempt++)
            {
                hazardPosition = new Vector2(
                    rng.RandfRange(bounds.Position.X + 110.0f, bounds.End.X - 110.0f),
                    rng.RandfRange(bounds.Position.Y + 110.0f, bounds.End.Y - 110.0f));

                float distanceToRoute = DistanceToSegment(hazardPosition, spawnPoint, objectivePoint);
                if (distanceToRoute < corridorHalfWidth)
                {
                    continue;
                }

                foundPosition = true;
                break;
            }

            if (!foundPosition)
            {
                continue;
            }

            definition.HazardSpawns.Add(new HazardSpawnData
            {
                HazardType = "static_cloud",
                Position = hazardPosition,
                Radius = radius,
                DamagePerSecond = rng.RandfRange(9.0f, 14.0f),
            });
        }

        return definition;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;
        float segmentLengthSquared = segment.LengthSquared();
        if (segmentLengthSquared <= 0.001f)
        {
            return point.DistanceTo(segmentStart);
        }

        float projected = (point - segmentStart).Dot(segment) / segmentLengthSquared;
        projected = Mathf.Clamp(projected, 0.0f, 1.0f);
        Vector2 closestPoint = segmentStart + (segment * projected);
        return point.DistanceTo(closestPoint);
    }
}
