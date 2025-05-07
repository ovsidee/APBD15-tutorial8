using APBD_Tutorial8.Models.DTO_s;

namespace APBD_Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDto>> getTripsWithCountriesAsync(CancellationToken cancellationToken);
}