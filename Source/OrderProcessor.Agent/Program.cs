using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessor.Infra;

namespace OrderProcessor.Agent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<OrderProcessor>()
                .AddSingleton<OrderQueue>()
                .AddSingleton<ConfirmationTable>()
                .BuildServiceProvider();

            var orderProcessor = serviceProvider.GetRequiredService<OrderProcessor>();
            await orderProcessor.BeginProcessing();
        }
    }
}
