using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
await builder.ConfigureApplicationAsync();

var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.UseRequestLocalization();
app.UseExceptionHandler();
app.UseUnitOfWork();
app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();

app.Run();