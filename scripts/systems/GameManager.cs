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
        EnsureInputActions();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private static void EnsureInputActions()
    {
        EnsureAction(MoveUpAction, Key.W, Key.Up);
        EnsureAction(MoveDownAction, Key.S, Key.Down);
        EnsureAction(MoveLeftAction, Key.A, Key.Left);
        EnsureAction(MoveRightAction, Key.D, Key.Right);
        EnsureAction(ScanPulseAction, Key.Space);
        EnsureAction(InteractAction, Key.E);
        EnsureAction(RestartRunAction, Key.R);
    }

    private static void EnsureAction(string actionName, params Key[] keys)
    {
        if (InputMap.HasAction(actionName))
        {
            return;
        }

        InputMap.AddAction(actionName, 0.5f);
        foreach (Key key in keys)
        {
            InputMap.ActionAddEvent(actionName, CreateKeyEvent(key));
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
