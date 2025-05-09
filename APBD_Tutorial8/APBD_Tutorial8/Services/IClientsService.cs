﻿using APBD_Tutorial8.Models;
using APBD_Tutorial8.Models.DTO_s;

namespace APBD_Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDto>> getClientTripsByIdAsync(int id, CancellationToken cancellationToken);
    Task<int?> CreateClientAsync(Client newClient, CancellationToken cancellationToken);
    Task<string> RegisterClientToTripAsync(int clientId, int tripId, CancellationToken cancellationToken);
    Task<string> UnregisterClientFromTripAsync(int clientId, int tripId, CancellationToken cancellationToken);
}