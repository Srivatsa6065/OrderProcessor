using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using OrderProcessor.Infra;
using OrderProcessor.Model;
using static System.Console;
using static System.Threading.Tasks.Task;

namespace OrderProcessor.Agent
{
    public class OrderProcessor
    {
        private readonly OrderQueue _orderQueue;
        private readonly ConfirmationTable _confirmationTable;
        private static readonly Guid AgentId = Guid.NewGuid();
        private static readonly int MagicNumber = new Random().Next(1, 10);

        public OrderProcessor(OrderQueue orderQueue,ConfirmationTable confirmationTable)
        {
            _orderQueue = orderQueue;
            _confirmationTable = confirmationTable;
        }

        public async Task BeginProcessing()
        {
            WriteLine($"I'm agent {AgentId}, my magic number is {MagicNumber}");
            await _confirmationTable.Init();
            await Process();
        }

        private async Task Process()
        {
            while (true)
            {
                if (_orderQueue.CheckForNewMessages())
                {
                    var messages = _orderQueue.GetMessages();
                    foreach (var message in messages)
                    {
                        var shouldContinue = await CommandHandler(message);
                        if (!shouldContinue)
                        {
                            return;
                        }
                    }
                }

                await Delay(2 * 1000);
            }
        }

        private async Task<bool> CommandHandler(QueueMessage message)
        {
            var command = JsonSerializer.Deserialize<ProcessOrderCommand>(message.MessageText);

            WriteLine($"console Received order {command.Order.OrderId}");

            if (command.Order.RandomNumber == MagicNumber)
            {
                WriteLine("Oh no, my magic number was found");
                Read();
                return false;
            }

            WriteLine(command.Order.OrderText);
            var confirmation = new Confirmation
            {
                AgentId = AgentId,
                OrderId = command.Order.OrderId,
                OrderStatus = "Processed"
            };
            await _confirmationTable.Inset(confirmation);
            _orderQueue.DeleteMessage(message);

            return true;
        }
    }
}
