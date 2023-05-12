# SanfordSalesOrders

This solution contains two Azure Functions (.Net 6 and Azure Function V4):
1. SendSalesOrderToQueue: it's a Http trigger function, output binding is service bus queue. It will be triggered by Http request and send a message (sales order) to Azure service bus queue.
2. RetrieveSalesOrderFromQueue: it's a Service Bus Trigger function, this function listens to the service bus queue and will process the message (sales order) as long as it's been sent to the queue (validation and convert to production order), output binding is Azure Cosmos DB, the converted production order will be inserted into there at the end.

Instructions to run this application:
1. In order to run this application locally, you need to create local.settings.json file in the same folder as host.json with the content as below:
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsServiceBus": "",
    "CosmosConnectionString": ""
  }
}
2. Create service bus queue namespace and new queue 'salesordersqueue' in Azure Portal
3. Under the service bus namespace, go to 'Shared access policies' and click 'RootManageSharedAccessKey', copy the 'Primary Connection String' and paste it in the local.settings.json (Values -> AzureWebJobsServiceBus)
4. Create Cosmos DB account in Azure Portal and also create couple of things below:
databaseName: ProductionOrdersDB
containerName: ProductionOrderContainer
5. Go to the Cosmos DB account just created, click 'Keys' under 'Settings' and copy the 'PRIMARY CONNECTION STRING', paste it in the local.settings.json (Values -> CosmosConnectionString)
6. Build and run the application in Visual Studio locally and waiting for the console window pop up
7. Copy the url from there, the URL is something like 'http://localhost:7291/api/SendSalesOrderToQueue', do a post request on that URL in Postman with the body as below:

{
	"salesOrderId" : "1234",
	"customer" : "John Dee",
	"orderdate" : "2023-05-11T16:48:23+12:00",
	"salesOrderItems" : [
		{
			"productid" : "ABC",
			"orderedqty" : 12.0,
			"unitprice" : 3.21
		},
		{
			"productid" : "XYZ",
			"orderedqty" : 8.0,
			"unitprice" : 12.34
		}
	]
}

8. You should be able to see the production order in the Cosmos DB if everything is running fine (either in Azure Portal or add Cosmos DB service dependency in Visual Studio), the production order is in JSON format and looks something like this:

{
    "id": "df7a0a24-eb4e-4a48-a31e-408eece94fd3",
    "salesOrderId": "1234",
    "productionStartDate": "2023-05-14T00:52:21.6099307+12:00",
    "productionEndDate": "2023-05-21T00:52:21.6099307+12:00",
    "productionStatus": 0,
    "productionOrderItems": [
        {
            "productId": "ABC",
            "producedQty": 12,
            "productItemStatus": 0
        },
        {
            "productId": "XYZ",
            "producedQty": 8,
            "productItemStatus": 0
        }
    ],
    "_rid": "enZFAO6P3QYDAAAAAAAAAA==",
    "_self": "dbs/enZFAA==/colls/enZFAO6P3QY=/docs/enZFAO6P3QYDAAAAAAAAAA==/",
    "_etag": "\"a101bf21-0000-0700-0000-645e36870000\"",
    "_attachments": "attachments/",
    "_ts": 1683895943
}

9. Feel free to test different sales orders, you should be able to see the error messages through the console window if it's an invalid sales order and this order will be deleted from the queue after the execution is done.

If I got more time, I will probably add the front end UI with Web API to send the message to service bus queue to increase the user experience and use the cosmos db client through dependency injection instead of the output binding.
