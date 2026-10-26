namespace StudySpace.Models;

public enum NoiseCategory { Quiet, Moderate, Loud }

public enum UserRole { Student, Admin }

public enum ReservationStatus { Confirmed, Cancelled, Completed }

public enum OccupancyLevel { Empty, Moderate, Crowded, Full }

public static class EnumExtensions
{
    public static string BadgeClass(this NoiseCategory n) => n switch
    {
        NoiseCategory.Quiet => "badge-quiet",
        NoiseCategory.Moderate => "badge-moderate",
        _ => "badge-loud"
    };

    public static string BadgeClass(this OccupancyLevel o) => o switch
    {
        OccupancyLevel.Empty => "bg-success",
        OccupancyLevel.Moderate => "bg-info text-dark",
        OccupancyLevel.Crowded => "bg-warning text-dark",
        _ => "bg-danger"
    };

    public static string BadgeClass(this ReservationStatus s) => s switch
    {
        ReservationStatus.Confirmed => "bg-success",
        ReservationStatus.Cancelled => "bg-secondary",
        _ => "bg-primary"
    };

    public static int Percent(this OccupancyLevel o) => o switch
    {
        OccupancyLevel.Empty => 10,
        OccupancyLevel.Moderate => 45,
        OccupancyLevel.Crowded => 80,
        _ => 100
    };

    public static string ProgressClass(this OccupancyLevel o) => o switch
    {
        OccupancyLevel.Empty => "bg-success",
        OccupancyLevel.Moderate => "bg-info",
        OccupancyLevel.Crowded => "bg-warning",
        _ => "bg-danger"
    };
}
