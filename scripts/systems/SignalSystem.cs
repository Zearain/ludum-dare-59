namespace LudumDare59.Systems;

using Godot;

using LudumDare59.Data;

public sealed class SignalSystem
{
    public SignalReading BuildReading(
        Vector2 playerPosition,
        Vector2 objectivePosition,
        float signalRange,
        float noiseStrength,
        float echoStrength,
        int distortionSeed,
        float elapsedSeconds,
        float clarityMultiplier)
    {
        Vector2 toObjective = objectivePosition - playerPosition;
        float distance = toObjective.Length();
        Vector2 trueDirection = distance > 0.001f ? toObjective / distance : Vector2.Right;
        float distancePressure = Mathf.Clamp(distance / Mathf.Max(1.0f, signalRange), 0.0f, 1.2f);
        float clarityScale = clarityMultiplier <= 1.0f ? 1.0f : 1.0f / clarityMultiplier;
        float noiseAmount = Mathf.Clamp(((noiseStrength * 0.45f) + (distancePressure * 0.5f)) * clarityScale, 0.02f, 0.8f);
        float echoAmount = Mathf.Clamp(((echoStrength * 0.42f) + (distancePressure * 0.35f)) * clarityScale, 0.0f, 0.72f);

        Vector2 displayedDirection = ApplyEcho(trueDirection, distortionSeed, elapsedSeconds, echoAmount);
        displayedDirection = ApplyNoise(displayedDirection, distortionSeed, elapsedSeconds, noiseAmount);

        float confidence = Mathf.Clamp(0.82f - (distancePressure * 0.42f) - (noiseAmount * 0.22f) - (echoAmount * 0.18f), 0.18f, 0.96f);
        float strength = Mathf.Clamp(1.0f - distancePressure, 0.0f, 1.0f);
        strength = Mathf.Clamp(strength - (noiseAmount * 0.14f) - (echoAmount * 0.1f), 0.0f, 1.0f);

        return new SignalReading
        {
            Direction = displayedDirection,
            Strength = strength,
            Confidence = confidence,
            JitterAmount = Mathf.Clamp(Mathf.Lerp(0.48f, 0.08f, confidence) + (noiseAmount * 0.12f), 0.05f, 0.6f),
        };
    }

    public SignalReading BuildScanPing(
        Vector2 playerPosition,
        Vector2 objectivePosition,
        float signalRange,
        float noiseStrength,
        float echoStrength,
        int distortionSeed,
        float elapsedSeconds,
        float clarityMultiplier)
    {
        return BuildReading(
            playerPosition,
            objectivePosition,
            signalRange,
            noiseStrength,
            echoStrength,
            distortionSeed,
            elapsedSeconds,
            clarityMultiplier);
    }

    private static Vector2 ApplyEcho(Vector2 trueDirection, int distortionSeed, float elapsedSeconds, float echoAmount)
    {
        if (echoAmount <= 0.001f)
        {
            return trueDirection;
        }

        float phase = (distortionSeed % 97) * 0.17f;
        float sway = Mathf.Sin((elapsedSeconds * 0.95f) + phase);
        float echoAngle = ((Mathf.Pi * 0.82f) + (0.55f * sway)) * echoAmount;
        Vector2 falseDirection = trueDirection.Rotated(echoAngle);
        float blend = Mathf.Clamp(0.28f + (0.52f * Mathf.Max(0.0f, sway)), 0.0f, 0.95f) * echoAmount;
        return trueDirection.Slerp(falseDirection, blend).Normalized();
    }

    private static Vector2 ApplyNoise(Vector2 baseDirection, int distortionSeed, float elapsedSeconds, float noiseAmount)
    {
        if (noiseAmount <= 0.001f)
        {
            return baseDirection;
        }

        float jitterPhase = (distortionSeed % 59) * 0.31f;
        float jitterAngle = Mathf.Sin((elapsedSeconds * 6.5f) + jitterPhase) * 1.05f * noiseAmount;
        return baseDirection.Rotated(jitterAngle).Normalized();
    }
}
