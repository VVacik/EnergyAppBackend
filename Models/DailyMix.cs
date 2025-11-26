namespace EnergyApi.Models;

public class DailyMix
{
    public DateTime Date { get; set; }
    public Dictionary<string, double> Averages { get; set; }
    public double CleanEnergyPercent { get; set; }
}
