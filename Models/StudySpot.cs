namespace StudySpace.Models;

public class SpotFilter
{
    public string? Search { get; set; }
    public int? BuildingId { get; set; }
    public NoiseCategory? Noise { get; set; }
    public bool PowerOutlets { get; set; }
    public bool BookableOnly { get; set; }
    public bool OpenNow { get; set; }
    public int? MinSeats { get; set; }
}

public class StudySpot
{
    public int SpotID { get; set; }
    public int BuildingID { get; set; }
    public string SpotName { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
    public int MaxCapacity { get; set; }
    public NoiseCategory NoiseCategory { get; set; } = NoiseCategory.Quiet;
    public bool HasPowerOutlets { get; set; } = true;
    public bool IsBookable { get; set; }
    public bool IsActive { get; set; } = true;
    public TimeOnly OpeningTime { get; set; } = new(8, 0);
    public TimeOnly ClosingTime { get; set; } = new(22, 0);
    public string? MaintenanceNotice { get; set; }

    public NoiseCategory? LiveNoise { get; private set; }
    public OccupancyLevel? LiveOccupancy { get; private set; }
    public DateTime? LastReportAt { get; private set; }

    public StudySpot() { }

    public StudySpot(int spotId, int buildingId, string name, string? room, int capacity,
        NoiseCategory noise, bool outlets, bool bookable, TimeOnly? open = null, TimeOnly? close = null)
    {
        SpotID = spotId; BuildingID = buildingId; SpotName = name; RoomNumber = room;
        MaxCapacity = capacity; NoiseCategory = noise; HasPowerOutlets = outlets; IsBookable = bookable;
        if (open.HasValue) OpeningTime = open.Value;
        if (close.HasValue) ClosingTime = close.Value;
    }

    //business logic
    public bool IsLiveFresh => LastReportAt.HasValue && (DateTime.Now - LastReportAt.Value).TotalMinutes <= 90;

    public NoiseCategory EffectiveNoise => IsLiveFresh && LiveNoise.HasValue ? LiveNoise.Value : NoiseCategory;

    public OccupancyLevel? CurrentOccupancy => IsLiveFresh ? LiveOccupancy : null;

    public bool IsOpenAt(TimeOnly time) => time >= OpeningTime && time < ClosingTime;

    public bool IsOpenNow() => IsActive && IsOpenAt(TimeOnly.FromDateTime(DateTime.Now));

    public int EstimatedFreeSeats()
    {
        if (CurrentOccupancy is null) return MaxCapacity;
        return CurrentOccupancy.Value switch
        {
            OccupancyLevel.Empty => MaxCapacity,
            OccupancyLevel.Moderate => (int)Math.Round(MaxCapacity * 0.55),
            OccupancyLevel.Crowded => (int)Math.Round(MaxCapacity * 0.2),
            _ => 0
        };
    }

    public void ApplyReport(StatusReport report)
    {
        LiveNoise = report.ReportedNoise;
        LiveOccupancy = report.OccupancyLevel;
        LastReportAt = report.TimeStamp;
    }

    public void ClearLiveStatus() { LiveNoise = null; LiveOccupancy = null; LastReportAt = null; }

    public void Enable() => IsActive = true;
    public void Disable() => IsActive = false;

    public void UpdateFrom(StudySpot other)
    {
        BuildingID = other.BuildingID; SpotName = other.SpotName; RoomNumber = other.RoomNumber;
        MaxCapacity = other.MaxCapacity; NoiseCategory = other.NoiseCategory;
        HasPowerOutlets = other.HasPowerOutlets; IsBookable = other.IsBookable; IsActive = other.IsActive;
        OpeningTime = other.OpeningTime; ClosingTime = other.ClosingTime;
        MaintenanceNotice = string.IsNullOrWhiteSpace(other.MaintenanceNotice) ? null : other.MaintenanceNotice.Trim();
    }

    public bool Matches(SpotFilter f, string buildingName)
    {
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var q = f.Search.Trim();
            bool hit = SpotName.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (RoomNumber?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                    || buildingName.Contains(q, StringComparison.OrdinalIgnoreCase);
            if (!hit) return false;
        }
        if (f.BuildingId.HasValue && BuildingID != f.BuildingId.Value) return false;
        if (f.Noise.HasValue && EffectiveNoise != f.Noise.Value) return false;
        if (f.PowerOutlets && !HasPowerOutlets) return false;
        if (f.BookableOnly && !IsBookable) return false;
        if (f.OpenNow && !IsOpenNow()) return false;
        if (f.MinSeats.HasValue && MaxCapacity < f.MinSeats.Value) return false;
        return true;
    }
}
