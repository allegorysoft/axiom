using Allegory.Axiom.Hosting;
using Allegory.Axiom.OpenTelemetry;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
app.EnsureTracingStarted(); // https://github.com/allegorysoft/axiom/issues/83
await app.InitializeApplicationAsync();
await app.RunAsync();