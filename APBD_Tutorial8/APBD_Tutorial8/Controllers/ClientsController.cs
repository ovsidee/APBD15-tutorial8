using APBD_Tutorial8.Models;
using APBD_Tutorial8.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }
    
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> getClientTripsByIdAsync(int id, CancellationToken cancellationToken)
    {
        var trips = await _clientsService.getClientTripsByIdAsync(id, cancellationToken);

        if (trips == null)
            return NotFound($"Client with ID {id} does not exist.");

        if (trips.Count == 0)
            return Ok("Client exists but has no trips registered.");

        return Ok(trips);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] Client newClient, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newClient.FirstName) ||
            string.IsNullOrWhiteSpace(newClient.LastName) ||
            string.IsNullOrWhiteSpace(newClient.Email) ||
            string.IsNullOrWhiteSpace(newClient.Pesel) ||
            string.IsNullOrWhiteSpace(newClient.Telephone))
        {
            return BadRequest("FirstName, LastName, Email, Pesel and Telephone are required.");
        }

        var newClientId = await _clientsService.CreateClientAsync(newClient, cancellationToken);

        if (newClientId is null)
            return StatusCode(500, "Failed to create client.");

        return Created(string.Empty, new { IdClient = newClientId });
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId, CancellationToken cancellationToken)
    {
        var result = await _clientsService.RegisterClientToTripAsync(id, tripId, cancellationToken);

        return result switch
        {
            "ClientNotFound" => NotFound("Client not found."),
            "TripNotFound" => NotFound("Trip not found."),
            "TripFull" => BadRequest("Trip has reached the maximum number of participants."),
            "AlreadyRegistered" => Conflict("Client is already registered for this trip."),
            "Success" => Ok("Client registered to trip successfully."),
            _ => StatusCode(500, "An error occurred while registering the client.")
        };
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> UnregisterClientFromTrip(int id, int tripId, CancellationToken cancellationToken)
    {
        var result = await _clientsService.UnregisterClientFromTripAsync(id, tripId, cancellationToken);

        return result switch
        {
            "NotRegistered" => NotFound("Registration not found."),
            "Success" => Ok("Client successfully unregistered from trip."),
            _ => StatusCode(500, "An error occurred while unregistering the client.")
        };
    }
}