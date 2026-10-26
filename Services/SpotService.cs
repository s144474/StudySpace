using StudySpace.Exceptions;
using StudySpace.Models;

namespace StudySpace.Services;

public class SpotCardVm
{
    public StudySpot Spot { get; set; } = null!;
    public string BuildingName { get; set; } = string.Empty;
    public string CampusZone { get; set; } = string.Empty;
}

public class SpotResults
{
    public List<SpotCardVm> Cards { get; set; } = new();
    public List<Building> Buildings { get; set; } = new();
}

public class SpotService
{
    private readonly DataStore _store;
    public SpotService(DataStore store) => _store = store;

    public List<Building> GetBuildings()
    {
        lock (_store.SyncRoot) return _store.Buildings.ToList();
    }

    public StudySpot? Find(int id)
    {
        lock (_store.SyncRoot) return _store.Spots.FirstOrDefault(s => s.SpotID == id);
    }

    public StudySpot Get(int id) => Find(id) ?? throw new NotFoundException($"Study spot #{id} was not found.");

    public Building GetBuilding(int id)
    {
        lock (_store.SyncRoot)
            return _store.Buildings.FirstOrDefault(b => b.BuildingID == id)
                   ?? throw new NotFoundException($"Building #{id} was not found.");
    }

    public SpotResults Search(SpotFilter filter, bool includeInactive = false)
    {
        lock (_store.SyncRoot)
        {
            var cards = _store.Spots
                .Where(s => includeInactive || s.IsActive)
                .Select(s => new { Spot = s, Building = _store.Buildings.First(b => b.BuildingID == s.BuildingID) })
                .Where(x => x.Spot.Matches(filter, x.Building.BuildingName))
                .OrderBy(x => x.Spot.EffectiveNoise).ThenBy(x => x.Building.BuildingName).ThenBy(x => x.Spot.SpotName)
                .Select(x => new SpotCardVm { Spot = x.Spot, BuildingName = x.Building.BuildingName, CampusZone = x.Building.CampusZone })
                .ToList();
            return new SpotResults { Cards = cards, Buildings = _store.Buildings.ToList() };
        }
    }

    //admin
    public StudySpot Save(StudySpot input)
    {
        if (input.MaxCapacity <= 0) throw new BusinessRuleException("Capacity must be greater than zero.");
        if (input.ClosingTime <= input.OpeningTime) throw new BusinessRuleException("Closing time must be after opening time.");

        lock (_store.SyncRoot)
        {
            if (!_store.Buildings.Any(b => b.BuildingID == input.BuildingID))
                throw new NotFoundException("The selected building does not exist.");

            if (input.SpotID == 0)
            {
                input.SpotID = _store.NextSpotId();
                input.MaintenanceNotice = string.IsNullOrWhiteSpace(input.MaintenanceNotice) ? null : input.MaintenanceNotice.Trim();
                _store.Spots.Add(input);
                return input;
            }

            var existing = _store.Spots.FirstOrDefault(s => s.SpotID == input.SpotID)
                           ?? throw new NotFoundException($"Study spot #{input.SpotID} was not found.");
            existing.UpdateFrom(input);
            return existing;
        }
    }

    public StudySpot SetActive(int id, bool active)
    {
        var spot = Get(id);
        lock (_store.SyncRoot)
        {
            if (active) spot.Enable(); else spot.Disable();
            return spot;
        }
    }

    //crowdsourced reports
    public StatusReport AddReport(int spotId, int userId, string reporterName,
        NoiseCategory noise, OccupancyLevel occupancy, string? comment)
    {
        lock (_store.SyncRoot)
        {
            var spot = _store.Spots.FirstOrDefault(s => s.SpotID == spotId)
                       ?? throw new NotFoundException($"Study spot #{spotId} was not found.");
            if (!spot.IsActive) throw new BusinessRuleException("Reports cannot be submitted for a disabled spot.");

            var recent = _store.Reports.Any(r => r.SpotID == spotId && r.UserID == userId
                                                 && (DateTime.Now - r.TimeStamp).TotalMinutes < 5);
            if (recent) throw new BusinessRuleException("You already reported this spot a moment ago. Please wait 5 minutes before updating it again.");

            var report = new StatusReport(_store.NextReportId(), spotId, userId, reporterName, noise, occupancy,
                string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());
            _store.Reports.Add(report);
            spot.ApplyReport(report);
            return report;
        }
    }

    public List<StatusReport> GetReports(int spotId, int take = 8)
    {
        lock (_store.SyncRoot)
            return _store.Reports.Where(r => r.SpotID == spotId && !r.IsHidden)
                .OrderByDescending(r => r.TimeStamp).Take(take).ToList();
    }

    public List<StatusReport> RecentReports(int take = 20)
    {
        lock (_store.SyncRoot)
            return _store.Reports.OrderByDescending(r => r.TimeStamp).Take(take).ToList();
    }

    public string SpotName(int spotId)
    {
        lock (_store.SyncRoot) return _store.Spots.FirstOrDefault(s => s.SpotID == spotId)?.SpotName ?? "(deleted spot)";
    }

    public void HideReport(int reportId)
    {
        lock (_store.SyncRoot)
        {
            var report = _store.Reports.FirstOrDefault(r => r.ReportID == reportId)
                         ?? throw new NotFoundException($"Report #{reportId} was not found.");
            report.IsHidden = true;

            var spot = _store.Spots.First(s => s.SpotID == report.SpotID);
            var latest = _store.Reports.Where(r => r.SpotID == spot.SpotID && !r.IsHidden)
                .OrderByDescending(r => r.TimeStamp).FirstOrDefault();
            if (latest is null) spot.ClearLiveStatus(); else spot.ApplyReport(latest);
        }
    }
}
