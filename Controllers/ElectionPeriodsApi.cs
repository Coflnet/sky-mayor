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

namespace Coflnet.Sky.Mayor.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class ElectionPeriodsApiController : ControllerBase
    {
        private MayorService mayorService;

        public ElectionPeriodsApiController(MayorService mayorService)
        {
            this.mayorService = mayorService;
        }

        /// <summary>
        /// Inserts election periods
        /// </summary>
        /// <remarks>Endpoint to insert election periods, should only be used to insert missing/hisotical data</remarks>
        /// <param name="periods">the election periods that are going to be inserted</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        [HttpPost]
        [Route("/electionPeriod")]
        [ValidateModelState]
        [SwaggerOperation("ElectionPeriodPost")]
        [SwaggerResponse(statusCode: 201, type: typeof(List<ModelElectionPeriod>), description: "Created")]
        public virtual async Task<IActionResult> ElectionPeriodPost([FromBody] List<ModelElectionPeriod> periods)
        {
            await mayorService.InsertElectionPeriods(periods);
            return StatusCode(201);
        }

        /// <summary>
        /// Get election periods by timespan
        /// </summary>
        /// <remarks>Returns all election periods that took place in a given timespan</remarks>
        /// <param name="from">from The beginning of the selected timespan</param>
        /// <param name="to">The end of the selected timespan</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        [HttpGet]
        [Route("/electionPeriod/range")]
        [ValidateModelState]
        [SwaggerOperation("ElectionPeriodRangeGet")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<ModelElectionPeriod>), description: "OK")]
        public async Task<IEnumerable<ModelElectionPeriod>> ElectionPeriodRangeGet([FromQuery(Name = "from")][Required()] long from, [FromQuery(Name = "to")][Required()] long to)
        {
            var startTime = DateTimeOffset.FromUnixTimeMilliseconds(from);
            var endTime = DateTimeOffset.FromUnixTimeMilliseconds(to);
            return await mayorService.GetElectionPeriods(GetMinecraftYear(startTime), GetMinecraftYear(endTime) );
        }

        private static int CurrentMinecraftYear()
        {
            return (int)((DateTime.Now - new DateTime(2019, 6, 13)).TotalDays / (TimeSpan.FromDays(5) + TimeSpan.FromHours(4)).TotalDays + 1);
        }

        private static int GetMinecraftYear(DateTimeOffset date)
        {
            return (int)((date - new DateTime(2019, 6, 13)).TotalDays / (TimeSpan.FromDays(5) + TimeSpan.FromHours(4)).TotalDays + 1);
        }

        /// <summary>
        /// Get the election period of a certain year
        /// </summary>
        /// <remarks>Returns the election periods that took place in a given year</remarks>
        /// <param name="year">the searched year</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        [HttpGet]
        [Route("/electionPeriod/{year}")]
        [ValidateModelState]
        [SwaggerOperation("ElectionPeriodYearGet")]
        [SwaggerResponse(statusCode: 200, type: typeof(ModelElectionPeriod), description: "OK")]
        public async Task<ModelElectionPeriod> ElectionPeriodYearGet([FromRoute(Name = "year")][Required] int year)
        {
            return await mayorService.GetElectionPeriod(year);
        }
    }
}
