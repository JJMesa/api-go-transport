using GoTransport.Application.Dtos.Manufacturer;

namespace GoTransport.Application.Dtos.Vehicle;

public class VehicleDto
{
    public int VehicleId { get; set; }

    public ManufacturerDto Manufacturer { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;

    public int Model { get; set; }

    public int Capacity { get; set; }

    public bool IsActive { get; set; }
}