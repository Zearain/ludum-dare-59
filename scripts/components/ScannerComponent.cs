namespace LudumDare59.Components;

using System;

using Godot;

public partial class ScannerComponent : Node
{
    [Export]
    public float ScanCooldownSeconds { get; set; } = 3.0f;

    [Export]
    public float PingDurationSeconds { get; set; } = 1.2f;

    [Export]
    public float ScanClarityMultiplier { get; set; } = 2.8f;

    public float CooldownRemaining => _cooldownRemaining;

    public event Action? ScanTriggered;

    private float _cooldownRemaining;

    public override void _Process(double delta)
    {
        _cooldownRemaining = Mathf.Max(0.0f, _cooldownRemaining - (float)delta);

        if (Input.IsActionJustPressed(Systems.GameManager.ScanPulseAction))
        {
            TryTriggerScan();
        }
    }

    public bool TryTriggerScan()
    {
        if (_cooldownRemaining > 0.0f)
        {
            return false;
        }

        _cooldownRemaining = ScanCooldownSeconds;
        ScanTriggered?.Invoke();
        return true;
    }

    public void ResetScanner()
    {
        _cooldownRemaining = 0.0f;
    }
}
