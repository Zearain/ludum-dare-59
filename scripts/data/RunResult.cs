namespace LudumDare59.Data;

public sealed class RunResult
{
    public bool Completed { get; set; }

    public string ResultTitle { get; set; } = string.Empty;

    public double FinalTimeSeconds { get; set; }

    public float DamageTaken { get; set; }

    public float HullRemaining { get; set; }

    public int SectorsCleared { get; set; }

    public int FinalScore { get; set; }
}
