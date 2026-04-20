namespace LudumDare59.Components;

using System;

using Godot;

using LudumDare59.Entities;
using LudumDare59.Systems;

public partial class RelayActivationComponent : Node
{
    [Export]
    public float HoldDurationSeconds { get; set; } = 2.4f;

    [Export]
    public float ActivationRadius { get; set; } = 88.0f;

    [Export]
    public float ProgressDecayPerSecond { get; set; } = 0.65f;

    [Export]
    public bool RequiresInteractInput { get; set; } = true;

    [Export]
    public AudioStreamPlayer? ActivationLoopPlayer { get; set; }

    public float ActivationProgressNormalized => HoldDurationSeconds <= 0.0f
        ? 1.0f
        : Mathf.Clamp(_currentProgress / HoldDurationSeconds, 0.0f, 1.0f);

    public event Action? ActivationCompleted;

    private Node2D? _objectiveNode;
    private PlayerShip? _playerShip;
    private float _currentProgress;
    private bool _isComplete;
    private bool _hasWarnedMissingActivationLoopPlayer;

    public bool IsPlayerInActivationRange { get; private set; }

    public override void _Process(double delta)
    {
        if (_isComplete || _playerShip is null || _objectiveNode is null)
        {
            UpdateActivationLoopPlayback(false);
            return;
        }

        float distance = _objectiveNode.GlobalPosition.DistanceTo(_playerShip.GlobalPosition);
        bool inRange = distance <= ActivationRadius;
        IsPlayerInActivationRange = inRange;
        bool inputReady = !RequiresInteractInput || Input.IsActionPressed(GameManager.InteractAction);
        bool shouldPlayActivationLoop = inRange && inputReady;

        UpdateActivationLoopPlayback(shouldPlayActivationLoop);

        if (shouldPlayActivationLoop)
        {
            _currentProgress += (float)delta;
        }
        else
        {
            _currentProgress = Mathf.Max(0.0f, _currentProgress - ((float)delta * ProgressDecayPerSecond));
        }

        if (_currentProgress < HoldDurationSeconds)
        {
            return;
        }

        _isComplete = true;
        _currentProgress = HoldDurationSeconds;
        UpdateActivationLoopPlayback(false);
        ActivationCompleted?.Invoke();
    }

    public void Initialize(Node2D objectiveNode, PlayerShip playerShip)
    {
        _objectiveNode = objectiveNode;
        _playerShip = playerShip;
        WarnIfActivationLoopPlayerMissing();
        ResetActivation();
    }

    public void ConfigureActivation(float holdDurationSeconds)
    {
        HoldDurationSeconds = Mathf.Max(0.1f, holdDurationSeconds);
        ResetActivation();
    }

    public void ResetActivation()
    {
        _currentProgress = 0.0f;
        _isComplete = false;
        IsPlayerInActivationRange = false;
        UpdateActivationLoopPlayback(false);
    }

    private void UpdateActivationLoopPlayback(bool shouldPlay)
    {
        if (ActivationLoopPlayer is null)
        {
            return;
        }

        if (shouldPlay)
        {
            if (!ActivationLoopPlayer.Playing)
            {
                ActivationLoopPlayer.Play();
            }

            return;
        }

        if (ActivationLoopPlayer.Playing)
        {
            ActivationLoopPlayer.Stop();
        }
    }

    private void WarnIfActivationLoopPlayerMissing()
    {
        if (_hasWarnedMissingActivationLoopPlayer || ActivationLoopPlayer is not null || _objectiveNode is null)
        {
            return;
        }

        GD.PushWarning($"{nameof(RelayActivationComponent)} on '{_objectiveNode.Name}' has no ActivationLoopPlayer assigned.");
        _hasWarnedMissingActivationLoopPlayer = true;
    }
}
