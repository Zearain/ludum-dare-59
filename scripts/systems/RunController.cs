namespace LudumDare59.Systems;

using Godot;

using LudumDare59.Components;
using LudumDare59.Core;
using LudumDare59.Data;
using LudumDare59.Entities;
using LudumDare59.Interfaces;
using LudumDare59.UI;

public partial class RunController : Node
{
    private const int RelaySectorCount = 1;

    private readonly RunTimer _runTimer = new();
    private readonly SectorGenerator _sectorGenerator = new();
    private readonly SignalSystem _signalSystem = new();

    private RunSeed? _runSeed;
    private RunState _runState = RunState.Intro;
    private int _currentSectorIndex;

    private PackedScene _relayBuoyScene = null!;
    private PackedScene _pirateWreckScene = null!;
    private PackedScene _staticCloudScene = null!;

    private Node2D _sectorRoot = null!;
    private Node2D _objectiveContainer = null!;
    private Node2D _hazardContainer = null!;
    private PlayerShip _playerShip = null!;
    private Camera2D _camera = null!;
    private HudController _hud = null!;

    private SectorDefinition? _activeSector;
    private IActivatableObjective? _activeObjective;

    private bool _isInitialized;

    public override void _Ready()
    {
        _relayBuoyScene = GD.Load<PackedScene>("res://scenes/objectives/relay_buoy.tscn");
        _pirateWreckScene = GD.Load<PackedScene>("res://scenes/objectives/pirate_wreck.tscn");
        _staticCloudScene = GD.Load<PackedScene>("res://scenes/hazards/static_cloud.tscn");
    }

    public override void _ExitTree()
    {
        if (_playerShip is not null)
        {
            _playerShip.Died -= OnPlayerDied;
        }

        DetachActiveObjective();
    }

    public void Initialize(
        PlayerShip playerShip,
        Camera2D camera,
        HudController hud,
        Node2D sectorRoot,
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
        _sectorRoot = sectorRoot;
        _objectiveContainer = objectiveContainer;
        _hazardContainer = hazardContainer;

        _playerShip.Died += OnPlayerDied;
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
        _runTimer.Reset();
        _runState = RunState.InSector;
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

        ClearSectorContents();
        SpawnObjective(_activeSector);
        SpawnHazards(_activeSector);
        _playerShip.PrepareForSector(_activeSector.SpawnPoint);
    }

    private void SpawnObjective(SectorDefinition sector)
    {
        PackedScene scene = sector.IsFinalSector ? _pirateWreckScene : _relayBuoyScene;
        Node candidate = scene.Instantiate();
        if (candidate is not Node2D objectiveNode)
        {
            candidate.QueueFree();
            GD.PushError("Objective scene root must be Node2D.");
            return;
        }

        objectiveNode.GlobalPosition = sector.ObjectivePoint;
        _objectiveContainer.AddChild(objectiveNode);

        if (objectiveNode is not IActivatableObjective objective)
        {
            GD.PushError("Objective scene must implement IActivatableObjective.");
            return;
        }

        DetachActiveObjective();
        _activeObjective = objective;
        _activeObjective.ObjectiveCompleted += OnObjectiveCompleted;
        _activeObjective.AssignPlayer(_playerShip);
    }

    private void SpawnHazards(SectorDefinition sector)
    {
        foreach (HazardSpawnData hazardSpawn in sector.HazardSpawns)
        {
            if (hazardSpawn.HazardType != "static_cloud")
            {
                continue;
            }

            Node candidate = _staticCloudScene.Instantiate();
            if (candidate is not Node2D hazardNode)
            {
                candidate.QueueFree();
                continue;
            }

            hazardNode.GlobalPosition = hazardSpawn.Position;
            _hazardContainer.AddChild(hazardNode);

            HazardDamageComponent? damageComponent = hazardNode.GetNodeOrNull<HazardDamageComponent>("HazardDamageComponent");
            damageComponent?.Initialize(hazardNode, _playerShip);
            damageComponent?.Configure(hazardSpawn.Radius, hazardSpawn.DamagePerSecond);

            Node2D? visual = hazardNode.GetNodeOrNull<Node2D>("Visual");
            if (visual is not null)
            {
                float scale = hazardSpawn.Radius / 84.0f;
                visual.Scale = new Vector2(scale, scale);
            }
        }
    }

    private void OnObjectiveCompleted(IActivatableObjective objective)
    {
        if (_runState != RunState.InSector || objective != _activeObjective)
        {
            return;
        }

        if (_activeSector?.IsFinalSector == true)
        {
            _runState = RunState.RunComplete;
            return;
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

        _runState = RunState.RunFailed;
    }

    private void ClearSectorContents()
    {
        DetachActiveObjective();

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
            SignalReading signalReading = _signalSystem.BuildReading(
                _playerShip.GlobalPosition,
                _activeObjective.ObjectivePosition,
                scanner.ActiveClarityMultiplier);

            _hud.SetSignal(signalReading);
            _hud.SetObjective(_activeObjective.ObjectiveLabel);
        }
        else
        {
            _hud.ClearSignal();
            _hud.SetObjective("No objective");
        }

        _hud.SetRunState(_runState switch
        {
            RunState.InSector => string.Empty,
            RunState.RunComplete => "Run complete - press R to restart",
            RunState.RunFailed => "Run failed - press R to restart",
            _ => string.Empty,
        });
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
