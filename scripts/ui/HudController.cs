namespace LudumDare59.UI;

using Godot;

using LudumDare59.Data;

public partial class HudController : CanvasLayer
{
    private Label _timerLabel = null!;
    private Label _hullLabel = null!;
    private ProgressBar _hullBar = null!;
    private Label _scannerLabel = null!;
    private ProgressBar _scannerBar = null!;
    private Label _signalLabel = null!;
    private ProgressBar _signalBar = null!;
    private Label _objectiveLabel = null!;
    private Label _stateLabel = null!;

    public override void _Ready()
    {
        _timerLabel = GetNode<Label>("Root/Panel/VBox/TimerLabel");
        _hullLabel = GetNode<Label>("Root/Panel/VBox/HullLabel");
        _hullBar = GetNode<ProgressBar>("Root/Panel/VBox/HullBar");
        _scannerLabel = GetNode<Label>("Root/Panel/VBox/ScannerLabel");
        _scannerBar = GetNode<ProgressBar>("Root/Panel/VBox/ScannerBar");
        _signalLabel = GetNode<Label>("Root/Panel/VBox/SignalLabel");
        _signalBar = GetNode<ProgressBar>("Root/Panel/VBox/SignalBar");
        _objectiveLabel = GetNode<Label>("Root/Panel/VBox/ObjectiveLabel");
        _stateLabel = GetNode<Label>("Root/Panel/VBox/StateLabel");
    }

    public void SetTimer(double elapsedSeconds)
    {
        int totalSeconds = Mathf.Max(0, (int)elapsedSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        _timerLabel.Text = $"Time: {minutes:00}:{seconds:00}";
    }

    public void SetHull(float currentHull, float maxHull)
    {
        _hullBar.MaxValue = maxHull;
        _hullBar.Value = currentHull;
        _hullLabel.Text = $"Hull: {currentHull:0}/{maxHull:0}";
    }

    public void SetScanner(float cooldownRemaining, float cooldownDuration)
    {
        float progress = cooldownDuration <= 0.0f
            ? 1.0f
            : Mathf.Clamp(1.0f - (cooldownRemaining / cooldownDuration), 0.0f, 1.0f);

        _scannerBar.MaxValue = 1.0f;
        _scannerBar.Value = progress;

        _scannerLabel.Text = cooldownRemaining <= 0.0f
            ? "Scanner: Ready"
            : $"Scanner: {cooldownRemaining:0.0}s";
    }

    public void SetSignal(SignalReading reading)
    {
        float angleDegrees = Mathf.RadToDeg(reading.Direction.Angle());
        if (angleDegrees < 0.0f)
        {
            angleDegrees += 360.0f;
        }

        _signalBar.MaxValue = 1.0f;
        _signalBar.Value = reading.Strength;
        _signalLabel.Text = $"Signal: {angleDegrees:000} deg | {reading.Strength:P0}";
    }

    public void ClearSignal()
    {
        _signalBar.MaxValue = 1.0f;
        _signalBar.Value = 0.0f;
        _signalLabel.Text = "Signal: --";
    }

    public void SetObjective(string objectiveText)
    {
        _objectiveLabel.Text = $"Objective: {objectiveText}";
    }

    public void SetRunState(string stateText)
    {
        _stateLabel.Text = stateText;
    }
}
