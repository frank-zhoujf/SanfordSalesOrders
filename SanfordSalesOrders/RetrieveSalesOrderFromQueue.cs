using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SanfordProductionOrders.Model;
using SanfordSalesOrders.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanfordSalesOrders
{
    public class RetrieveSalesOrderFromQueue
    {
        [FunctionName("RetrieveSalesOrderFromQueue")]
        public async Task Run([ServiceBusTrigger("salesordersqueue", Connection = "AzureWebJobsServiceBus")] dynamic queueMessage,
            [CosmosDB(
                databaseName: "ProductionOrdersDB",
                containerName: "ProductionOrderContainer",
                Connection = "CosmosConnectionString")] IAsyncCollector<string> productionOrderOut, ILogger log)
        {
            try
            {
                if (string.IsNullOrEmpty(queueMessage.Value?.ToString()))
                {
                    log.LogInformation("Empty sales order.");
                    throw new Exception();
                }

                var salesOrder = JsonConvert.DeserializeObject<SalesOrder>($"{queueMessage.Value}");

                if (salesOrder == null || !ValidSalesOrder(salesOrder, log))
                {
                    throw new Exception();
                }

                var productionOrder = ConvertToProductionOrder(salesOrder);

                var productionOrderJson = JsonConvert.SerializeObject(productionOrder, Formatting.Indented);

                log.LogInformation(productionOrderJson);

                await productionOrderOut.AddAsync(productionOrderJson);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.StackTrace);
                log.LogInformation("Failed message is removed from the queue.");
            }
        }

        private bool ValidSalesOrder(SalesOrder salesOrder, ILogger log)
        {
            var valid = true;

            if (salesOrder.SalesOrderId is null)
            {
                valid = false;
                log.LogInformation($"The sales order Id is missing.");
            }
            
            if (!DateTimeOffset.TryParse(salesOrder.OrderDate.ToString(), out var orderDate))
            {
                valid = false;
                log.LogInformation($"The format of the order date is not valid: {salesOrder.SalesOrderId}");
            }

            if (salesOrder.SalesOrderItems.Count == 0)
            {
                valid = false;
                log.LogInformation($"There are no order itmes for this sales order: {salesOrder.SalesOrderId}");
            }
            else
            {
                foreach (var salesOrderItem in salesOrder.SalesOrderItems)
                {
                    if (!decimal.TryParse(salesOrderItem.OrderedQty.ToString(), out var orderedQty))
                    {
                        valid = false;
                        log.LogInformation($"The ordered qty of {salesOrderItem.ProductId} is not a valid number.");
                    }

                    if (!decimal.TryParse(salesOrderItem.UnitPrice.ToString(), out var unitPrice))
                    {
                        valid = false;
                        log.LogInformation($"The unit price of {salesOrderItem.ProductId} is not a valid number.");
                    }
                }
            }

            return valid;
        }

        private ProductionOrder ConvertToProductionOrder(SalesOrder salesOrder)
        {
            var productionOrder = new ProductionOrder
            {
                ProductionOrderId = Guid.NewGuid().ToString(),
                SalesOrderId = salesOrder.SalesOrderId,
                ProductionStartDate = DateTime.Now.AddDays(1),
                ProductionStatus = ProductionStatus.NotStarted,
                ProductionOrderItems = new List<ProductionOrderItem>()
            };

            productionOrder.ProductionEndDate = productionOrder.ProductionStartDate.AddDays(7);

            foreach (var salesOrderItem in salesOrder.SalesOrderItems)
            {
                productionOrder.ProductionOrderItems.Add(
                    new ProductionOrderItem
                    {
                        ProductId = salesOrderItem.ProductId,
                        ProducedQty = salesOrderItem.OrderedQty,
                        ProductItemStatus = ProductionStatus.NotStarted
                    }
                );
            }

            return productionOrder;
        }
    }
}
