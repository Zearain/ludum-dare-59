namespace LudumDare59.Systems;

using System.Collections.Generic;

using Godot;

using LudumDare59.Data;

public sealed class SectorGenerator
{
    private const float SpawnSafetyRadius = 260.0f;
    private const float ObjectiveSafetyRadius = 320.0f;

    public SectorDefinition GenerateSector(int seed, int sectorIndex, bool isFinalSector)
    {
        RandomNumberGenerator rng = new();
        rng.Seed = (ulong)(uint)seed;

        float difficulty = isFinalSector ? 6.0f : sectorIndex + 1.0f;

        float arenaWidth = rng.RandfRange(3800.0f, 4600.0f) + (difficulty * 180.0f);
        float arenaHeight = rng.RandfRange(1900.0f, 2500.0f) + (difficulty * 90.0f);
        Rect2 bounds = new(-arenaWidth * 0.5f, -arenaHeight * 0.5f, arenaWidth, arenaHeight);

        float yPadding = 180.0f;
        Vector2 spawnPoint = new(bounds.Position.X + 280.0f, rng.RandfRange(bounds.Position.Y + yPadding, bounds.End.Y - yPadding));
        Vector2 objectivePoint = new(bounds.End.X - 320.0f, rng.RandfRange(bounds.Position.Y + yPadding, bounds.End.Y - yPadding));
        Vector2 middlePointA = new(
            rng.RandfRange(-arenaWidth * 0.22f, -arenaWidth * 0.05f),
            rng.RandfRange(bounds.Position.Y + 220.0f, bounds.End.Y - 220.0f));
        Vector2 middlePointB = new(
            rng.RandfRange(arenaWidth * 0.02f, arenaWidth * 0.22f),
            rng.RandfRange(bounds.Position.Y + 220.0f, bounds.End.Y - 220.0f));

        float signalRange = Mathf.Max(
            arenaWidth * 0.7f,
            spawnPoint.DistanceTo(middlePointA) + middlePointA.DistanceTo(middlePointB) + middlePointB.DistanceTo(objectivePoint));
        float hazardDensity = Mathf.Clamp(0.42f + (difficulty * 0.08f) + (isFinalSector ? 0.08f : 0.0f), 0.45f, 0.92f);
        float noiseStrength = Mathf.Clamp(0.16f + ((difficulty - 1.0f) * 0.13f) + (isFinalSector ? 0.18f : 0.0f), 0.12f, 0.95f);
        float echoStrength = Mathf.Clamp(0.08f + ((difficulty - 2.0f) * 0.11f) + (isFinalSector ? 0.22f : 0.0f), 0.0f, 0.78f);

        SectorDefinition definition = new()
        {
            SectorIndex = sectorIndex,
            Seed = seed,
            ArenaBounds = bounds,
            SpawnPoint = spawnPoint,
            ObjectivePoint = objectivePoint,
            HazardDensity = hazardDensity,
            SignalRange = signalRange,
            NoiseStrength = noiseStrength,
            EchoStrength = echoStrength,
            DistortionSeed = seed ^ (sectorIndex * 7349),
            IsFinalSector = isFinalSector,
        };

        Vector2[] route = [spawnPoint, middlePointA, middlePointB, objectivePoint];
        float corridorHalfWidth = Mathf.Lerp(180.0f, 120.0f, Mathf.Clamp((difficulty - 1.0f) / 5.0f, 0.0f, 1.0f));
        int cloudCount = 6 + (sectorIndex * 2) + (isFinalSector ? 3 : 0);
        int movingCloudCount = Mathf.Max(0, sectorIndex) + (isFinalSector ? 2 : 0);
        int debrisCount = 6 + (sectorIndex * 2) + (isFinalSector ? 3 : 0);
        int lightningCount = 1 + Mathf.Max(0, sectorIndex - 1) + (isFinalSector ? 3 : 0);

        for (int i = 0; i < cloudCount; i++)
        {
            Vector2 cloudPosition = i % 3 == 0
                ? SampleLanePressurePosition(rng, route, corridorHalfWidth * 0.82f, bounds)
                : FindOffRoutePosition(rng, bounds, route, corridorHalfWidth * 0.6f, 120.0f, spawnPoint, objectivePoint);

            if (cloudPosition == Vector2.Zero)
            {
                continue;
            }

            float radius = rng.RandfRange(120.0f, 200.0f);
            HazardSpawnData hazard = new()
            {
                HazardType = "static_cloud",
                Position = cloudPosition,
                Radius = radius,
                DamagePerSecond = rng.RandfRange(9.0f, 15.0f) + (difficulty * 0.7f),
            };

            if (!HasHazardSpacing(definition.HazardSpawns, hazard))
            {
                continue;
            }

            definition.HazardSpawns.Add(hazard);
        }

        for (int i = 0; i < movingCloudCount; i++)
        {
            Vector2 movingCellPosition = SampleLanePressurePosition(rng, route, corridorHalfWidth * 0.7f, bounds);
            if (!IsSafeFromCriticalPoints(movingCellPosition, spawnPoint, objectivePoint, ObjectiveSafetyRadius + 60.0f))
            {
                continue;
            }

            HazardSpawnData hazard = new()
            {
                HazardType = "moving_storm_cell",
                Position = movingCellPosition,
                Radius = rng.RandfRange(110.0f, 165.0f),
                DamagePerSecond = rng.RandfRange(11.0f, 18.0f) + difficulty,
                DriftOffset = new Vector2(rng.RandfRange(-240.0f, 240.0f), rng.RandfRange(-180.0f, 180.0f)),
                DriftSpeed = rng.RandfRange(0.45f, 0.8f),
            };

            if (!HasHazardSpacing(definition.HazardSpawns, hazard))
            {
                continue;
            }

            definition.HazardSpawns.Add(hazard);
        }

        for (int i = 0; i < debrisCount; i++)
        {
            Vector2 debrisPosition = SampleLanePressurePosition(rng, route, corridorHalfWidth, bounds);
            if (!IsSafeFromCriticalPoints(debrisPosition, spawnPoint, objectivePoint, ObjectiveSafetyRadius))
            {
                continue;
            }

            HazardSpawnData hazard = new()
            {
                HazardType = "debris_field",
                Position = debrisPosition,
                Radius = rng.RandfRange(74.0f, 128.0f),
                DamagePerSecond = rng.RandfRange(15.0f, 24.0f) + difficulty,
            };

            if (!HasHazardSpacing(definition.HazardSpawns, hazard))
            {
                continue;
            }

            definition.HazardSpawns.Add(hazard);
        }

        for (int i = 0; i < lightningCount; i++)
        {
            Vector2 lightningPosition = SampleLightningPosition(rng, route, bounds, objectivePoint, corridorHalfWidth);
            if (!IsSafeFromCriticalPoints(lightningPosition, spawnPoint, objectivePoint, ObjectiveSafetyRadius + 40.0f))
            {
                continue;
            }

            HazardSpawnData hazard = new()
            {
                HazardType = "lightning_arc",
                Position = lightningPosition,
                Radius = rng.RandfRange(105.0f, 160.0f),
                DamagePerSecond = rng.RandfRange(22.0f, 34.0f) + (difficulty * 1.4f),
                ActiveDurationSeconds = rng.RandfRange(0.9f, 1.4f),
                CooldownDurationSeconds = rng.RandfRange(1.2f, 2.1f),
            };

            if (!HasHazardSpacing(definition.HazardSpawns, hazard))
            {
                continue;
            }

            definition.HazardSpawns.Add(hazard);
        }

        return definition;
    }

    private static Vector2 FindOffRoutePosition(RandomNumberGenerator rng, Rect2 bounds, IReadOnlyList<Vector2> route, float corridorHalfWidth, float padding, Vector2 spawnPoint, Vector2 objectivePoint)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            Vector2 position = new(
                rng.RandfRange(bounds.Position.X + padding, bounds.End.X - padding),
                rng.RandfRange(bounds.Position.Y + padding, bounds.End.Y - padding));

            if (DistanceToPolyline(position, route) < corridorHalfWidth)
            {
                continue;
            }

            if (!IsSafeFromCriticalPoints(position, spawnPoint, objectivePoint, ObjectiveSafetyRadius))
            {
                continue;
            }

            return position;
        }

        return Vector2.Zero;
    }

    private static Vector2 SampleLanePressurePosition(RandomNumberGenerator rng, IReadOnlyList<Vector2> route, float corridorHalfWidth, Rect2 bounds)
    {
        int segmentIndex = rng.RandiRange(0, route.Count - 2);
        Vector2 segmentStart = route[segmentIndex];
        Vector2 segmentEnd = route[segmentIndex + 1];
        Vector2 along = segmentStart.Lerp(segmentEnd, rng.RandfRange(0.18f, 0.82f));
        Vector2 tangent = (segmentEnd - segmentStart).Normalized();
        Vector2 normal = new(-tangent.Y, tangent.X);
        float offset = corridorHalfWidth + rng.RandfRange(-65.0f, 18.0f);
        Vector2 position = along + (normal * offset * (rng.Randf() < 0.5f ? -1.0f : 1.0f));
        return ClampToBounds(position, bounds, 140.0f);
    }

    private static Vector2 SampleLightningPosition(RandomNumberGenerator rng, IReadOnlyList<Vector2> route, Rect2 bounds, Vector2 objectivePoint, float corridorHalfWidth)
    {
        if (rng.Randf() < 0.28f)
        {
            Vector2 offset = new(rng.RandfRange(-360.0f, -240.0f), rng.RandfRange(-260.0f, 260.0f));
            return ClampToBounds(objectivePoint + offset, bounds, 170.0f);
        }

        return SampleLanePressurePosition(rng, route, corridorHalfWidth * 0.85f, bounds);
    }

    private static bool IsSafeFromCriticalPoints(Vector2 position, Vector2 spawnPoint, Vector2 objectivePoint, float objectivePadding)
    {
        return position.DistanceTo(spawnPoint) >= SpawnSafetyRadius
            && position.DistanceTo(objectivePoint) >= objectivePadding;
    }

    private static bool HasHazardSpacing(IReadOnlyList<HazardSpawnData> existingHazards, HazardSpawnData candidate)
    {
        for (int i = 0; i < existingHazards.Count; i++)
        {
            HazardSpawnData existing = existingHazards[i];
            float minimumDistance = GetMinimumHazardSpacing(existing, candidate);
            if (existing.Position.DistanceTo(candidate.Position) < minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    private static float GetMinimumHazardSpacing(HazardSpawnData a, HazardSpawnData b)
    {
        bool eitherLargeCloud = IsLargeHazard(a.HazardType) || IsLargeHazard(b.HazardType);
        bool bothRoutePressure = IsRoutePressureHazard(a.HazardType) && IsRoutePressureHazard(b.HazardType);
        float radiusSpacing = (a.Radius + b.Radius) * 0.68f;

        if (eitherLargeCloud)
        {
            return Mathf.Max(radiusSpacing, 210.0f);
        }

        if (bothRoutePressure)
        {
            return Mathf.Max(radiusSpacing, 150.0f);
        }

        return Mathf.Max(radiusSpacing, 125.0f);
    }

    private static bool IsLargeHazard(string hazardType)
    {
        return hazardType == "static_cloud" || hazardType == "moving_storm_cell";
    }

    private static bool IsRoutePressureHazard(string hazardType)
    {
        return hazardType == "debris_field" || hazardType == "lightning_arc";
    }

    private static Vector2 ClampToBounds(Vector2 position, Rect2 bounds, float padding)
    {
        return new Vector2(
            Mathf.Clamp(position.X, bounds.Position.X + padding, bounds.End.X - padding),
            Mathf.Clamp(position.Y, bounds.Position.Y + padding, bounds.End.Y - padding));
    }

    private static float DistanceToPolyline(Vector2 point, IReadOnlyList<Vector2> route)
    {
        float closestDistance = float.MaxValue;
        for (int i = 0; i < route.Count - 1; i++)
        {
            closestDistance = Mathf.Min(closestDistance, DistanceToSegment(point, route[i], route[i + 1]));
        }

        return closestDistance;
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
