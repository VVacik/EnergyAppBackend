namespace EnergyApi.Models;



//Contains Table for objects GenerationData (Time from, Time to, and Table for Generation mixes) 
public class CarbonApiResponse
{
    public GenerationData[] Data { get; set; }
}


//Represents 1 Time stanp of Energy production
public class GenerationData
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    // Contains Table for storing objects Generation Mix (Fuel, Perc)
    public GenerationMix[] GenerationMix { get; set; }
}


// Represents 1 Fuel type and its percentage share in timestamp
public class GenerationMix
{
    public string Fuel { get; set; }
    public double Perc { get; set; }
}
