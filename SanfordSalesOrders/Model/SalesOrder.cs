using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SanfordSalesOrders.Model
{
    public class SalesOrder
    {
        [JsonProperty("salesOrderId")]
        public string SalesOrderId { get; set; }
        [JsonProperty("customer")]
        public string Customer { get; set; }
        [JsonProperty("orderDate")]
        public DateTimeOffset OrderDate { get; set; }
        [JsonProperty("salesOrderItems")]
        public List<SalesOrderItem> SalesOrderItems { get; set; }
    }

    public class SalesOrderItem
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }
        [JsonProperty("orderedQty")]
        public decimal OrderedQty { get; set; }
        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }
    }
}
