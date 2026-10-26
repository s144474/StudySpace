namespace StudySpace.Models;

public class Building
{
    public int BuildingID { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string CampusZone { get; set; } = string.Empty;
    public int MapX { get; set; }
    public int MapY { get; set; }

    public Building() { }

    public Building(int id, string name, string zone, int mapX, int mapY)
    {
        BuildingID = id; BuildingName = name; CampusZone = zone; MapX = mapX; MapY = mapY;
    }

    public override string ToString() => $"{BuildingName} ({CampusZone})";
}
