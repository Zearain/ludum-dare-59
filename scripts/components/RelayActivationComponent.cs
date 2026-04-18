namespace LudumDare59.Components;

using System;

using Godot;

using LudumDare59.Entities;
using LudumDare59.Systems;

public partial class RelayActivationComponent : Node
{
    [Export]
    public float HoldDurationSeconds { get; set; } = 2.0f;

    [Export]
    public float ActivationRadius { get; set; } = 72.0f;

    [Export]
    public bool RequiresInteractInput { get; set; } = true;

    public float ActivationProgressNormalized => HoldDurationSeconds <= 0.0f
        ? 1.0f
        : Mathf.Clamp(_currentProgress / HoldDurationSeconds, 0.0f, 1.0f);

    public event Action? ActivationCompleted;

    private Node2D? _objectiveNode;
    private PlayerShip? _playerShip;
    private float _currentProgress;
    private bool _isComplete;

    public override void _Process(double delta)
    {
        if (_isComplete || _playerShip is null || _objectiveNode is null)
        {
            return;
        }

        float distance = _objectiveNode.GlobalPosition.DistanceTo(_playerShip.GlobalPosition);
        bool inRange = distance <= ActivationRadius;
        bool inputReady = !RequiresInteractInput || Input.IsActionPressed(GameManager.InteractAction);

        if (inRange && inputReady)
        {
            _currentProgress += (float)delta;
        }
        else
        {
            _currentProgress = 0.0f;
        }

        if (_currentProgress < HoldDurationSeconds)
        {
            return;
        }

        _isComplete = true;
        _currentProgress = HoldDurationSeconds;
        ActivationCompleted?.Invoke();
    }

    public void Initialize(Node2D objectiveNode, PlayerShip playerShip)
    {
        _objectiveNode = objectiveNode;
        _playerShip = playerShip;
        ResetActivation();
    }

    public void ResetActivation()
    {
        _currentProgress = 0.0f;
        _isComplete = false;
    }
}
