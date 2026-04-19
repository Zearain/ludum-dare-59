namespace LudumDare59.Entities;

using Godot;

using LudumDare59.Components;

public partial class MovingStormCell : Node2D
{
    private Polygon2D _visual = null!;
    private HazardDamageComponent _hazardDamageComponent = null!;
    private Vector2 _origin;
    private Vector2 _driftOffset;
    private float _driftSpeed = 0.8f;
    private float _elapsedTime;

    public override void _Ready()
    {
        _visual = GetNode<Polygon2D>("Visual");
        _hazardDamageComponent = GetNode<HazardDamageComponent>("HazardDamageComponent");
        _origin = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        _elapsedTime += (float)delta * _driftSpeed;
        float t = (Mathf.Sin(_elapsedTime) + 1.0f) * 0.5f;
        GlobalPosition = _origin.Lerp(_origin + _driftOffset, t);
    }

    public void Configure(float radius, float damagePerSecond, Vector2 driftOffset, float driftSpeed)
    {
        _hazardDamageComponent.Configure(radius, damagePerSecond);
        _driftOffset = driftOffset;
        _driftSpeed = Mathf.Max(0.2f, driftSpeed);

        if (_visual is not null)
        {
            _visual.Scale = new Vector2(radius / 84.0f, radius / 84.0f);
        }
    }
}
