namespace LudumDare59.UI;

using Godot;

using LudumDare59.Data;

public partial class ScoreScreenController : CanvasLayer
{
    private Label _titleLabel = null!;
    private Label _timeLabel = null!;
    private Label _damageLabel = null!;
    private Label _hullLabel = null!;
    private Label _sectorsLabel = null!;
    private Label _scoreLabel = null!;
    private Label _promptLabel = null!;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("Root/Panel/VBox/TitleLabel");
        _timeLabel = GetNode<Label>("Root/Panel/VBox/TimeLabel");
        _damageLabel = GetNode<Label>("Root/Panel/VBox/DamageLabel");
        _hullLabel = GetNode<Label>("Root/Panel/VBox/HullLabel");
        _sectorsLabel = GetNode<Label>("Root/Panel/VBox/SectorsLabel");
        _scoreLabel = GetNode<Label>("Root/Panel/VBox/ScoreLabel");
        _promptLabel = GetNode<Label>("Root/Panel/VBox/PromptLabel");
        HideScreen();
    }

    public void ShowResult(RunResult result)
    {
        int totalSeconds = Mathf.Max(0, (int)result.FinalTimeSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _titleLabel.Text = result.ResultTitle;
        _timeLabel.Text = $"Time: {minutes:00}:{seconds:00}";
        _damageLabel.Text = $"Damage Taken: {result.DamageTaken:0}";
        _hullLabel.Text = $"Hull Remaining: {result.HullRemaining:0}";
        _sectorsLabel.Text = $"Sectors Cleared: {result.SectorsCleared}";
        _scoreLabel.Text = $"Score: {result.FinalScore}";
        _promptLabel.Text = "Press R to restart";
        Visible = true;
    }

    public void HideScreen()
    {
        Visible = false;
    }
}
