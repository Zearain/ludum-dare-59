namespace LudumDare59.Components;

using Godot;

public partial class ShipInputComponent : Node
{
    public Vector2 GetMovementInput()
    {
        return Input.GetVector(
            Systems.GameManager.MoveLeftAction,
            Systems.GameManager.MoveRightAction,
            Systems.GameManager.MoveUpAction,
            Systems.GameManager.MoveDownAction);
    }
}
