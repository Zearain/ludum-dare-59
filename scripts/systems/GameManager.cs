namespace LudumDare59.Systems;

using System;

using Godot;

public partial class GameManager : Node
{
    public const string MoveUpAction = "move_up";
    public const string MoveDownAction = "move_down";
    public const string MoveLeftAction = "move_left";
    public const string MoveRightAction = "move_right";
    public const string ScanPulseAction = "scan_pulse";
    public const string InteractAction = "interact";
    public const string RestartRunAction = "restart_run";

    public static GameManager? Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance is not null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private static InputEventKey CreateKeyEvent(Key key)
    {
        return new InputEventKey
        {
            Keycode = key,
            PhysicalKeycode = key,
        };
    }
}
