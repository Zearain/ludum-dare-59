namespace LudumDare59;

using Godot;

using LudumDare59.Entities;
using LudumDare59.Systems;
using LudumDare59.UI;

public partial class Main : Node
{
    private RunController _runController = null!;
    private MusicController _musicController = null!;
    private HudController _hud = null!;
    private ScoreScreenController _scoreScreen = null!;
    private MainMenuController _mainMenu = null!;

    public override void _Ready()
    {
        _runController = GetNode<RunController>("RunController");
        _musicController = GetNode<MusicController>("MusicController");
        Node2D sectorRoot = GetNode<Node2D>("SectorRoot");
        Node2D objectiveContainer = sectorRoot.GetNode<Node2D>("ObjectiveContainer");
        Node2D hazardContainer = sectorRoot.GetNode<Node2D>("HazardContainer");
        PlayerShip playerShip = GetNode<PlayerShip>("PlayerShip");
        Camera2D camera = GetNode<Camera2D>("Camera2D");
        _hud = GetNode<HudController>("HUD");
        _scoreScreen = GetNode<ScoreScreenController>("ScoreScreen");
        _mainMenu = GetNode<MainMenuController>("MainMenu");

        _runController.SectorLoaded += _musicController.OnSectorLoaded;
        _runController.RunFinished += _musicController.OnRunFinished;
        _runController.RunFinished += OnRunFinished;

        _mainMenu.StartRequested += OnStartRequested;
        _mainMenu.QuitRequested += OnQuitRequested;

        _scoreScreen.RestartRequested += OnRestartRequested;
        _scoreScreen.QuitRequested += OnQuitRequested;

        _runController.Initialize(playerShip, camera, _hud, _scoreScreen, objectiveContainer, hazardContainer);

        ShowMainMenu();
    }

    public override void _ExitTree()
    {
        if (_runController is not null && _musicController is not null)
        {
            _runController.SectorLoaded -= _musicController.OnSectorLoaded;
            _runController.RunFinished -= _musicController.OnRunFinished;
            _runController.RunFinished -= OnRunFinished;
        }

        if (_mainMenu is not null)
        {
            _mainMenu.StartRequested -= OnStartRequested;
            _mainMenu.QuitRequested -= OnQuitRequested;
        }

        if (_scoreScreen is not null)
        {
            _scoreScreen.RestartRequested -= OnRestartRequested;
            _scoreScreen.QuitRequested -= OnQuitRequested;
        }
    }

    private void ShowMainMenu()
    {
        GetTree().Paused = true;
        _hud.Visible = false;
        _scoreScreen.HideScreen();
        _mainMenu.ShowMenu();
        _musicController.PlayMainMenu();
    }

    private void StartRunFromMenu()
    {
        _mainMenu.HideMenu();
        _scoreScreen.HideScreen();
        GetTree().Paused = false;
        _runController.StartNewRun();
    }

    private void OnStartRequested()
    {
        StartRunFromMenu();
    }

    private void OnRestartRequested()
    {
        StartRunFromMenu();
    }

    private void OnQuitRequested()
    {
        GetTree().Quit();
    }

    private void OnRunFinished(bool _completed)
    {
        GetTree().Paused = true;
    }
}
