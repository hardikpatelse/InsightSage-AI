namespace InsightSage.Shared.Models.Custom.Examples.Response
{
    public class WeatherResponse
    {
        public string Name { get; set; } = string.Empty;
        public Main Main { get; set; } = new();
        public List<Weather> Weather { get; set; } = new();
    }
}
