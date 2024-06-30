using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace SolaceOboManager.Producer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000);

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
            UserName = "publisher",
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
        var count = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create the message
            using (IMessage message = ContextFactory.Instance.CreateMessage())
            {
                message.Destination = ContextFactory.Instance.CreateTopic("clientSubscribeTopic");
                // Create the message content as a binary attachment
                message.BinaryAttachment = Encoding.ASCII.GetBytes($"Sample Message - {count}");
                
                message.ElidingEligible = true;
                // Publish the message to the topic on the Solace messaging router
                Console.WriteLine($"Publishing message...{count}");
                returnCode = session.Send(message);
                if (returnCode == ReturnCode.SOLCLIENT_OK)
                {
                    Console.WriteLine("Done.");
                }
                else
                {
                    Console.WriteLine("Publishing failed, return code: {0}", returnCode);
                }
            }
            count++;
            await Task.Delay(500);

            //if(count > 20) 
            //{ 
            //    Environment.Exit(0);
            //}
        }
    }

    private void sessionEventHandler(object? sender, SessionEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private void messageEventHander(object? sender, MessageEventArgs e)
    {

    }
}
