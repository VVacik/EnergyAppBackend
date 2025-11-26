namespace EnergyApi.Models;



//Contains Table for objects GenerationData (Time from, Time to, and Table for Generation mixes) 
public class CarbonApiResponse
{
    required public GenerationData[] Data { get; set; }
}


//Represents 1 Time stanp of Energy production
public class GenerationData
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    // Contains Table for storing objects Generation Mix (Fuel, Perc)
    required public GenerationMix[] GenerationMix { get; set; }
}


// Represents 1 Fuel type and its percentage share in timestamp
public class GenerationMix
{
    required public string Fuel { get; set; }
    required public double Perc { get; set; }
}
