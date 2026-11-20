var house = new House("12 Park Street", 1998, 2);
var tower = new Skyscraper("34 Central Avenue", 2021, 36, 8);
Building[] buildings = [new Building("5 Garden Lane", 1975), house, tower];
foreach (Building building in buildings)
    building.GetInfo();

house.OpenDoor();
tower.GoToRoof();
Building asBuilding = house;
Console.WriteLine($"House reference: {house.GetBuildingType()}");
Console.WriteLine($"Building reference: {asBuilding.GetBuildingType()}");

public class Building(string address, int yearBuilt)
{
    public string Address { get; } = address;
    public int YearBuilt { get; } = yearBuilt;

    public virtual void GetInfo() => Console.WriteLine($"{Address}, built {YearBuilt}");
    public string GetBuildingType() => "Building";
}

public class House(string address, int yearBuilt, int numFloors) : Building(address, yearBuilt)
{
    public int NumFloors { get; } = numFloors;

    public override void GetInfo()
    {
        base.GetInfo();
        Console.WriteLine($"Floors: {NumFloors}");
    }

    public void OpenDoor() => Console.WriteLine($"Door opened at {Address}.");
    public new string GetBuildingType() => "House";
}

public sealed class Skyscraper(string address, int yearBuilt, int numFloors, int numElevators)
    : House(address, yearBuilt, numFloors)
{
    public int NumElevators { get; } = numElevators;

    public override void GetInfo()
    {
        base.GetInfo();
        Console.WriteLine($"Elevators: {NumElevators}");
    }

    public void GoToRoof() => Console.WriteLine($"Arrived at the roof of {Address}.");
}
