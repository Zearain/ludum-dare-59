namespace LudumDare59.Components;

using System;

using Godot;

public partial class ShipHullComponent : Node
{
    [Export]
    public float MaxHull { get; set; } = 100.0f;

    public float CurrentHull { get; private set; }

    public float TotalDamageTaken { get; private set; }

    public bool IsDepleted => CurrentHull <= 0.0f;

    public event Action<float, float>? HullChanged;

    public event Action? HullDepleted;

    public override void _Ready()
    {
        ResetHull();
    }

    public void ResetHull()
    {
        CurrentHull = MaxHull;
        TotalDamageTaken = 0.0f;
        HullChanged?.Invoke(CurrentHull, MaxHull);
    }

    public void ApplyDamage(float amount)
    {
        if (amount <= 0.0f || IsDepleted)
        {
            return;
        }

        float previousHull = CurrentHull;
        CurrentHull = Mathf.Max(0.0f, CurrentHull - amount);
        TotalDamageTaken += Mathf.Max(0.0f, previousHull - CurrentHull);
        HullChanged?.Invoke(CurrentHull, MaxHull);

        if (CurrentHull <= 0.0f)
        {
            HullDepleted?.Invoke();
        }
    }
}
