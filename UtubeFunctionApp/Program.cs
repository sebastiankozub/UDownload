using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((host, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();


        services.AddAzureClients(clientBuilder =>
        {
            var str1 = host.Configuration.GetSection("AzuriteStorageConnection");
            clientBuilder.AddBlobServiceClient(host.Configuration.GetSection("AzuriteStorageConnection"))
                .WithName("ApiFetchAndCache");

            var str2 = host.Configuration.GetSection("AzuriteStorageConnection");
            clientBuilder.AddTableServiceClient(host.Configuration.GetSection("AzuriteStorageConnection"));
        });


    })
    .Build();

host.Run();
