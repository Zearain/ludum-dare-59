namespace LudumDare59.Core;

public sealed class RunTimer
{
    public double ElapsedSeconds { get; private set; }

    public void Reset()
    {
        ElapsedSeconds = 0.0;
    }

    public void Tick(double delta)
    {
        if (delta <= 0.0)
        {
            return;
        }

        ElapsedSeconds += delta;
    }
}
