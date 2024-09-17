var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CLAPi_ExcelEngine_Api>("clapi-excelengine-api");

await builder.Build().RunAsync();
