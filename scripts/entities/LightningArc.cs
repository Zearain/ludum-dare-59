namespace LudumDare59.Entities;

using Godot;

using LudumDare59.Components;

public partial class LightningArc : StaticBody2D
{
    private Polygon2D _visual = null!;
    private HazardDamageComponent _hazardDamageComponent = null!;
    private float _activeDurationSeconds = 1.2f;
    private float _cooldownDurationSeconds = 1.8f;
    private float _phaseTimeRemaining;
    private bool _isActive = true;

    public override void _Ready()
    {
        _visual = GetNode<Polygon2D>("Visual");
        _hazardDamageComponent = GetNode<HazardDamageComponent>("HazardDamageComponent");
        SetArcActive(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        _phaseTimeRemaining -= (float)delta;
        if (_phaseTimeRemaining > 0.0f)
        {
            return;
        }

        SetArcActive(!_isActive);
    }

    public void Configure(float radius, float damagePerSecond, float activeDurationSeconds, float cooldownDurationSeconds)
    {
        _hazardDamageComponent.Configure(radius, damagePerSecond);
        _activeDurationSeconds = Mathf.Max(0.2f, activeDurationSeconds);
        _cooldownDurationSeconds = Mathf.Max(0.2f, cooldownDurationSeconds);
        SetArcActive(_isActive);
    }

    private void SetArcActive(bool isActive)
    {
        _isActive = isActive;
        _phaseTimeRemaining = isActive ? _activeDurationSeconds : _cooldownDurationSeconds;

        if (_hazardDamageComponent is not null)
        {
            _hazardDamageComponent.IsDamageEnabled = isActive;
        }

        if (_visual is not null)
        {
            _visual.Color = isActive
                ? new Color(1.0f, 0.95f, 0.4f, 0.9f)
                : new Color(0.55f, 0.6f, 1.0f, 0.28f);
        }
    }
}
