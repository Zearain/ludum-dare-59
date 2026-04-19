namespace LudumDare59.Data;

using Godot;

public sealed class SignalReading
{
    public Vector2 Direction { get; set; }

    public float JitterAmount { get; set; }

    public float Confidence { get; set; }
}
