var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres", "18")
    .WithDataVolume();
var siteContentDatabase = postgres.AddDatabase("sitecontent");

var apiService = builder.AddProject<Projects.Gestionnaire_ApiService>("apiservice")
    .WithReference(siteContentDatabase)
    .WaitFor(siteContentDatabase)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Gestionnaire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
