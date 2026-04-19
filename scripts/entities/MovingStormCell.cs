namespace LudumDare59.Entities;

using Godot;

public partial class MovingStormCell : AnimatableBody2D
{
    private Vector2 _origin;
    private Vector2 _driftOffset;
    private float _driftSpeed = 0.8f;
    private float _elapsedTime;

    public override void _Ready()
    {
        _origin = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        _elapsedTime += (float)delta * _driftSpeed;
        float t = (Mathf.Sin(_elapsedTime) + 1.0f) * 0.5f;
        GlobalPosition = _origin.Lerp(_origin + _driftOffset, t);
    }

    public void Configure(Vector2 driftOffset, float driftSpeed)
    {
        _driftOffset = driftOffset;
        _driftSpeed = Mathf.Max(0.2f, driftSpeed);
    }
}
