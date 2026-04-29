using System;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Assembly1;

public class Assembly1Package : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        Console.WriteLine("Assembly1Package configure executed");
        return Task.CompletedTask;
    }
}