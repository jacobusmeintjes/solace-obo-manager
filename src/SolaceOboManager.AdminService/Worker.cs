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
        await _solaceConfigurationAgent.CreateClientProfile("default", new MsgVpnClientProfile { ClientProfileName = "clientProfile", ElidingEnabled = false, ElidingDelay = 2000 });

        await _solaceConfigurationAgent.CreateAclProfile("default", new MsgVpnAclProfile { AclProfileName = "clientProfile", ClientConnectDefaultAction = "allow" });
        await _solaceConfigurationAgent.CreatePublishTopicExceptions("default", "clientProfile", new MsgVpnAclProfilePublishTopicException { AclProfileName = "clientProfile", VpnName = "default", PublishTopicException = "subscriptionRequest" });

        await _solaceConfigurationAgent.CreateUser("default", new MsgVpnClientUsername { Username = "obomanager", Password = "password", SubscriptionManagerEnabled = true });
        await _solaceConfigurationAgent.CreateUser("default", new MsgVpnClientUsername { Username = "client", Password = "password", AclProfileName = "clientProfile", ProfileName = "clientProfile" });
        await _solaceConfigurationAgent.CreateUser("default", new MsgVpnClientUsername { Username = "publisher", Password = "password" });


        Environment.Exit(0);
    }
}
