using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using HackersGround.Workshop.Challenge.ConsoleApp.Models;
using HackersGround.Workshop.Challenge.ConsoleApp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace HackersGround.Workshop.Challenge.ConsoleApp.Services;

/// <summary>
/// This provides interfaces to the <see cref="ChallengeCheckerService"/> class.
/// </summary>
public interface IChallengeCheckerService
{
    /// <summary>
    /// Runs the trophy checker service.
    /// </summary>
    /// <param name="args">List of arguments.</param>
    Task RunAsync(string[] args);
}

/// <summary>
/// This represents the workshop challenge checker service entity.
/// </summary>
/// <param name="http"><see cref="HttpClient"/> instance.</param>
/// <param name="logger"><see cref="ILogger{TCategoryName}"/> instance.</param>
public class ChallengeCheckerService(HttpClient http, ILogger<ChallengeCheckerService> logger) : IChallengeCheckerService
{
    private const string YOUTUBELINK = "https://youtu.be/NN4Zzp-vOrU";

    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly ILogger<ChallengeCheckerService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

#pragma warning disable IDE1006 // Naming Styles

    private static readonly List<Exception> exceptions = new()
    {
        new ArgumentException("TEST: No challenge code identified. It MUST be 'WORKSHOP'."),
        new ArgumentException("TEST: No frontend app URL."),
        new ArgumentException("TEST: No backend app URL."),
        new ArgumentException("TEST: No dashboard app URL."),
        new ArgumentException("TEST: Invalid app URL. It MUST end with '.azurecontainerapps.io'."),
        new ArgumentException("TEST: No frontend app found."),
        new ArgumentException("TEST: No backend app found."),
        new ArgumentException("TEST: No dashboard app found."),
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
                throw new ArgumentException("No challenge code identified. It MUST be WORKSHOP.");
            }

            if (string.IsNullOrWhiteSpace(options.FrontendAppUrl) == true)
            {
                throw new ArgumentException("No Frontend app URL.");
            }

            if (string.IsNullOrWhiteSpace(options.BackendAppUrl) == true)
            {
                throw new ArgumentException("No Backend app URL.");
            }

            if (string.IsNullOrWhiteSpace(options.DashboardAppUrl) == true)
            {
                throw new ArgumentException("No Dashboard app URL.");
            }

            if (options.FrontendAppUrl!.TrimEnd('/').EndsWith(".azurecontainerapps.io") == false ||
                options.BackendAppUrl!.TrimEnd('/').EndsWith(".azurecontainerapps.io") == false ||
                options.DashboardAppUrl!.TrimEnd('/').EndsWith(".azurecontainerapps.io") == false)
            {
                throw new ArgumentException("Invalid app URL. It MUST end with '.azurecontainerapps.io'.");
            }

            // Check frontend app
            //this._logger.LogInformation($"Verifying frontend app...");

            var frontend = await this.CheckFrontendAppAsync(options);
            this._logger.LogInformation($"Frontend app: {frontend}");
            if (frontend == ChallengeStatusType.Invalid)
            {
                throw new ArgumentException("No frontend app found.");
            }

            // Check backend app
            //this._logger.LogInformation($"Verifying backend app...");

            var backend = await this.CheckBackendAppAsync(options);
            this._logger.LogInformation($"Backend app: {frontend}");
            if (backend == ChallengeStatusType.Invalid)
            {
                throw new ArgumentException("No backend app found.");
            }

            // Check dashboard app
            //this._logger.LogInformation($"Verifying dashboard app...");

            var dashboard = await this.CheckDashboardAppAsync(options);
            this._logger.LogInformation($"Dashboard app: {frontend}");
            if (dashboard == ChallengeStatusType.Invalid)
            {
                throw new ArgumentException("No dashboard app found.");
            }

            payload.ChallengeStatus = (frontend == ChallengeStatusType.Completed &&
                                       backend == ChallengeStatusType.Completed &&
                                       dashboard == ChallengeStatusType.Completed)
                ? ChallengeStatusType.Completed
                : ChallengeStatusType.NotCompleted;

            payload.Message = payload.ChallengeStatus == ChallengeStatusType.Completed
                ? "Workshop challenge completed"
                : $"Workshop challenge NOT completed. Frontend: {frontend}, Backend: {backend}, Dashboard: {dashboard}";

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }
        catch (ArgumentException ex)
        {
            payload.ChallengeCode = options.ChallengeCode.GetValueOrDefault();
            payload.ChallengeStatus = ChallengeStatusType.Invalid;
            payload.Message = ex.Message;

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }
        catch (TimeoutException ex)
        {
            payload.ChallengeCode = options.ChallengeCode.GetValueOrDefault();
            payload.ChallengeStatus = ChallengeStatusType.NotCompleted;
            payload.Message = ex.Message.Replace("\n", "").Replace("\r", "").Replace("\\", "").Replace(" ", "");

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }
        catch (Exception ex)
        {
            payload.ChallengeCode = options.ChallengeCode.GetValueOrDefault();
            payload.ChallengeStatus = ChallengeStatusType.Failed;
            payload.Message = ex.Message;

            Console.WriteLine(JsonSerializer.Serialize(payload, jso));
        }

        await Task.CompletedTask;
    }

    private async Task<ChallengeStatusType> CheckFrontendAppAsync(ArgumentOptions options)
    {
        using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
        await using var browser = await playwright.Chromium.LaunchAsync().ConfigureAwait(false);
        var page = await browser.NewPageAsync();

        await page.GotoAsync(options.FrontendAppUrl!).ConfigureAwait(false);

        await page.WaitForTimeoutAsync(5000).ConfigureAwait(false);

        await page.Locator("input[id='youtube-link']").FillAsync(YOUTUBELINK).ConfigureAwait(false);
        await page.Locator("select[id='video-language-code']").SelectOptionAsync("English").ConfigureAwait(false);
        await page.Locator("select[id='summary-language-code']").SelectOptionAsync("Korean").ConfigureAwait(false);
        await page.Locator("button[id='summary']").ClickAsync().ConfigureAwait(false);

        await page.WaitForTimeoutAsync(90000).ConfigureAwait(false);

        var answer = await page.Locator("textarea[id='result']").TextContentAsync().ConfigureAwait(false);
        var result = string.IsNullOrWhiteSpace(answer) == false
                        ? ChallengeStatusType.Completed
                        : ChallengeStatusType.NotCompleted;

        //this._logger.LogInformation($"Frontend app checked: {result}");
        return result;
    }

    private async Task<ChallengeStatusType> CheckBackendAppAsync(ArgumentOptions options)
    {

        var response = await _http.GetAsync($"{options.BackendAppUrl!.TrimEnd('/')}/weatherforecast").ConfigureAwait(false);
        var answer = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var weatherData = JsonSerializer.Deserialize<List<WeatherForecast>>(answer);

        var result = response.IsSuccessStatusCode
            ? (weatherData?.Count == 5
                ? ChallengeStatusType.Completed
                : ChallengeStatusType.NotCompleted)
            : (response.StatusCode == HttpStatusCode.InternalServerError
                ? ChallengeStatusType.Invalid
                : ChallengeStatusType.Failed);

        //this._logger.LogInformation($"Backend app checked: {result}");
        return result;
    }

    private async Task<ChallengeStatusType> CheckDashboardAppAsync(ArgumentOptions options)
    {
        var response = await _http.GetAsync(options.DashboardAppUrl!).ConfigureAwait(false);


        var result = response.IsSuccessStatusCode
            ? ChallengeStatusType.Completed
            : response.StatusCode < HttpStatusCode.InternalServerError
                ? ChallengeStatusType.Invalid
                : ChallengeStatusType.Failed;

        //this._logger.LogInformation($"Dashboard app checked: {result}");

        return result;
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
        public int TemperatureF { get; set; }
    }

    private void DisplayHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  -c, -code, --challenge-code <challenge-code>   Challenge Code to check the workshop challenge. Possible values are 'WORKSHOP'");
        Console.WriteLine("  -f, --frontend-url <frontend-url>              Frontend app URL. It MUST end with '.azurecontainerapps.io'");
        Console.WriteLine("  -b, --backend-url <backend-url>                Backend app URL. It MUST end with '.azurecontainerapps.io'");
        Console.WriteLine("  -d, --dashboard-url <dashboard-url>            Frontend app URL. It MUST end with '.azurecontainerapps.io'");
        Console.WriteLine("  --force-error                                  Force error");
        Console.WriteLine("  --error-code <error-code>                      Error code. It should be 0 to 8. Default is 0.");
        Console.WriteLine("  -h, --help                                     Display help");
    }
}
