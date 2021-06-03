using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using OrderProcessor.Model;

namespace OrderProcessor.Infra
{
    public class ConfirmationTable
    {
        private CloudTable _table;

        public async Task Init()
        {
            await InitTable();
        }

        public async Task Inset(Confirmation confirmation)
        {
            var entity = new ConfirmationTableEntity
            {
                AgentId = confirmation.AgentId,
                OrderId = confirmation.OrderId,
                OrderStatus = confirmation.OrderStatus,
                PartitionKey = confirmation.OrderId.ToString(),
                RowKey = confirmation.OrderId.ToString()
            };

            var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            await _table.ExecuteAsync(insertOrMergeOperation);
        }

        private async Task InitTable()
        {
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference("Confirmation");
            await table.CreateIfNotExistsAsync();
            _table = table;
        }

        public async Task<Confirmation> Search(int orderId)
        {

            var retrieveOperation = TableOperation.Retrieve<ConfirmationTableEntity>(orderId.ToString(), orderId.ToString());
            var result = await _table.ExecuteAsync(retrieveOperation);

            if (result.Result is ConfirmationTableEntity confirmationTableEntity)
            {
                return new Confirmation
                {
                    AgentId = confirmationTableEntity.AgentId,
                    OrderId = confirmationTableEntity.OrderId,
                    OrderStatus = confirmationTableEntity.OrderStatus
                };
            }

            return null;
        }
    }

    public class ConfirmationTableEntity : TableEntity
    {
        public int OrderId { get; set; }

        public Guid AgentId { get; set; }

        public string OrderStatus { get; set; }
    }
}
