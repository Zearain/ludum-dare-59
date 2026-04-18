namespace LudumDare59.Components;

using Godot;

using LudumDare59.Entities;

public partial class ShipMovementComponent : Node
{
    [Export]
    public float MaxSpeed { get; set; } = 420.0f;

    [Export]
    public float Acceleration { get; set; } = 1800.0f;

    [Export]
    public float Deceleration { get; set; } = 2000.0f;

    [Export]
    public PlayerShip OwnerBody { get; set; } = null!;

    public Vector2 Velocity => OwnerBody?.Velocity ?? Vector2.Zero;

    private Vector2 _currentInput;

    public void ProcessMovement(Vector2 inputDirection, double delta)
    {
        _currentInput = inputDirection;
        if (OwnerBody is null)
        {
            return;
        }

        Vector2 desiredVelocity = _currentInput * MaxSpeed;
        float step = (float)delta * (_currentInput == Vector2.Zero ? Deceleration : Acceleration);

        OwnerBody.Velocity = OwnerBody.Velocity.MoveToward(desiredVelocity, step);
        OwnerBody.MoveAndSlide();
    }

    public void ResetMotion()
    {
        if (OwnerBody is not null)
        {
            OwnerBody.Velocity = Vector2.Zero;
        }
    }
}
