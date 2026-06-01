using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
await app.InitializeApplicationAsync();

app.UseRequestLocalization();
app.UseExceptionHandler();
app.UseUnitOfWork();
app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();

app.Run();