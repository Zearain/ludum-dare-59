namespace LudumDare59.Interfaces;

using System;

using Godot;

using LudumDare59.Entities;

public interface IActivatableObjective
{
    event Action<IActivatableObjective>? ObjectiveCompleted;

    string ObjectiveLabel { get; }

    Vector2 ObjectivePosition { get; }

    bool IsCompleted { get; }

    void AssignPlayer(PlayerShip playerShip);
}
