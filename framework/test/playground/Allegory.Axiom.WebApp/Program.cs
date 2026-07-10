using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
await app.InitializeApplicationAsync();
await app.RunAsync();