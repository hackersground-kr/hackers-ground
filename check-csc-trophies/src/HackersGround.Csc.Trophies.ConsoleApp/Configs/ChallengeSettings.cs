namespace HackersGround.Csc.Trophies.ConsoleApp.Configs;

/// <summary>
/// This represents the settings entity for challenges.
/// </summary>
public class ChallengeSettings : ChallengeSettings<ChallengeItemSettings>
{
    /// <summary>
    /// Defines the section name.
    /// </summary>
    public const string Name = "Challenges";
}

/// <summary>
/// This represents the settings entity for challenges.
/// </summary>
public class ChallengeSettings<T> : Dictionary<string, T>
{
}

/// <summary>
/// This represents the settings entity for challenge items.
/// </summary>
public class ChallengeItemSettings : Dictionary<int, List<string>>
{
}