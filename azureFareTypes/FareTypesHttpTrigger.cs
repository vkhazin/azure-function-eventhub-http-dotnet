using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace azureFareTypes
{
    public class FareTypesHttpTrigger
    {
        public FareTypesDbContext FareTypesDb;

        public FareTypesHttpTrigger(FareTypesDbContext db)
        {
            FareTypesDb = db;
        }

        [FunctionName("FareTypesHttpTriggerGet")]
        public async Task<IActionResult> RunGet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "farerates/{id}")] HttpRequest req,
            int id,
            ILogger log)
        {
            var cosmosDb = new FareTypesCosmosDb();
            await cosmosDb.Init();

            var fareType = await cosmosDb.Get(id);
            return fareType != null
                ? (ActionResult)new OkObjectResult(fareType)
                : new NotFoundResult();
        }

        [FunctionName("FareTypesHttpTriggerGetAll")]
        public async Task<IActionResult> RunGetAll(
            [HttpTrigger(AuthorizationLevel.Function, "get", "delete", "put", "patch", Route = "farerates")] HttpRequest req,
            ILogger log)
        {
            var cosmosDb = new FareTypesCosmosDb();
            await cosmosDb.Init();

            return req.Method.ToLower() == "get"
                ? (ActionResult)new OkObjectResult(cosmosDb.GetAll())
                : new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        [FunctionName("FareTypesHttpTriggerDelete")]
        public async Task<IActionResult> RunDelete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "farerates/{id}")] HttpRequest req,
            int id,
            ILogger log)
        {
            return new StatusCodeResult(await FareTypesDb.Delete(id)
                ? StatusCodes.Status200OK
                : StatusCodes.Status404NotFound);
        }

        [FunctionName("FareTypesHttpTriggerPost")]
        public async Task<IActionResult> RunPost(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "farerates")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<FareType>(requestBody);

            if (data == null)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

            return new StatusCodeResult(await FareTypesDb.Insert(data)
                ? StatusCodes.Status201Created
                : StatusCodes.Status409Conflict);
        }

        [FunctionName("FareTypesHttpTriggerPut")]
        public async Task<IActionResult> RunPut(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "farerates/{id}")] HttpRequest req,
            int id,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<FareType>(requestBody);

            if (data == null || data.FareTypeId != id)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

            return new StatusCodeResult(await FareTypesDb.Delete(id) && await FareTypesDb.Insert(data)
                ? StatusCodes.Status200OK
                : StatusCodes.Status404NotFound);
        }

        [FunctionName("FareTypesHttpTriggerPatch")]
        public async Task<IActionResult> RunPatch(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "farerates/{id}")] HttpRequest req,
            int id,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<FareType>(requestBody);

            if (data == null)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

            return new StatusCodeResult(await FareTypesDb.Update(id, data)
                ? StatusCodes.Status200OK
                : StatusCodes.Status404NotFound);
        }
    }
}