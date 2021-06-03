using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using OrderProcessor.Model;

namespace OrderProcessor.Infra
{
    public class OrderQueue
    {
        private readonly QueueClient _queueClient;

        public OrderQueue()
        {
            _queueClient = new QueueClient("UseDevelopmentStorage=true", "orders");
            _queueClient.CreateIfNotExists();
        }

        public void SendMessage(ProcessOrderCommand command)
        {
            var message = JsonSerializer.Serialize(command);
            _queueClient.SendMessage(message);
        }

        public bool CheckForNewMessages()
        {
            PeekedMessage[] message = _queueClient.PeekMessages();
            return message.Any();
        }

        public IEnumerable<QueueMessage> GetMessages()
        {
            QueueMessage[] message = _queueClient.ReceiveMessages(10);
            return message;
        }
        
        public void DeleteMessage(QueueMessage queueMessage)
        {
            _queueClient.DeleteMessage(queueMessage.MessageId, queueMessage.PopReceipt);
        }
    }
}
