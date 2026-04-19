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

    [Export]
    public float ClarityBoostDurationSeconds { get; set; } = 1.2f;

    public float CooldownRemaining => _cooldownRemaining;

    public float ClarityMultiplier => _clarityBoostRemaining > 0.0f ? ScanClarityMultiplier : 1.0f;

    public float ClarityBoostRemaining => _clarityBoostRemaining;

    public event Action? ScanTriggered;

    private float _cooldownRemaining;
    private float _clarityBoostRemaining;

    public override void _Process(double delta)
    {
        _cooldownRemaining = Mathf.Max(0.0f, _cooldownRemaining - (float)delta);
        _clarityBoostRemaining = Mathf.Max(0.0f, _clarityBoostRemaining - (float)delta);

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
        _clarityBoostRemaining = ClarityBoostDurationSeconds;
        ScanTriggered?.Invoke();
        return true;
    }

    public void ResetScanner()
    {
        _cooldownRemaining = 0.0f;
        _clarityBoostRemaining = 0.0f;
    }
}
