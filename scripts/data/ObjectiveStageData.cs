namespace LudumDare59.Data;

using Godot;

public sealed class ObjectiveStageData
{
    public ObjectiveStageKind StageKind { get; set; }

    public Vector2 Position { get; set; }

    public float ActivationDurationSeconds { get; set; }

    public string ObjectiveLabel { get; set; } = string.Empty;
}
