namespace WorldTime.Models
{
    public class TimeConverterViewModel
    {
        public string FromZoneId { get; set; } = "";
        public string ToZoneId { get; set; } = "";
        public DateTime? SourceTime { get; set; }
        public DateTime? ConvertedTime { get; set; }
        public List<TimeZoneOption> AvailableZones { get; set; } = new();
    }

    public class TimeZoneOption
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }
}