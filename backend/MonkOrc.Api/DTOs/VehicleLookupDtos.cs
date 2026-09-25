namespace MonkOrc.Api.DTOs
{
    public class VehicleLookupRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
    }

    public class VehicleLookupData
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Version { get; set; }
        public int Year { get; set; }
        public int? ModelYear { get; set; }
        public string? Color { get; set; }
        public string? Fuel { get; set; }
        public string? Chassis { get; set; }
        public string? Renavam { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? VehicleType { get; set; }
        public string? Engine { get; set; }
    }

    public class VehicleLookupResponse
    {
        public bool Success { get; set; }
        public bool AlreadyExists { get; set; }
        public string? ExistingCustomerName { get; set; }
        public string Message { get; set; } = string.Empty;
        public VehicleLookupData? Data { get; set; }
    }
}
