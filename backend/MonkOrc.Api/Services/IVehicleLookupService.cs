using MonkOrc.Api.DTOs;

namespace MonkOrc.Api.Services
{
    public interface IVehicleLookupService
    {
        Task<VehicleLookupResponse> ConsultPlateAsync(string licensePlate);
    }
}
