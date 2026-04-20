namespace LudumDare59.Entities;

using System;

using Godot;

using LudumDare59.Components;
using LudumDare59.Data;
using LudumDare59.Interfaces;

public partial class RelayBuoy : Node2D, IActivatableObjective, ISignalSource
{
    public event Action<IActivatableObjective>? ObjectiveCompleted;

    public string ObjectiveLabel => IsCompleted ? _completedLabel : _objectiveLabel;

    public Vector2 ObjectivePosition => GlobalPosition;

    public Vector2 SignalPosition => GlobalPosition;

    public bool IsCompleted { get; private set; }

    public float ActivationProgressNormalized => _activationComponent?.ActivationProgressNormalized ?? 0.0f;

    public bool IsPlayerInActivationRange => _activationComponent?.IsPlayerInActivationRange ?? false;

    private RelayActivationComponent _activationComponent = null!;
    private AudioStreamPlayer _activationAudio = null!;
    private Polygon2D _visual = null!;
    private string _objectiveLabel = "Activate Relay";
    private string _completedLabel = "Relay online";

    public override void _Ready()
    {
        _activationComponent = GetNode<RelayActivationComponent>("RelayActivationComponent");
        _activationAudio = GetNode<AudioStreamPlayer>("ActivationAudio");
        _visual = GetNode<Polygon2D>("Visual");
        _activationComponent.ActivationLoopPlayer = _activationAudio;
        _activationComponent.ActivationCompleted += OnActivationCompleted;
    }

    public override void _ExitTree()
    {
        if (_activationComponent is not null)
        {
            _activationComponent.ActivationCompleted -= OnActivationCompleted;
            _activationComponent.ActivationLoopPlayer = null;
        }
    }

    public void AssignPlayer(PlayerShip playerShip)
    {
        _activationComponent.Initialize(this, playerShip);
    }

    public void Configure(ObjectiveStageData stageData)
    {
        IsCompleted = false;
        _activationComponent.ConfigureActivation(stageData.ActivationDurationSeconds);
        _objectiveLabel = string.IsNullOrWhiteSpace(stageData.ObjectiveLabel)
            ? stageData.StageKind == ObjectiveStageKind.WarpRelay ? "Activate Warp Relay" : "Activate Sub-Relay"
            : stageData.ObjectiveLabel;
        _completedLabel = stageData.StageKind == ObjectiveStageKind.WarpRelay
            ? "Warp relay online"
            : "Sub-relay online";

        if (_visual is null)
        {
            return;
        }

        _visual.Color = stageData.StageKind == ObjectiveStageKind.WarpRelay
            ? new Color(1.0f, 0.54f, 0.18f, 0.95f)
            : new Color(0.2f, 1.0f, 0.8f, 0.95f);
        _visual.Scale = stageData.StageKind == ObjectiveStageKind.WarpRelay
            ? new Vector2(1.35f, 1.35f)
            : Vector2.One;
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
