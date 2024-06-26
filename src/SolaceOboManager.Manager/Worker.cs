using SolaceOboManager.Shared;
using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace SolaceOboManager.Manager;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private ISession _session;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initialize Solace Systems Messaging API with logging to console at Warning level
        ContextFactoryProperties cfp = new ContextFactoryProperties()
        {
            SolClientLogLevel = SolLogLevel.Warning
        };
        cfp.LogToConsoleError();
        ContextFactory.Instance.Init(cfp);

        IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null);

        // Create session properties
        SessionProperties sessionProps = new SessionProperties()
        {
            Host = "tcp://localhost:55554",
            VPNName = "default",
            UserName = "obomanager",
            Password = "password"
        };

        _session = context.CreateSession(sessionProps, messageEventHander, sessionEventHandler);

        ReturnCode returnCode = _session.Connect();
        if (returnCode == ReturnCode.SOLCLIENT_OK)
        {
            Console.WriteLine("Session successfully connected.");
        }
        else
        {
            Console.WriteLine("Error connecting, return code: {0}", returnCode);
        }

        // This is the topic on Solace messaging router where a message is published
        // Must subscribe to it to receive messages
        _session.Subscribe(ContextFactory.Instance.CreateTopic("subscriptionRequest"), true);
    }

    private void sessionEventHandler(object? sender, SessionEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private void messageEventHander(object? sender, MessageEventArgs e)
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<SubscriptionRequest>(Encoding.ASCII.GetString(e.Message.BinaryAttachment));
        var instance = ContextFactory.Instance;
        var client = instance.CreateClientName(request.ClientName);

        var topic = instance.CreateTopic("clientSubscribeTopic");

        var returnCode = _session.Subscribe(client, topic, 0, null);

        if(returnCode == ReturnCode.SOLCLIENT_OK)
        {
            Console.WriteLine("Client subscribed to topic successfully!");
        }

    }
}

