namespace StudySpace.Models;

public class StatusReport
{
    public int ReportID { get; set; }
    public int SpotID { get; set; }
    public int UserID { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public NoiseCategory ReportedNoise { get; set; }
    public OccupancyLevel OccupancyLevel { get; set; }
    public string? Comment { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public bool IsHidden { get; set; }

    public StatusReport() { }

    public StatusReport(int id, int spotId, int userId, string reporterName,
        NoiseCategory noise, OccupancyLevel occupancy, string? comment)
    {
        ReportID = id; SpotID = spotId; UserID = userId; ReporterName = reporterName;
        ReportedNoise = noise; OccupancyLevel = occupancy; Comment = comment;
        TimeStamp = DateTime.Now;
    }

    public bool IsFresh(int minutes = 90) => (DateTime.Now - TimeStamp).TotalMinutes <= minutes;

    public string TimeAgo()
    {
        var span = DateTime.Now - TimeStamp;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} h ago";
        return $"{(int)span.TotalDays} d ago";
    }
}
