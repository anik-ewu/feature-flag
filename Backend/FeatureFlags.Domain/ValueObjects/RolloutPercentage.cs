namespace FeatureFlags.Domain.ValueObjects;

public sealed record RolloutPercentage
{
    public int Value { get; }

    private RolloutPercentage(int value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Rollout percentage must be between 0 and 100.");
        }
        
        Value = value;
    }

    public static RolloutPercentage Create(int value) => new(value);

    // Implicit conversion makes it easier to work with Entity Framework and logic.
    public static implicit operator int(RolloutPercentage percentage) => percentage.Value;
    public static explicit operator RolloutPercentage(int value) => new(value);
}
