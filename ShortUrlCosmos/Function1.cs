using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ShortUrlCosmos
{
    public static class Function1
    {
        private static readonly string _endpointUrl = System.Environment.GetEnvironmentVariable("endpointUrl");

        private static readonly string _primaryKey = System.Environment.GetEnvironmentVariable("primaryKey");
        private static readonly string _databaseId = "payment_url";
        private static readonly string _containerId = "short_link";
        private static readonly string _partitionKey = "link";
        private static CosmosClient cosmosClient = new CosmosClient(_endpointUrl, _primaryKey);

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{id}")] HttpRequest req,
            string id,
            ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. {id}");
            if (id != null)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var container = cosmosClient.GetContainer(_databaseId, _containerId);
                var url = await container.ReadItemAsync<ShortUrl>(id, new PartitionKey(_partitionKey));
                watch.Stop();

                log.LogInformation($"Looking for: {id} cost us {watch.Elapsed.TotalMilliseconds}");
                if (url != null)
                {
                    return new RedirectResult(url.Resource.Url);
                }
                //TODO: Redirect to 404
                return new NotFoundObjectResult("Result not found.");
            }



            return new OkObjectResult("Id is null");
        }

        class ShortUrl
        {
            public string Id { get; set; }
            public string Url { get; set; }
        }
    }
}
