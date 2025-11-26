namespace EnergyApi.Models;

// Represents agregated mix for 1 day

public class DailyMix
{
    public DateTime Date { get; set; }

    public Dictionary<string, double> Averages { get; set; }
    // Dictionary (Key = fuel name) (Value = average of percentages combined from timestamps during the day)
    //exc: { "wind": 35.5, "solar": 15.2 }
    public double CleanEnergyPercent { get; set; }
    // Percentage of usage Clean energy sources during the day
}
