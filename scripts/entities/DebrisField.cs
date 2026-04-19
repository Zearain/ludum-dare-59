namespace LudumDare59.Entities;

using Godot;

using LudumDare59.Components;

public partial class DebrisField : AnimatableBody2D
{
    private Polygon2D _visual = null!;
    private HazardDamageComponent _hazardDamageComponent = null!;
    private float _radius = 84.0f;
    private float _time;

    public override void _Ready()
    {
        _visual = GetNode<Polygon2D>("Visual");
        _hazardDamageComponent = GetNode<HazardDamageComponent>("HazardDamageComponent");
    }

    public override void _PhysicsProcess(double delta)
    {
        _time += (float)delta;
        Rotation += (float)delta * 0.14f;

        float pulse = 0.92f + (Mathf.Sin(_time * 2.8f) * 0.08f);
        _visual.Scale = new Vector2((_radius / 84.0f) * pulse, (_radius / 84.0f) * pulse);
        _visual.Color = new Color(0.75f, 0.72f, 0.82f, Mathf.Lerp(0.64f, 0.92f, pulse));
        _hazardDamageComponent.SyncCollisionToVisual();
    }

    public void Configure(float radius, float damagePerSecond)
    {
        _radius = Mathf.Max(36.0f, radius);
        _hazardDamageComponent.Configure(_radius, damagePerSecond);
    }
}
