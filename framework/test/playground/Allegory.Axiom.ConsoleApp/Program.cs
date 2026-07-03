using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
await app.InitializeApplicationAsync();
await app.RunAsync();