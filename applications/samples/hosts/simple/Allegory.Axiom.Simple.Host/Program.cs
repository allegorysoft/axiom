using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);


await builder.ConfigureApplicationAsync();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers(o =>
{
    o.Filters.Add<UnitOfWorkActionFilter>();
});

var app = builder.Build();

await app.InitializeApplicationAsync();

app.MapGet("/",() => "Hello World!");


app.UseRequestLocalization();
app.UseExceptionHandler();
app.UseUnitOfWork();
app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();
app.MapControllers();

app.Run();
