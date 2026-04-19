namespace LudumDare59.Entities;

using System;

using Godot;

using LudumDare59.Components;
using LudumDare59.Interfaces;

public partial class RelayBuoy : Node2D, IActivatableObjective, ISignalSource
{
    public event Action<IActivatableObjective>? ObjectiveCompleted;

    public string ObjectiveLabel => IsCompleted ? "Relay online" : "Activate Relay";

    public Vector2 ObjectivePosition => GlobalPosition;

    public Vector2 SignalPosition => GlobalPosition;

    public bool IsCompleted { get; private set; }

    public float ActivationProgressNormalized => _activationComponent?.ActivationProgressNormalized ?? 0.0f;

    public bool IsPlayerInActivationRange => _activationComponent?.IsPlayerInActivationRange ?? false;

    private RelayActivationComponent _activationComponent = null!;

    public override void _Ready()
    {
        _activationComponent = GetNode<RelayActivationComponent>("RelayActivationComponent");
        _activationComponent.ActivationCompleted += OnActivationCompleted;
    }

    public override void _ExitTree()
    {
        if (_activationComponent is not null)
        {
            _activationComponent.ActivationCompleted -= OnActivationCompleted;
        }
    }

    public void AssignPlayer(PlayerShip playerShip)
    {
        _activationComponent.Initialize(this, playerShip);
    }

    private void OnActivationCompleted()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        ObjectiveCompleted?.Invoke(this);
    }
}
