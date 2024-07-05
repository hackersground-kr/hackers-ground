using System.Collections.Generic;

namespace HackersGround.Csc.Trophies.ConsoleApp.Configs;

/// <summary>
/// This represents the settings entity for challenges.
/// </summary>
public class ChallengeSettings : Dictionary<string, List<string>>
{
    /// <summary>
    /// Defines the section name.
    /// </summary>
    public const string Name = "Challenges";
}
