namespace LudumDare59.Components;

using System;

using Godot;

public partial class ScannerComponent : Node
{
    [Export]
    public float ScanCooldownSeconds { get; set; } = 3.0f;

    [Export]
    public float ClarityBoostSeconds { get; set; } = 1.5f;

    [Export]
    public float ScanClarityMultiplier { get; set; } = 1.7f;

    public float CooldownRemaining => _cooldownRemaining;

    public float ActiveClarityMultiplier => _clarityTimeRemaining > 0.0f ? ScanClarityMultiplier : 1.0f;

    public event Action? ScanTriggered;

    private float _cooldownRemaining;
    private float _clarityTimeRemaining;

    public override void _Process(double delta)
    {
        _cooldownRemaining = Mathf.Max(0.0f, _cooldownRemaining - (float)delta);
        _clarityTimeRemaining = Mathf.Max(0.0f, _clarityTimeRemaining - (float)delta);

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
        _clarityTimeRemaining = ClarityBoostSeconds;
        ScanTriggered?.Invoke();
        return true;
    }

    public void ResetScanner()
    {
        _cooldownRemaining = 0.0f;
        _clarityTimeRemaining = 0.0f;
    }
}
