using Refit;
using SolaceOboManager.AdminService;
using SolaceOboManager.AdminService.SolaceConfig;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var solaceOptions = builder.Configuration.GetSection("SolaceConfiguration").Get<SolaceConfigurationOptions>();

builder.Services.AddRefitClient<ISolaceConfigurationAgent>()
    .ConfigureHttpClient(c => 
    { 
        c.BaseAddress = new Uri("http://localhost:8080");
        c.DefaultRequestHeaders.Clear();
        var authenticationString = $"{solaceOptions.Username}:{solaceOptions.Password}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
        c.DefaultRequestHeaders.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");

    });

var host = builder.Build();
host.Run();
