using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HackersGround.Csc.Trophies.ConsoleApp.Configs;
using HackersGround.Csc.Trophies.ConsoleApp.Models;
using HackersGround.Csc.Trophies.ConsoleApp.Options;

using Microsoft.Playwright;

namespace HackersGround.Csc.Trophies.ConsoleApp.Services;

/// <summary>
/// This provides interfaces to the <see cref="TrophyCheckerService"/> class.
/// </summary>
public interface ITrophyCheckerService
{
    /// <summary>
    /// Runs the trophy checker service.
    /// </summary>
    /// <param name="args">List of arguments.</param>
    Task RunAsync(string[] args);
}

/// <summary>
/// This represents the trophy checker service entity.
/// </summary>
/// <param name="settings"><see cref="ChallengeSettings"/> instance.</param>
public class TrophyCheckService(ChallengeSettings settings) : ITrophyCheckerService
{
    private readonly ChallengeSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
#pragma warning disable IDE1006 // Naming Styles
    private static readonly List<Exception> exceptions = new()
    {
        new ArgumentException("TEST: No challenge code identified. It MUST be either AZ-900 or AI-900"),
        new ArgumentException("TEST: No Microsoft Learn profile URL."),
        new ArgumentException("TEST: Invalid Microsoft Learn profile URL. It MUST start with https://learn.microsoft.com/ko-kr/users/."),
        new ArgumentException("TEST: No trophies found."),
        new Exception("TEST: An error occurred."),
    };
#pragma warning restore IDE1006 // Naming Styles

    private static JsonSerializerOptions jso => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    /// <inheritdoc />
    public async Task RunAsync(string[] args)
    {
        var options = ArgumentOptions.Parse(args);
        if (options.Help)
        {
            this.DisplayHelp();
            return;
        }

        var payload = new ChallengeResultModel()
        {
            ChallengeCode = options.ChallengeCode.GetValueOrDefault()
        };

        try
        {
            if (options.ForceError == true)
            {
                var index = options.ErrorCode.GetValueOrDefault() % exceptions.Count;
                throw exceptions[index];
            }

            if (options.ChallengeCode == ChallengeCodeType.Undefined)
            {
                throw new ArgumentException("No challenge code identified. It MUST be either AZ-900 or AI-900");
            }

            if (string.IsNullOrWhiteSpace(options.MicrosoftLearnProfileUrl) == true)
            {
                throw new ArgumentException("No Microsoft Learn profile URL.");
            }

            if (options.MicrosoftLearnProfileUrl.StartsWith("https://learn.microsoft.com/ko-kr/users/") == false)
            {
                throw new ArgumentException("Invalid Microsoft Learn profile URL. It MUST start with https://learn.microsoft.com/ko-kr/users/.");
            }

            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
            await using var browser = await playwright.Chromium.LaunchAsync().ConfigureAwait(false);
            var page = await browser.NewPageAsync();

            await page.GotoAsync(options.MicrosoftLearnProfileUrl).ConfigureAwait(false);

            await page.WaitForTimeoutAsync(5000).ConfigureAwait(false);

            var titles = await page.Locator("section[id='trophies-section']")
                                   .Locator("a[class='card-content-title']")
                                   .AllAsync().ConfigureAwait(false);
            if (titles.Any() == false)
            {
                throw new ArgumentException("No trophies found.");
            }

            List<string> trophies = [];
            foreach (var title in titles)
            {
                var trophy = await title.Locator("h3").TextContentAsync().ConfigureAwait(false);
#pragma warning disable CS8604 // Possible null reference argument.
                trophies.Add(trophy);
#pragma warning restore CS8604 // Possible null reference argument.
            }

#pragma warning disable CS8604 // Possible null reference argument.
            var modules = this._settings[options.ChallengeCode.ToString()];
#pragma warning restore CS8604 // Possible null reference argument.
            List<string> complete = [];
            foreach (var module in modules)
            {
                if (trophies.Contains(module) == true)
                {
                    complete.Add(module);
                }
            }

            payload.ChallengeStatus = modules.Count == complete.Count
                ? ChallengeStatusType.Completed
                : ChallengeStatusType.NotCompleted;

            payload.Message = payload.ChallengeStatus == ChallengeStatusType.Completed
                ? "All modules are completed"
                : $"Not all modules are completed. Missing modules: {string.Join(", ", modules.Except(complete))}";

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }
        catch (ArgumentException ex)
        {
            payload.ChallengeCode = options.ChallengeCode.GetValueOrDefault();
            payload.ChallengeStatus = ChallengeStatusType.Invalid;
            payload.Message = ex.Message;

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }
        catch (Exception ex)
        {
            payload.ChallengeCode = options.ChallengeCode.GetValueOrDefault();
            payload.ChallengeStatus = ChallengeStatusType.Failed;
            payload.Message = ex.Message;

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }
    }

    private void DisplayHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  -c, -code, --challenge-code <challenge-code>   Challenge Code to check trophies. Possible values are 'AZ-900' or 'AI-900'");
        Console.WriteLine("  -u, -url, --profile-url <profile-url>          Microsoft Learn Profile URL. It MUST start with 'https://learn.microsoft.com/ko-kr/users/'");
        Console.WriteLine("  --force-error                                  Force error");
        Console.WriteLine("  --error-code <error-code>                      Error code. It should be 0 to 4. Default is 0.");
        Console.WriteLine("  -h, --help                                     Display help");
    }
}
