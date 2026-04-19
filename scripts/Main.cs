namespace LudumDare59;

using Godot;

using LudumDare59.Entities;
using LudumDare59.Systems;
using LudumDare59.UI;

public partial class Main : Node
{
    public override void _Ready()
    {
        RunController runController = GetNode<RunController>("RunController");
        Node2D sectorRoot = GetNode<Node2D>("SectorRoot");
        Node2D objectiveContainer = sectorRoot.GetNode<Node2D>("ObjectiveContainer");
        Node2D hazardContainer = sectorRoot.GetNode<Node2D>("HazardContainer");
        PlayerShip playerShip = GetNode<PlayerShip>("PlayerShip");
        Camera2D camera = GetNode<Camera2D>("Camera2D");
        HudController hud = GetNode<HudController>("HUD");
        ScoreScreenController scoreScreen = GetNode<ScoreScreenController>("ScoreScreen");

        runController.Initialize(playerShip, camera, hud, scoreScreen, sectorRoot, objectiveContainer, hazardContainer);
    }
}
