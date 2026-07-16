using Allegory.Axiom.Hosting;
using Allegory.Axiom.OpenTelemetry;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
app.EnsureTracingStarted();
await app.InitializeApplicationAsync();
await app.RunAsync();