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
    private Label _objectiveLabel = null!;
    private Label _stageProgressLabel = null!;
    private Label _activationLabel = null!;
    private ProgressBar _activationBar = null!;
    private Label _sectorLabel = null!;
    private Label _transmissionLabel = null!;
    private Label _stateLabel = null!;
    private Control _scannerOverlay = null!;
    private Control _scannerRing = null!;
    private ColorRect _scannerRingBlip = null!;
    private ColorRect _scannerOuterBlip = null!;

    public override void _Ready()
    {
        _timerLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/TimerLabel");
        _hullLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/HullLabel");
        _hullBar = GetNode<ProgressBar>("Root/Panel/MarginContainer/VBox/HullBar");
        _scannerLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/ScannerLabel");
        _scannerBar = GetNode<ProgressBar>("Root/Panel/MarginContainer/VBox/ScannerBar");
        _objectiveLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/ObjectiveLabel");
        _stageProgressLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/StageProgressLabel");
        _activationLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/ActivationLabel");
        _activationBar = GetNode<ProgressBar>("Root/Panel/MarginContainer/VBox/ActivationBar");
        _sectorLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/SectorLabel");
        _transmissionLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/TransmissionLabel");
        _stateLabel = GetNode<Label>("Root/Panel/MarginContainer/VBox/StateLabel");
        _scannerOverlay = GetNode<Control>("Root/ScannerOverlay");
        _scannerRing = GetNode<Control>("Root/ScannerOverlay/ScannerRing");
        _scannerRingBlip = GetNode<ColorRect>("Root/ScannerOverlay/ScannerRing/ScannerRingBlip");
        _scannerOuterBlip = GetNode<ColorRect>("Root/ScannerOverlay/ScannerOuterBlip");
        ClearScannerPing();
        ClearStageProgress();
        ClearActivation();
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

    public void SetActivation(float activationProgress, bool isPlayerInRange, bool requiresInteract)
    {
        _activationBar.MaxValue = 1.0f;
        _activationBar.Value = activationProgress;
        _activationBar.Visible = isPlayerInRange || activationProgress > 0.0f;
        _activationLabel.Text = isPlayerInRange
            ? (requiresInteract ? "Activation: Hold E / A" : "Activation: In Progress")
            : activationProgress > 0.0f
                ? "Activation: Stabilizing"
                : "Activation: Get Close";
    }

    public void ClearActivation()
    {
        _activationBar.MaxValue = 1.0f;
        _activationBar.Value = 0.0f;
        _activationBar.Visible = false;
        _activationLabel.Text = "Activation: --";
    }

    public void ShowScannerPing(SignalReading reading)
    {
        Vector2 viewportCenter = GetViewport().GetVisibleRect().Size * 0.5f;
        float ringRadius = 44.0f;
        float outerRadius = 112.0f;
        Vector2 direction = reading.Direction.Normalized();

        _scannerOverlay.Visible = true;
        _scannerRing.Visible = true;
        _scannerRing.Position = viewportCenter - (_scannerRing.Size * 0.5f);

        Vector2 ringCenter = _scannerRing.Size * 0.5f;
        Vector2 ringOffset = direction * ringRadius;
        _scannerRingBlip.Visible = true;
        _scannerRingBlip.Position = ringCenter + ringOffset - (_scannerRingBlip.Size * 0.5f);

        float outerSpread = Mathf.Lerp(18.0f, 6.0f, reading.Confidence);
        Vector2 outerDirection = direction.Rotated(Mathf.DegToRad(outerSpread * Mathf.Sin(Time.GetTicksMsec() * 0.01f)));
        _scannerOuterBlip.Visible = true;
        _scannerOuterBlip.Position = viewportCenter + (outerDirection * outerRadius) - (_scannerOuterBlip.Size * 0.5f);
        _scannerOuterBlip.Modulate = new Color(0.35f, 0.95f, 1.0f, Mathf.Lerp(0.55f, 0.95f, reading.Confidence));
    }

    public void ClearScannerPing()
    {
        _scannerOverlay.Visible = false;
        _scannerRing.Visible = false;
        _scannerRingBlip.Visible = false;
        _scannerOuterBlip.Visible = false;
    }

    public void SetObjective(string objectiveText)
    {
        _objectiveLabel.Text = $"Objective: {objectiveText}";
    }

    public void SetStageProgress(int stageNumber, int stageTotal, ObjectiveStageKind? stageKind)
    {
        int clampedStage = Mathf.Clamp(stageNumber, 1, Mathf.Max(1, stageTotal));
        string kindText = stageKind switch
        {
            ObjectiveStageKind.SubRelay => "Sub-Relay",
            ObjectiveStageKind.WarpRelay => "Warp Relay",
            ObjectiveStageKind.WreckPart => "Wreck Part",
            ObjectiveStageKind.BlackBoxExtraction => "Extraction",
            _ => "Objective",
        };
        _stageProgressLabel.Text = $"Stage: {clampedStage}/{Mathf.Max(1, stageTotal)} ({kindText})";
    }

    public void ClearStageProgress()
    {
        _stageProgressLabel.Text = "Stage: --";
    }

    public void SetSector(int sectorNumber, bool isFinalSector)
    {
        _sectorLabel.Text = isFinalSector
            ? $"Sector: Final ({sectorNumber})"
            : $"Sector: Relay {sectorNumber}/5";
    }

    public void SetTransmission(string transmissionText)
    {
        _transmissionLabel.Text = string.IsNullOrWhiteSpace(transmissionText)
            ? "Transmission: --"
            : $"Transmission: {transmissionText}";
    }

    public void SetRunState(string stateText)
    {
        _stateLabel.Text = stateText;
    }

}
