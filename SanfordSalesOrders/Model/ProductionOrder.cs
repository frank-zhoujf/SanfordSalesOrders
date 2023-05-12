using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SanfordProductionOrders.Model
{
    public enum ProductionStatus { NotStarted, InProgress, Completed, Cancelled };
    public class ProductionOrder
    {
        [JsonProperty("id")]
        public string ProductionOrderId { get; set; }

        [JsonProperty("salesOrderId")]
        public string SalesOrderId { get; set; }

        [JsonProperty("productionStartDate")]
        public DateTimeOffset ProductionStartDate { get; set; }

        [JsonProperty("productionEndDate")]
        public DateTimeOffset ProductionEndDate { get; set; }

        [JsonProperty("productionStatus")]
        public ProductionStatus ProductionStatus { get; set; }

        [JsonProperty("productionOrderItems")]
        public List<ProductionOrderItem> ProductionOrderItems { get; set; }
    }

    public class ProductionOrderItem
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }
        [JsonProperty("producedQty")]
        public decimal ProducedQty { get; set; }
        [JsonProperty("productItemStatus")]
        public ProductionStatus ProductItemStatus { get; set; }
    }
}
