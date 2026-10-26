using Microsoft.AspNetCore.Identity;
using StudySpace.Models;

namespace StudySpace.Services;

public class DataStore
{
    public object SyncRoot { get; } = new();

    public List<User> Users { get; } = new();
    public List<Building> Buildings { get; } = new();
    public List<StudySpot> Spots { get; } = new();
    public List<Reservation> Reservations { get; } = new();
    public List<StatusReport> Reports { get; } = new();

    private int _userId, _spotId, _reservationId, _reportId;
    public int NextUserId() => ++_userId;
    public int NextSpotId() => ++_spotId;
    public int NextReservationId() => ++_reservationId;
    public int NextReportId() => ++_reportId;

    public DataStore(IPasswordHasher<User> hasher) => Seed(hasher);

    private void Seed(IPasswordHasher<User> hasher)
    {
        AddUser(hasher, "Dr. Salim Al-Busaidi", "admin@squ.edu.om", "Admin@123", UserRole.Admin);
        AddUser(hasher, "Ghassan Almawali", "student@student.squ.edu.om", "Student@123", UserRole.Student);
        AddUser(hasher, "Almustafa AlAmri", "almustafa@student.squ.edu.om", "Student@123", UserRole.Student);

        Buildings.AddRange(new[]
        {
            new Building(1, "Main Library", "Central Zone", 50, 48),
            new Building(2, "College of Science", "North Zone", 24, 22),
            new Building(3, "College of Engineering", "East Zone", 76, 24),
            new Building(4, "Student Center", "South Zone", 42, 80),
        });

        var t = (int h, int m) => new TimeOnly(h, m);
        Spots.AddRange(new[]
        {
            new StudySpot(NextSpotId(), 1, "2nd Floor Silent Corner", "2F-A", 40, NoiseCategory.Quiet, true, false, t(8,0), t(23,0)),
            new StudySpot(NextSpotId(), 1, "Group Study Room 1", "G-101", 8, NoiseCategory.Moderate, true, true, t(8,0), t(22,0)),
            new StudySpot(NextSpotId(), 1, "Group Study Room 2", "G-102", 6, NoiseCategory.Moderate, true, true, t(8,0), t(22,0)),
            new StudySpot(NextSpotId(), 1, "Ground Floor Reading Hall", "RH-1", 120, NoiseCategory.Moderate, true, false, t(8,0), t(23,0)),
            new StudySpot(NextSpotId(), 2, "Science Quiet Lounge", "S-210", 25, NoiseCategory.Quiet, true, false, t(7,30), t(20,0)),
            new StudySpot(NextSpotId(), 2, "Silent Study Pod A", "S-POD1", 2, NoiseCategory.Quiet, true, true, t(8,0), t(20,0)),
            new StudySpot(NextSpotId(), 3, "Engineering Lab Study Zone", "E-114", 30, NoiseCategory.Moderate, true, false, t(8,0), t(21,0)),
            new StudySpot(NextSpotId(), 3, "Collaboration Room", "E-220", 10, NoiseCategory.Loud, true, true, t(8,0), t(21,0)),
            new StudySpot(NextSpotId(), 4, "Open Lounge", "SC-001", 60, NoiseCategory.Loud, false, false, t(7,0), t(23,59)),
            new StudySpot(NextSpotId(), 4, "Courtyard Tables", "Outdoor", 24, NoiseCategory.Moderate, false, false, t(7,0), t(22,0)),
        });
        Spots[7].MaintenanceNotice = "Whiteboard replacement scheduled on Thursday morning.";

        Report(1, 2, NoiseCategory.Quiet, OccupancyLevel.Moderate, "Few seats left near the windows.", 12);
        Report(4, 3, NoiseCategory.Moderate, OccupancyLevel.Crowded, "Busy before midterms.", 25);
        Report(5, 2, NoiseCategory.Quiet, OccupancyLevel.Empty, null, 8);
        Report(9, 3, NoiseCategory.Loud, OccupancyLevel.Full, "Very loud today.", 5);

        var tomorrow = DateTime.Today.AddDays(1);
        Reservations.Add(new Reservation(NextReservationId(), 2, 2, tomorrow.AddHours(10), tomorrow.AddHours(12), "Group project meeting"));
    }

    private void AddUser(IPasswordHasher<User> hasher, string name, string email, string password, UserRole role)
    {
        var user = new User(NextUserId(), name, email, string.Empty, role);
        user.PasswordHash = hasher.HashPassword(user, password);
        Users.Add(user);
    }

    private void Report(int spotId, int userId, NoiseCategory noise, OccupancyLevel occ, string? comment, int minutesAgo)
    {
        var user = Users.First(u => u.UserID == userId);
        var report = new StatusReport(NextReportId(), spotId, userId, user.FirstName(), noise, occ, comment)
        {
            TimeStamp = DateTime.Now.AddMinutes(-minutesAgo)
        };
        Reports.Add(report);
        Spots.First(s => s.SpotID == spotId).ApplyReport(report);
    }
}
