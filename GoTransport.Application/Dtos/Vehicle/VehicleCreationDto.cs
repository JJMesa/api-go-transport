namespace GoTransport.Application.Dtos.Vehicle;

public class VehicleCreationDto
{
    public int ManufacturerId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public int Model { get; set; }

    public int Capacity { get; set; }
}