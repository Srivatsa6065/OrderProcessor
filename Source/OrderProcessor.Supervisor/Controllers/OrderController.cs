using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using OrderProcessor.Infra;
using OrderProcessor.Model;

namespace OrderProcessor.Supervisor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {

        private readonly ILogger<OrderController> _logger;
        private readonly OrderQueue _orderQueue;
        private readonly ConfirmationTable _confirmationTable;

        private static int _orderId = 0;

        public OrderController(ILogger<OrderController> logger, OrderQueue orderQueue,ConfirmationTable confirmationTable)
        {
            _logger = logger;
            _orderQueue = orderQueue;
            _confirmationTable = confirmationTable;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string orderText)
        {
            var order = new Order
            {
                OrderId = _orderId++,
                RandomNumber = new Random().Next(1, 10),
                OrderText = orderText
            };
            var command = new ProcessOrderCommand
            {
                Order = order
            };
            _logger.LogDebug($"Send order #{order.OrderId} with random number {order.RandomNumber}");
            _orderQueue.SendMessage(command);

            var processedAgentId = await GetProceedAgentId(order.OrderId);
            
            return Ok(processedAgentId);
        }

        private async Task<string> GetProceedAgentId(int orderId)
        {
            await _confirmationTable.Init();
            var count = 0; 
            while (count++ <10)
            {
                var confirmation = await _confirmationTable.Search(orderId);
                if (confirmation != null)
                    return confirmation.AgentId.ToString();

               await Task.Delay(2 * 1000);
            }

            throw new Exception("No agent found to process the request");
        }
    }
}
