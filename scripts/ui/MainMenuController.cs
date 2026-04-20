namespace LudumDare59.UI;

using System;

using Godot;

public partial class MainMenuController : CanvasLayer
{
    private Button _startButton = null!;
    private Button _quitButton = null!;

    public event Action? StartRequested;

    public event Action? QuitRequested;

    public override void _Ready()
    {
        _startButton = GetNode<Button>("%StartButton");
        _quitButton = GetNode<Button>("%QuitButton");

        _startButton.Pressed += OnStartPressed;
        _quitButton.Pressed += OnQuitPressed;
    }

    public override void _ExitTree()
    {
        if (_startButton is not null)
        {
            _startButton.Pressed -= OnStartPressed;
        }

        if (_quitButton is not null)
        {
            _quitButton.Pressed -= OnQuitPressed;
        }
    }

    public void ShowMenu()
    {
        Visible = true;
        _startButton.CallDeferred("grab_focus");
    }

    public void HideMenu()
    {
        Visible = false;
    }

    private void OnStartPressed()
    {
        StartRequested?.Invoke();
    }

    private void OnQuitPressed()
    {
        QuitRequested?.Invoke();
    }
}
