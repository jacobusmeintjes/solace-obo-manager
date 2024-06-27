using SolaceOboManager.AppHost.WaitForDependencies;

var builder = DistributedApplication.CreateBuilder(args);

var solace = builder.AddContainer("pubSubStandardSingleNode", "solace/solace-pubsub-standard", "latest")
    .WithVolume("storage-group", "/var/lib/solace")
    .WithContainerRuntimeArgs("--shm-size", "1g", "--ulimit", "core=-1", "--ulimit", "nofile=2448:6592")
    .WithEnvironment("system_scaling_maxconnectioncount", "100")
    .WithEnvironment("username_admin_password", "admin")
    .WithEnvironment("username_admin_globalaccesslevel", "admin")
    .WithHttpEndpoint(8080, 8080, "admin")
    .WithEndpoint(8008, 8008, "ws")
    .WithEndpoint(8000, 8000, "mqtt")
    .WithEndpoint(55554, 55555, "tcp")
    .WithHttpEndpoint(5550, 5550, "health")
    .WithHealthCheck(endpointName: "health", "/health-check/direct-active");

var admin = builder.AddProject<Projects.SolaceOboManager_AdminService>("solacemanager-admin")
    .WithEnvironment("SolaceConfiguration__Username", "admin")
    .WithEnvironment("SolaceConfiguration__Password", "admin")
    .WaitFor(solace);

builder.AddProject<Projects.SolaceOboManager_Manager>("solaceobomanager-manager")
    .WaitForCompletion(admin);


builder.AddProject<Projects.SolaceOboManager_Client>("solaceobomanager-client")
    .WaitForCompletion(admin);


builder.AddProject<Projects.SolaceOboManager_Producer>("solaceobomanager-producer")
    .WaitForCompletion(admin);

builder.Build().Run();
