using EnergyApi.Models;
using System.Net.Http.Json;


// This class implements ICarbonExternalClient and is responsible for fetching and processing
// energy generation data from an external API (Carbon Intensity API).
// It centralizes API calls and calculations so that controller logic remains clean.
public class CarbonExternalClient : ICarbonExternalClient
{
    //Setup for Http request
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://api.carbonintensity.org.uk/generation/";

    
    // Set of energy sources considered "clean". Used for calculating clean energy percentage.
    private static readonly HashSet<string> CleanSources = new()
    {
        "biomass", "nuclear", "hydro", "wind", "solar"
    };
    
    // Constructor receives HttpClient from DI. Ensures reusability and proper HTTP connection management.
    public CarbonExternalClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    


    //Fetching raw data from the API
    public async Task<CarbonApiResponse?> GetGenerationDataAsync(DateTime from, DateTime to)
    {
        string url = $"{ApiUrl}{from:yyyy-MM-dd}T00:00Z/{to:yyyy-MM-dd}T00:00Z";
        return await _httpClient.GetFromJsonAsync<CarbonApiResponse>(url);
    }


    //Calculate daily energy mix from raw Api data
    public IEnumerable<DailyMix> CalculateDailyMix(CarbonApiResponse apiResp, int days = 3)
    {
        DateTime today = DateTime.UtcNow.Date;
        var validDates = Enumerable.Range(0, days).Select(d => today.AddDays(d)).ToArray();
        //Prepare Array of days that are valid to thecurrent calculation

        return apiResp.Data
            .Where(d => validDates.Contains(d.From.Date))
            //Filters out only intervals in validDates
            .GroupBy(d => d.From.Date)
            //Groups intervals by date
            .Select(g => new DailyMix
            {
                Date = g.Key,
                //Set the day for aggregated record
                Averages = g
                    .SelectMany(x => x.GenerationMix)
                    .GroupBy(x => x.Fuel)
                    .ToDictionary(x => x.Key, x => x.Average(z => z.Perc)),
                // For each fuel type, calculate the average percentage across all intervals of that day.
                CleanEnergyPercent = g
                    .SelectMany(x => x.GenerationMix)
                    .Where(x => CleanSources.Contains(x.Fuel.ToLower()))
                    .Average(x => x.Perc)
                // Calculate the total clean energy percentage by averaging all clean sources.
            })
            .OrderBy(x => x.Date)
            .ToList();
            // Return a list of DailyMix objects representing daily aggregated energy data ordered by day.
    }

    
    //Calculate the optimal charging window for EVs based on clean energy
    public object CalculateOptimalWindow(CarbonApiResponse apiResp, int hours)
    {
        int intervalsNeeded = hours * 2;
        // Each interval is 30 minutes, so multiply hours by 2 to get the number of intervals needed.

        var intervals = apiResp.Data
            .Select(x =>
            {
                double clean = x.GenerationMix
                    .Where(g => CleanSources.Contains(g.Fuel.ToLower()))
                    .Sum(g => g.Perc);
                // Sum percentages of clean sources.

                double total = x.GenerationMix.Sum(g => g.Perc);
                // Sum percentages of all sources to normalize.

                return new
                {
                    x.From,
                    x.To,
                    CleanPercent = clean / (total == 0 ? 1 : total) * 100
                    // Compute clean energy percentage for this interval
                };
            })
            .OrderBy(x => x.From)
            .ToList();

        if (intervals.Count < intervalsNeeded)
            throw new InvalidOperationException("Not enough data for the requested window.");

        //Ensure there is enough data to cover the requested hours.

        double bestAvg = -1;
        int bestIndex = -1;
        // Variables to track the best window.

        for (int i = 0; i <= intervals.Count - intervalsNeeded; i++)
        {
            double avgClean = intervals.Skip(i).Take(intervalsNeeded).Average(x => x.CleanPercent);
            if (avgClean > bestAvg)
            {
                bestAvg = avgClean;
                bestIndex = i;
            }
        }
        // Sliding window algorithm:
        // Check every possible consecutive block of intervals and pick the one with highest average clean energy.

        return new
        {
            requestedHours = hours,
            start = intervals[bestIndex].From,
            end = intervals[bestIndex + intervalsNeeded - 1].To,
            averageCleanEnergy = bestAvg
        };
        // Return an anonymous object with optimal start/end times and average clean energy for that window.
    }
}
