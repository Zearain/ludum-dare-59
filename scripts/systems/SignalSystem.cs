namespace LudumDare59.Systems;

using Godot;

using LudumDare59.Data;

public sealed class SignalSystem
{
    private const float MaxSignalDistance = 1800.0f;

    public SignalReading BuildReading(Vector2 playerPosition, Vector2 objectivePosition, float scannerMultiplier)
    {
        Vector2 toObjective = objectivePosition - playerPosition;
        float distance = toObjective.Length();
        Vector2 direction = distance > 0.001f ? toObjective / distance : Vector2.Right;

        float normalizedDistance = Mathf.Clamp(distance / MaxSignalDistance, 0.0f, 1.0f);
        float baseStrength = Mathf.Lerp(1.0f, 0.12f, normalizedDistance);
        float strength = Mathf.Clamp(baseStrength * Mathf.Max(1.0f, scannerMultiplier), 0.05f, 1.0f);
        float confidence = Mathf.Clamp(0.25f + (strength * 0.65f), 0.1f, 1.0f);

        return new SignalReading
        {
            Direction = direction,
            Strength = strength,
            Confidence = confidence,
            JitterAmount = Mathf.Lerp(0.18f, 0.02f, confidence),
        };
    }
}
