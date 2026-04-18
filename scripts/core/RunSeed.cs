namespace LudumDare59.Core;

using Godot;

public sealed class RunSeed
{
    public RunSeed(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static RunSeed CreateRandom()
    {
        return new RunSeed((int)Time.GetUnixTimeFromSystem());
    }

    public int GetSectorSeed(int sectorIndex)
    {
        unchecked
        {
            return (Value * 397) ^ (sectorIndex * 7919);
        }
    }
}
