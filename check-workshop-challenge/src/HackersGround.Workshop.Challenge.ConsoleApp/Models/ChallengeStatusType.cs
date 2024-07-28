namespace HackersGround.Workshop.Challenge.ConsoleApp.Models;

/// <summary>
/// This defines the trophy verification status type.
/// </summary>
public enum ChallengeStatusType
{
    /// <summary>
    /// Indicates the status is invalid.
    /// </summary>
    Invalid,

    /// <summary>
    /// Indicates the status is completed.
    /// </summary>
    Completed,

    /// <summary>
    /// Indicates the status is not completed.
    /// </summary>
    NotCompleted,

    /// <summary>
    /// Indicates the status is failed.
    /// </summary>
    Failed,
}