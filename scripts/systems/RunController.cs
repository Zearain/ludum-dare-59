namespace LudumDare59.Systems;

using System;

using Godot;

using LudumDare59.Components;
using LudumDare59.Core;
using LudumDare59.Data;
using LudumDare59.Entities;
using LudumDare59.Interfaces;
using LudumDare59.UI;

public partial class RunController : Node
{
    private const int RelaySectorCount = 5;

    private readonly RunTimer _runTimer = new();
    private readonly SectorGenerator _sectorGenerator = new();
    private readonly SignalSystem _signalSystem = new();
    private readonly ScoreSystem _scoreSystem = new();
    private readonly string[] _relayTransmissionFragments =
    [
        "Relay one restored. Night Saint altered course toward the deeper belt.",
        "Relay two restored. The pirate signal is skipping between debris shadows.",
        "Relay three restored. Distortion spikes ahead. Stay sharp.",
        "Relay four restored. The dreadnought pushed through a lightning seam.",
        "Relay five restored. Final source locked. The wreck is close now.",
    ];

    private const string FinalTransmission = "Black box recovered. Night Saint went silent at the heart of the storm.";

    private RunSeed? _runSeed;
    private RunState _runState = RunState.Intro;
    private int _currentSectorIndex;
    private int _currentStageIndex;
    private int _completedStagesInSector;
    private int _sectorsCleared;
    private string _currentTransmission = string.Empty;
    private RunResult? _lastRunResult;

    private PackedScene _relayBuoyScene = null!;
    private PackedScene _pirateWreckScene = null!;
    private PackedScene _staticCloudScene = null!;
    private PackedScene _movingStormCellScene = null!;
    private PackedScene _debrisFieldScene = null!;
    private PackedScene _lightningArcScene = null!;

    private Node2D _objectiveContainer = null!;
    private Node2D _hazardContainer = null!;
    private PlayerShip _playerShip = null!;
    private Camera2D _camera = null!;
    private HudController _hud = null!;
    private ScoreScreenController _scoreScreen = null!;

    private SectorDefinition? _activeSector;
    private IActivatableObjective? _activeObjective;
    private ObjectiveStageData? _activeStageData;
    private SignalReading? _activeScanPing;
    private float _scanPingTimeRemaining;

    private bool _isInitialized;

    public event Action<int>? SectorLoaded;

    public event Action<bool>? RunFinished;

    public override void _Ready()
    {
        _relayBuoyScene = GD.Load<PackedScene>("res://scenes/objectives/relay_buoy.tscn");
        _pirateWreckScene = GD.Load<PackedScene>("res://scenes/objectives/pirate_wreck.tscn");
        _staticCloudScene = GD.Load<PackedScene>("res://scenes/hazards/static_cloud.tscn");
        _movingStormCellScene = GD.Load<PackedScene>("res://scenes/hazards/moving_storm_cell.tscn");
        _debrisFieldScene = GD.Load<PackedScene>("res://scenes/hazards/debris_field.tscn");
        _lightningArcScene = GD.Load<PackedScene>("res://scenes/hazards/lightning_arc.tscn");
    }

    public override void _ExitTree()
    {
        if (_playerShip is not null)
        {
            _playerShip.Died -= OnPlayerDied;
            _playerShip.ScannerComponent.ScanTriggered -= OnScanTriggered;
        }

        DetachActiveObjective();
    }

    public void Initialize(
        PlayerShip playerShip,
        Camera2D camera,
        HudController hud,
        ScoreScreenController scoreScreen,
        Node2D objectiveContainer,
        Node2D hazardContainer)
    {
        if (_isInitialized)
        {
            return;
        }

        _playerShip = playerShip;
        _camera = camera;
        _hud = hud;
        _scoreScreen = scoreScreen;
        _objectiveContainer = objectiveContainer;
        _hazardContainer = hazardContainer;

        _playerShip.Died += OnPlayerDied;
        _playerShip.ScannerComponent.ScanTriggered += OnScanTriggered;
        _isInitialized = true;
        StartRun();
    }

    public override void _Process(double delta)
    {
        if (!_isInitialized)
        {
            return;
        }

        if (_runState == RunState.InSector)
        {
            _runTimer.Tick(delta);
            _scanPingTimeRemaining = Mathf.Max(0.0f, _scanPingTimeRemaining - (float)delta);
            if (_scanPingTimeRemaining <= 0.0f)
            {
                _activeScanPing = null;
            }

        }

        if ((_runState == RunState.RunComplete || _runState == RunState.RunFailed)
            && Input.IsActionJustPressed(GameManager.RestartRunAction))
        {
            StartRun();
            return;
        }

        UpdateHud();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isInitialized || _runState != RunState.InSector)
        {
            return;
        }

        ConstrainPlayerToArena();
        _camera.GlobalPosition = _playerShip.GlobalPosition;
    }

    private void StartRun()
    {
        ClearSectorContents();
        _runSeed = RunSeed.CreateRandom();
        _currentSectorIndex = 0;
        _currentStageIndex = 0;
        _completedStagesInSector = 0;
        _sectorsCleared = 0;
        _currentTransmission = "Storm relay chain reacquired. Follow the signal and restore the drifting buoys.";
        _lastRunResult = null;
        _activeScanPing = null;
        _scanPingTimeRemaining = 0.0f;
        _runTimer.Reset();
        _runState = RunState.InSector;
        _scoreScreen.HideScreen();
        _hud.Visible = true;
        _playerShip.ResetForRun(Vector2.Zero);
        LoadSector(_currentSectorIndex);
    }

    private void LoadSector(int sectorIndex)
    {
        if (_runSeed is null)
        {
            return;
        }

        bool isFinalSector = sectorIndex >= RelaySectorCount;
        int sectorSeed = _runSeed.GetSectorSeed(sectorIndex);

        _activeSector = _sectorGenerator.GenerateSector(sectorSeed, sectorIndex, isFinalSector);
        _currentStageIndex = 0;
        _completedStagesInSector = 0;

        ClearSectorContents();
        SpawnActiveObjective();
        SpawnHazards(_activeSector);
        _playerShip.PrepareForSector(_activeSector.SpawnPoint);
        _camera.GlobalPosition = _activeSector.SpawnPoint;
        SectorLoaded?.Invoke(_currentSectorIndex);
    }

    private void SpawnActiveObjective()
    {
        if (_activeSector is null)
        {
            return;
        }

        if (_currentStageIndex < 0 || _currentStageIndex >= _activeSector.ObjectiveStages.Count)
        {
            return;
        }

        foreach (Node child in _objectiveContainer.GetChildren())
        {
            child.QueueFree();
        }

        ObjectiveStageData stageData = _activeSector.ObjectiveStages[_currentStageIndex];
        PackedScene scene = stageData.StageKind switch
        {
            ObjectiveStageKind.SubRelay => _relayBuoyScene,
            ObjectiveStageKind.WarpRelay => _relayBuoyScene,
            ObjectiveStageKind.WreckPart => _pirateWreckScene,
            ObjectiveStageKind.BlackBoxExtraction => _pirateWreckScene,
            _ => _relayBuoyScene,
        };

        Node candidate = scene.Instantiate();
        if (candidate is not Node2D objectiveNode)
        {
            candidate.QueueFree();
            GD.PushError("Objective scene root must be Node2D.");
            return;
        }

        objectiveNode.GlobalPosition = stageData.Position;
        _objectiveContainer.AddChild(objectiveNode);

        if (objectiveNode is not IActivatableObjective objective)
        {
            GD.PushError("Objective scene must implement IActivatableObjective.");
            return;
        }

        objective.Configure(stageData);
        DetachActiveObjective();
        _activeObjective = objective;
        _activeStageData = stageData;
        _activeObjective.ObjectiveCompleted += OnObjectiveCompleted;
        _activeObjective.AssignPlayer(_playerShip);
    }

    private void SpawnHazards(SectorDefinition sector)
    {
        foreach (HazardSpawnData hazardSpawn in sector.HazardSpawns)
        {
            PackedScene? scene = hazardSpawn.HazardType switch
            {
                "static_cloud" => _staticCloudScene,
                "moving_storm_cell" => _movingStormCellScene,
                "debris_field" => _debrisFieldScene,
                "lightning_arc" => _lightningArcScene,
                _ => null,
            };

            if (scene is null)
            {
                continue;
            }

            Node candidate = scene.Instantiate();
            if (candidate is not Node2D hazardNode)
            {
                candidate.QueueFree();
                continue;
            }

            hazardNode.GlobalPosition = hazardSpawn.Position;
            _hazardContainer.AddChild(hazardNode);

            HazardDamageComponent? damageComponent = hazardNode.GetNodeOrNull<HazardDamageComponent>("HazardDamageComponent");
            damageComponent?.Configure(hazardSpawn.Radius, hazardSpawn.DamagePerSecond);

            if (hazardNode is LightningArc lightningArc)
            {
                lightningArc.Configure(
                    hazardSpawn.Radius,
                    hazardSpawn.DamagePerSecond,
                    hazardSpawn.ActiveDurationSeconds,
                    hazardSpawn.CooldownDurationSeconds);
            }

            if (hazardNode is MovingStormCell movingStormCell)
            {
                movingStormCell.Configure(
                    hazardSpawn.DriftOffset,
                    hazardSpawn.DriftSpeed);
            }

            if (hazardNode is DebrisField debrisField)
            {
                debrisField.Configure(hazardSpawn.Radius, hazardSpawn.DamagePerSecond);
            }

        }
    }

    private void OnObjectiveCompleted(IActivatableObjective objective)
    {
        if (_runState != RunState.InSector || objective != _activeObjective || _activeSector is null)
        {
            return;
        }

        _completedStagesInSector = Mathf.Max(_completedStagesInSector, _currentStageIndex + 1);

        if (_currentStageIndex + 1 < _activeSector.ObjectiveStages.Count)
        {
            _currentStageIndex += 1;
            SpawnActiveObjective();
            return;
        }

        if (_activeSector.IsFinalSector)
        {
            _sectorsCleared = RelaySectorCount + 1;
            _currentTransmission = FinalTransmission;
            CompleteRun(true);
            return;
        }

        _sectorsCleared = Mathf.Max(_sectorsCleared, _currentSectorIndex + 1);
        if (_currentSectorIndex < _relayTransmissionFragments.Length)
        {
            _currentTransmission = _relayTransmissionFragments[_currentSectorIndex];
        }

        _currentSectorIndex += 1;
        LoadSector(_currentSectorIndex);
    }

    private void OnPlayerDied()
    {
        if (_runState != RunState.InSector)
        {
            return;
        }

        _sectorsCleared = Mathf.Max(_sectorsCleared, _currentSectorIndex);
        CompleteRun(false);
    }

    private void ClearSectorContents()
    {
        DetachActiveObjective();
        _activeStageData = null;

        foreach (Node child in _objectiveContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (Node child in _hazardContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void DetachActiveObjective()
    {
        if (_activeObjective is null)
        {
            return;
        }

        _activeObjective.ObjectiveCompleted -= OnObjectiveCompleted;
        _activeObjective = null;
    }

    private void UpdateHud()
    {
        _hud.SetTimer(_runTimer.ElapsedSeconds);
        _hud.SetHull(_playerShip.HullCurrent, _playerShip.HullMax);

        ScannerComponent scanner = _playerShip.ScannerComponent;
        _hud.SetScanner(scanner.CooldownRemaining, scanner.ScanCooldownSeconds);

        if (_activeObjective is not null)
        {
            _hud.SetObjective(_activeObjective.ObjectiveLabel);
            _hud.SetActivation(
                _activeObjective.ActivationProgressNormalized,
                _activeObjective.IsPlayerInActivationRange,
                true);
        }
        else
        {
            _hud.SetObjective("No objective");
            _hud.ClearActivation();
        }

        if (_activeScanPing is not null && _scanPingTimeRemaining > 0.0f)
        {
            _hud.ShowScannerPing(_activeScanPing);
        }
        else
        {
            _hud.ClearScannerPing();
        }

        if (_activeSector is not null)
        {
            _hud.SetSector(_activeSector.SectorIndex + 1, _activeSector.IsFinalSector);
            int activeStageDisplay = Mathf.Clamp(_currentStageIndex + 1, 1, Mathf.Max(1, _activeSector.ObjectiveStages.Count));
            _hud.SetStageProgress(activeStageDisplay, _activeSector.ObjectiveStages.Count, _activeStageData?.StageKind);
        }
        else
        {
            _hud.ClearStageProgress();
        }

        _hud.SetTransmission(_currentTransmission);

        _hud.SetRunState(_runState switch
        {
            RunState.InSector => string.Empty,
            RunState.RunComplete => "Transmission recovered.",
            RunState.RunFailed => "Storm contact lost.",
            _ => string.Empty,
        });
    }

    private void CompleteRun(bool completed)
    {
        _runState = completed ? RunState.RunComplete : RunState.RunFailed;
        RunFinished?.Invoke(completed);
        _lastRunResult = _scoreSystem.BuildResult(
            completed,
            _runTimer.ElapsedSeconds,
            _playerShip.TotalDamageTaken,
            _playerShip.HullCurrent,
            _sectorsCleared);
        _hud.Visible = false;
        _scoreScreen.ShowResult(_lastRunResult);
    }

    private void OnScanTriggered()
    {
        if (_runState != RunState.InSector || _activeObjective is null)
        {
            return;
        }

        _activeScanPing = _signalSystem.BuildScanPing(
            _playerShip.GlobalPosition,
            _activeObjective.ObjectivePosition,
            _activeSector?.SignalRange ?? 3200.0f,
            _activeSector?.NoiseStrength ?? 0.0f,
            _activeSector?.EchoStrength ?? 0.0f,
            _activeSector?.DistortionSeed ?? 0,
            (float)_runTimer.ElapsedSeconds,
            _playerShip.ScannerComponent.ClarityMultiplier);
        _scanPingTimeRemaining = _playerShip.ScannerComponent.PingDurationSeconds;
    }

    private void ConstrainPlayerToArena()
    {
        if (_activeSector is null)
        {
            return;
        }

        Rect2 bounds = _activeSector.ArenaBounds;
        Vector2 position = _playerShip.GlobalPosition;
        position.X = Mathf.Clamp(position.X, bounds.Position.X, bounds.End.X);
        position.Y = Mathf.Clamp(position.Y, bounds.Position.Y, bounds.End.Y);
        _playerShip.GlobalPosition = position;
    }

    private enum RunState
    {
        Intro,
        InSector,
        RunComplete,
        RunFailed,
    }
}
