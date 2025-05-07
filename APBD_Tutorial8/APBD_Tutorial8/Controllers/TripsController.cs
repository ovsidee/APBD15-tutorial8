using APBD_Tutorial8.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }
        
    [HttpGet]
    public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
    {
        var trips = await _tripsService.getTripsWithCountriesAsync(cancellationToken);
        return Ok(trips);
    }
}