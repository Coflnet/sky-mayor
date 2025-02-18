using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using Coflnet.Sky.Mayor.Attributes;
using Coflnet.Sky.Mayor.Models;
using Coflnet.Sky.Mayor.Services;
using System.Threading.Tasks;
using System.Linq;

namespace Coflnet.Sky.Mayor.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class MayorApiController : ControllerBase
    {
        public MayorService mayorService;

        public MayorApiController(MayorService mayorService)
        {
            this.mayorService = mayorService;
        }
        /// <summary>
        /// Get the the current mayor
        /// </summary>
        /// <remarks>Returns the name of the current mayor</remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [HttpGet]
        [Route("/mayor/current")]
        [ValidateModelState]
        [SwaggerOperation("MayorCurrentGet")]
        [SwaggerResponse(statusCode: 200, type: typeof(ModelCandidate), description: "OK")]
        public virtual async Task<ModelCandidate> MayorCurrentGet()
        {
            var currentElection = await mayorService.GetElectionPeriod(CurrentMinecraftYear());
            return currentElection.Winner;
        }

        private static int CurrentMinecraftYear()
        {
            return (int)((DateTime.Now - new DateTime(2019, 6, 13)).TotalDays / (TimeSpan.FromDays(5) + TimeSpan.FromHours(4)).TotalDays + 1);
        }

        /// <summary>
        /// Get the name of the last mayor
        /// </summary>
        /// <remarks>Returns the name of the last mayor</remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [HttpGet]
        [Route("/mayor/last")]
        [ValidateModelState]
        [SwaggerOperation("MayorLastGet")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "OK")]
        public async Task<string> MayorLastGet()
        {
            var lastElection = await mayorService.GetElectionPeriod(CurrentMinecraftYear() - 1);
            return lastElection.Winner.Name;
        }

        /// <summary>
        /// Get names of all mayors
        /// </summary>
        /// <remarks>Returns all mayor names</remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        [HttpGet]
        [Route("/mayor/names")]
        [ValidateModelState]
        [SwaggerOperation("MayorNamesGet")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<string>), description: "OK")]
        public virtual IActionResult MayorNamesGet()
        {

            //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(200, default(List<string>));
            //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(400);
            //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(404);
            string exampleJson = null;
            exampleJson = "[ \"\", \"\" ]";

            var example = exampleJson != null
            ? JsonConvert.DeserializeObject<List<string>>(exampleJson)
            : default(List<string>);
            //TODO: Change the data returned
            return new ObjectResult(example);
        }

        /// <summary>
        /// Get the next mayor
        /// </summary>
        /// <remarks>Returns the mayor with the most votes in the current election. If there is currently no election, this returns null.</remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        [HttpGet]
        [Route("/mayor/next")]
        [ValidateModelState]
        [SwaggerOperation("MayorNextGet")]
        [SwaggerResponse(statusCode: 200, type: typeof(ModelCandidate), description: "OK")]
        public virtual async Task<ModelCandidate> MayorNextGet()
        {
            var currentElection = await mayorService.GetElectionPeriod(CurrentMinecraftYear());
            if (currentElection == null)
            {
                return null;
            }
            return currentElection.Candidates.OrderByDescending(c => c.Votes).FirstOrDefault();
        }
    }
}
