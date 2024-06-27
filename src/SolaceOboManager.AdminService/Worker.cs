using SolaceOboManager.AdminService.SolaceConfig;

namespace SolaceOboManager.AdminService;

public partial class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISolaceConfigurationAgent _solaceConfigurationAgent;

    public Worker(ILogger<Worker> logger, ISolaceConfigurationAgent solaceConfigurationAgent)
    {
        _logger = logger;
        _solaceConfigurationAgent = solaceConfigurationAgent;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _solaceConfigurationAgent.CreateUser("default", new MsgVpnClientUsername { Username = "obomanager", Password = "password", SubscriptionManagerEnabled = true });
        await _solaceConfigurationAgent.CreateUser("default", new MsgVpnClientUsername { Username = "client", Password = "password" });
        await _solaceConfigurationAgent.CreateUser("default", new MsgVpnClientUsername { Username = "publisher", Password = "password" });


        Environment.Exit(0);
    }
}
