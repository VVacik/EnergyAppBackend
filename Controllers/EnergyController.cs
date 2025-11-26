using EnergyApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace EnergyApi.Controllers;

[ApiController]
[Route("energy")]
public class EnergyController : ControllerBase
{
    private static readonly string ApiUrl = "https://api.carbonintensity.org.uk/generation/";

    private static readonly HashSet<string> CleanSources = new()
    {
        "biomass", "nuclear", "hydro", "wind", "solar"
    };

    [HttpGet("mix")]
    public async Task<ActionResult<IEnumerable<DailyMix>>> GetEnergyMix()
    {
        var http = new HttpClient();

        DateTime today = DateTime.UtcNow.Date;
        DateTime afterTomorrow = today.AddDays(3);

        string url = $"{ApiUrl}{today:yyyy-MM-dd}T00:00Z/{afterTomorrow:yyyy-MM-dd}T00:00Z";

        var apiResp = await http.GetFromJsonAsync<CarbonApiResponse>(url);

        if (apiResp?.Data == null || apiResp.Data.Length == 0)
            return BadRequest("No data received from external API.");

        // grupacja po dniu
        DateTime d0 = today;
        DateTime d1 = today.AddDays(1);
        DateTime d2 = today.AddDays(2);

        var validDates = new[] { d0, d1, d2 };

        var grouped = apiResp.Data
            .Where(d => validDates.Contains(d.From.Date))
            .GroupBy(d => d.From.Date)

                    .Select(g => new DailyMix
            {
                Date = g.Key,
                Averages = g
                    .SelectMany(x => x.GenerationMix)
                    .GroupBy(x => x.Fuel)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Average(z => z.Perc)
                    ),
                CleanEnergyPercent = g
                    .SelectMany(x => x.GenerationMix)
                    .Where(x => CleanSources.Contains(x.Fuel.ToLower()))
                    .Average(x => x.Perc)
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Ok(grouped);
    }



    [HttpGet("optimal")]
    public async Task<ActionResult<object>> GetOptimalChargingWindow([FromQuery] int hours)
    {
        if (hours <= 0)
            return BadRequest("Hours must be > 0");

        int intervalsNeeded = hours * 2;

        var http = new HttpClient();

        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddDays(2); // next 48 hours

        string url = $"https://api.carbonintensity.org.uk/generation/{start:yyyy-MM-dd}T{start:HH:mm}Z/{end:yyyy-MM-dd}T{end:HH:mm}Z";

        var apiResp = await http.GetFromJsonAsync<CarbonApiResponse>(url);

        if (apiResp?.Data == null || apiResp.Data.Length == 0)
            return BadRequest("No data received from external API.");

        // policzenie procentu czystej energii dla każdego interwału
        var intervals = apiResp.Data
            .Select(x =>
            {
                double clean = x.GenerationMix
                    .Where(g => CleanSources.Contains(g.Fuel.ToLower()))
                    .Sum(g => g.Perc);

                double total = x.GenerationMix.Sum(g => g.Perc);

                return new
                {
                    x.From,
                    x.To,
                    CleanPercent = clean / (total == 0 ? 1 : total) * 100
                };
            })
            .OrderBy(x => x.From)
            .ToList();

        if (intervals.Count < intervalsNeeded)
            return BadRequest("Not enough data for the requested window.");

        // SLIDING WINDOW, krok = 1 interwał
        double bestAvg = -1;
        int bestIndex = -1;

        for (int i = 0; i <= intervals.Count - intervalsNeeded; i++)
        {
            double avgClean = intervals
                .Skip(i)
                .Take(intervalsNeeded)
                .Average(x => x.CleanPercent);

            if (avgClean > bestAvg)
            {
                bestAvg = avgClean;
                bestIndex = i;
            }
        }

        var bestStart = intervals[bestIndex].From;
        var bestEnd = intervals[bestIndex + intervalsNeeded - 1].To;

        return Ok(new
        {
            requestedHours = hours,
            start = bestStart,
            end = bestEnd,
            averageCleanEnergy = bestAvg
        });
    }

}
