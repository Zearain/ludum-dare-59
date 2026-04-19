namespace LudumDare59;

using Godot;

using LudumDare59.Entities;
using LudumDare59.Systems;
using LudumDare59.UI;

public partial class Main : Node
{
    private RunController _runController = null!;
    private MusicController _musicController = null!;

    public override void _Ready()
    {
        _runController = GetNode<RunController>("RunController");
        _musicController = GetNode<MusicController>("MusicController");
        Node2D sectorRoot = GetNode<Node2D>("SectorRoot");
        Node2D objectiveContainer = sectorRoot.GetNode<Node2D>("ObjectiveContainer");
        Node2D hazardContainer = sectorRoot.GetNode<Node2D>("HazardContainer");
        PlayerShip playerShip = GetNode<PlayerShip>("PlayerShip");
        Camera2D camera = GetNode<Camera2D>("Camera2D");
        HudController hud = GetNode<HudController>("HUD");
        ScoreScreenController scoreScreen = GetNode<ScoreScreenController>("ScoreScreen");

        _runController.SectorLoaded += _musicController.OnSectorLoaded;
        _runController.RunFinished += _musicController.OnRunFinished;
        _runController.Initialize(playerShip, camera, hud, scoreScreen, objectiveContainer, hazardContainer);
        _musicController.OnSectorLoaded(0);
    }

    public override void _ExitTree()
    {
        if (_runController is not null && _musicController is not null)
        {
            _runController.SectorLoaded -= _musicController.OnSectorLoaded;
            _runController.RunFinished -= _musicController.OnRunFinished;
        }
    }
}
