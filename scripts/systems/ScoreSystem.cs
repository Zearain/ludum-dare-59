namespace LudumDare59.Systems;

using LudumDare59.Data;

public sealed class ScoreSystem
{
    public RunResult BuildResult(bool completed, double finalTimeSeconds, float damageTaken, float hullRemaining, int sectorsCleared)
    {
        int timePenalty = (int)System.Math.Round(finalTimeSeconds * 8.0);
        int damagePenalty = (int)System.Math.Round(damageTaken * 35.0f);
        int hullBonus = (int)System.Math.Round(hullRemaining * 10.0f);
        int sectorBonus = sectorsCleared * 450;
        int completionBonus = completed ? 4500 : 0;
        int finalScore = System.Math.Max(0, 5000 + completionBonus + sectorBonus + hullBonus - timePenalty - damagePenalty);

        return new RunResult
        {
            Completed = completed,
            ResultTitle = completed ? "Black Box Recovered" : "Run Lost In The Storm",
            FinalTimeSeconds = finalTimeSeconds,
            DamageTaken = damageTaken,
            HullRemaining = hullRemaining,
            SectorsCleared = sectorsCleared,
            FinalScore = finalScore,
        };
    }
}
