using EnergyApi.Models;

public interface ICarbonExternalClient
{
    Task<CarbonApiResponse?> GetGenerationDataAsync(DateTime from, DateTime to);

    IEnumerable<DailyMix> CalculateDailyMix(CarbonApiResponse apiResp, int days = 3);

    object CalculateOptimalWindow(CarbonApiResponse apiResp, int hours);
}
