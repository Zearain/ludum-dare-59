namespace LudumDare59.Entities;

using System;

using Godot;

using LudumDare59.Components;
using LudumDare59.Data;
using LudumDare59.Interfaces;

public partial class PirateWreck : Node2D, IActivatableObjective, ISignalSource
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
    private string _objectiveLabel = "Recover Wreck Part";
    private string _completedLabel = "Wreck part secured";

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
            ? stageData.StageKind == ObjectiveStageKind.BlackBoxExtraction ? "Extract Black Box" : "Recover Wreck Part"
            : stageData.ObjectiveLabel;
        _completedLabel = stageData.StageKind == ObjectiveStageKind.BlackBoxExtraction
            ? "Black box recovered"
            : "Wreck part secured";

        if (_visual is null)
        {
            return;
        }

        _visual.Color = stageData.StageKind == ObjectiveStageKind.BlackBoxExtraction
            ? new Color(1.0f, 0.9f, 0.3f, 0.95f)
            : new Color(0.95f, 0.4f, 0.6f, 0.95f);
        _visual.Scale = stageData.StageKind == ObjectiveStageKind.BlackBoxExtraction
            ? new Vector2(1.45f, 1.45f)
            : new Vector2(1.1f, 1.1f);
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
