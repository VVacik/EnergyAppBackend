using EnergyApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace EnergyApi.Controllers;


//Controller for Daily energy mixes and optimal Charging windows
[ApiController]
[Route("energy")]
public class EnergyController : ControllerBase
{
    private readonly ICarbonExternalClient _carbonClient;

    public EnergyController(ICarbonExternalClient carbonClient)
    {
        _carbonClient = carbonClient;
    }

    // --- Endpoint: GET /energy/mix ---
    [HttpGet("mix")]
    public async Task<ActionResult<IEnumerable<DailyMix>>> GetEnergyMix()
    {
        var today = DateTime.UtcNow.Date;
        var afterTomorrow = today.AddDays(3);

        var apiResp = await _carbonClient.GetGenerationDataAsync(today, afterTomorrow);
        if (apiResp?.Data == null || apiResp.Data.Length == 0)
            return BadRequest("No data received from external API.");

        var result = _carbonClient.CalculateDailyMix(apiResp);
        return Ok(result);
    }


    // --- Endpoint: GET /energy/optimal?hours=X ---
    [HttpGet("optimal")]
    public async Task<ActionResult<object>> GetOptimalChargingWindow([FromQuery] int hours)
    {
        if ((hours <= 0) || (hours > 6)) return BadRequest("Hours must be > 0 and < 6");

        var start = DateTime.UtcNow;
        var end = start.AddDays(2);

        var apiResp = await _carbonClient.GetGenerationDataAsync(start, end);
        if (apiResp?.Data == null || apiResp.Data.Length == 0)
            return BadRequest("No data received from external API.");

        try
        {
            var result = _carbonClient.CalculateOptimalWindow(apiResp, hours);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
