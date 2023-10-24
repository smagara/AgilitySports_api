// Payload to feed the PrimeNG Chart api
namespace AgilitySportsAPI.Dtos;

// MLBAttendChartDTO myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class Dataset
    {
        public string label { get; set; }
        public string? backgroundColor { get; set; }
        public string? borderColor { get; set; }
        public string? borderWidth { get; set; }
        public List<string> data { get; set; }
    }

    public class MLBAttendChartDTO
    {
        public List<string> labels { get; set; }
        public List<Dataset> datasets { get; set; }
    }
