namespace HackersGround.Csc.Trophies.ConsoleApp.Models;

/// <summary>
/// This represents the model entity for the challenge result.
/// </summary>
public class ChallengeResultModel
{
    /// <summary>
    /// Gets or sets the challenge code.
    /// </summary>
    public ChallengeCodeType ChallengeCode { get; set; }

    /// <summary>
    /// Gets or sets the challenge status.
    /// </summary>
    public ChallengeStatusType ChallengeStatus { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string? Message { get; set; }
}
