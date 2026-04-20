namespace LudumDare59.Systems;

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

    private const string UiUpAction = "ui_up";
    private const string UiDownAction = "ui_down";
    private const string UiAcceptAction = "ui_accept";
    private const string UiCancelAction = "ui_cancel";

    public static GameManager? Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance is not null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
        EnsureUiNavigationActions();
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

    private static InputEventJoypadButton CreateJoypadButtonEvent(JoyButton button)
    {
        return new InputEventJoypadButton
        {
            ButtonIndex = button,
            Pressed = true,
        };
    }

    private static InputEventJoypadMotion CreateJoypadMotionEvent(JoyAxis axis, float value)
    {
        return new InputEventJoypadMotion
        {
            Axis = axis,
            AxisValue = value,
        };
    }

    private static void EnsureUiNavigationActions()
    {
        EnsureAction(
            UiUpAction,
            0.2f,
            CreateKeyEvent(Key.Up),
            CreateJoypadButtonEvent(JoyButton.DpadUp),
            CreateJoypadMotionEvent(JoyAxis.LeftY, -1.0f));

        EnsureAction(
            UiDownAction,
            0.2f,
            CreateKeyEvent(Key.Down),
            CreateJoypadButtonEvent(JoyButton.DpadDown),
            CreateJoypadMotionEvent(JoyAxis.LeftY, 1.0f));

        EnsureAction(
            UiAcceptAction,
            0.2f,
            CreateKeyEvent(Key.Enter),
            CreateKeyEvent(Key.Space),
            CreateJoypadButtonEvent(JoyButton.A));

        EnsureAction(
            UiCancelAction,
            0.2f,
            CreateKeyEvent(Key.Escape),
            CreateJoypadButtonEvent(JoyButton.B));
    }

    private static void EnsureAction(string actionName, float deadzone, params InputEvent[] events)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName, deadzone);
        }

        foreach (InputEvent inputEvent in events)
        {
            if (!InputMap.ActionHasEvent(actionName, inputEvent))
            {
                InputMap.ActionAddEvent(actionName, inputEvent);
            }
        }
    }
}
