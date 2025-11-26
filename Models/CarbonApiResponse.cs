namespace EnergyApi.Models;

public class CarbonApiResponse
{
    public GenerationData[] Data { get; set; }
}

public class GenerationData
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public GenerationMix[] GenerationMix { get; set; }
}

public class GenerationMix
{
    public string Fuel { get; set; }
    public double Perc { get; set; }
}
