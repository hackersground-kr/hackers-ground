using HackersGround.Csc.Trophies.ConsoleApp.Configs;
using HackersGround.Csc.Trophies.ConsoleApp.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
               .UseConsoleLifetime()
               .ConfigureServices(services =>
               {
                   var config = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .Build();
#pragma warning disable CS8604 // Possible null reference argument.
                   services.AddSingleton<ChallengeSettings>(config.GetSection(ChallengeSettings.Name)?.Get<ChallengeSettings>());
#pragma warning restore CS8604 // Possible null reference argument.
                   services.AddTransient<ITrophyCheckerService, TrophyCheckService>();
               })
               .Build();

var service = host.Services.GetRequiredService<ITrophyCheckerService>();
await service.RunAsync(args);
