using SolaceOboManager.Shared;
using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace SolaceOboManager.Client;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000);
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
            UserName = "client",
            Password = "password"
        };

        var session = context.CreateSession(sessionProps, messageEventHander, sessionEventHandler);

        ReturnCode returnCode = session.Connect();
        if (returnCode == ReturnCode.SOLCLIENT_OK)
        {
            Console.WriteLine("Session successfully connected.");
        }
        else
        {
            Console.WriteLine("Error connecting, return code: {0}", returnCode);
        }

        RequestSubscription(session);
    }

    private void RequestSubscription(ISession session)
    {
        
        using (IMessage requestMessage = ContextFactory.Instance.CreateMessage())
        {
            requestMessage.Destination = ContextFactory.Instance.CreateTopic("subscriptionRequest");
            // Create the request content as a binary attachment

            var request = new SubscriptionRequest { Username = session.Properties.ClientName };
            requestMessage.BinaryAttachment = Encoding.ASCII.GetBytes(System.Text.Json.JsonSerializer.Serialize(request));

            // Send the request message to the Solace messaging router
            Console.WriteLine("Sending request...");
            ReturnCode returnCode = session.Send(requestMessage);
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                // Expecting reply as a binary attachment
                Console.WriteLine("Subscription request sent successful!");
            }
            else
            {
                Console.WriteLine("Request failed, return code: {0}", returnCode);
            }
        }        
    }

    private void sessionEventHandler(object? sender, SessionEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private int _messageCount = 0;

    private void messageEventHander(object? sender, MessageEventArgs e)
    {
        Console.WriteLine(Encoding.ASCII.GetString(e.Message.BinaryAttachment));
        _messageCount++;

        //if(_messageCount > 10)
        //{
        //    Environment.Exit(0);
        //}
    }
}
