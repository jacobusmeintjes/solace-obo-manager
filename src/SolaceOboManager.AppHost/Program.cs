var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SolaceOboManager_Manager>("solaceobomanager-manager");


builder.AddProject<Projects.SolaceOboManager_Client>("solaceobomanager-client");


builder.AddProject<Projects.SolaceOboManager_Producer>("solaceobomanager-producer");


builder.Build().Run();
