using APBD_Tutorial8.Models;
using APBD_Tutorial8.Models.DTO_s;
using Microsoft.Data.SqlClient;

namespace APBD_Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string ConnectionString = "Data Source=localhost, 1433; User=sa; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";

    public async Task<List<ClientTripDto>?> getClientTripsByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var con = new SqlConnection(ConnectionString);
        using var comForSearchClient = new SqlCommand();
        
        await con.OpenAsync(cancellationToken);
        
        // check if client exists
        comForSearchClient.Connection = con;
        comForSearchClient.CommandText = "SELECT COUNT(1) FROM Client WHERE IdClient = @IdClient;";
        comForSearchClient.Parameters.AddWithValue("@IdClient", id);
        int exists = (int) await comForSearchClient.ExecuteScalarAsync(cancellationToken);

        if (exists == 0)
            return null;
        
        //searching for trips for this client
        var query = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo,
                   ct.RegisteredAt, ct.PaymentDate
            FROM Client_Trip ct
            JOIN Trip t ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @IdClient";

        using var comForSearchTrips = new SqlCommand();
        comForSearchTrips.Connection = con;
        comForSearchTrips.CommandText = query;
        comForSearchTrips.Parameters.AddWithValue("@IdClient", id);

        SqlDataReader reader = await comForSearchTrips.ExecuteReaderAsync(cancellationToken);
        var trips = new List<ClientTripDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            trips.Add(new ClientTripDto
            {
                IdTrip = (int)reader["IdTrip"],
                Name = (string)reader["Name"],
                Description = reader.IsDBNull(2) ? "" : (string)reader["Description"],
                DateFrom = (DateTime)reader["DateFrom"],
                DateTo = (DateTime)reader["DateTo"],
                RegisteredAt = new DateTime((int)reader["RegisteredAt"]),
                PaymentDate = reader.IsDBNull(6) ? null : new DateTime?(new DateTime((int)reader["PaymentDate"]))
            });
        }

        return trips;
    }
    
    public async Task<int?> CreateClientAsync(Client newClient, CancellationToken cancellationToken)
    {
        await using var con = new SqlConnection(ConnectionString);
        await using var com = new SqlCommand
        {
            Connection = con,
            CommandText = @"
                INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) 
                VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                SELECT SCOPE_IDENTITY();"
        };

        com.Parameters.AddWithValue("@FirstName", newClient.FirstName);
        com.Parameters.AddWithValue("@LastName", newClient.LastName);
        com.Parameters.AddWithValue("@Email", newClient.Email);
        com.Parameters.AddWithValue("@Telephone", (object?)newClient.Telephone ?? DBNull.Value);
        com.Parameters.AddWithValue("@Pesel", (object?)newClient.Pesel ?? DBNull.Value);

        await con.OpenAsync(cancellationToken);
        var result = await com.ExecuteScalarAsync(cancellationToken);

        return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    public async Task<string> RegisterClientToTripAsync(int clientId, int tripId, CancellationToken cancellationToken)
    {
        await using var con = new SqlConnection(ConnectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        //get all info
        com.CommandText = @"
            SELECT COUNT(*) FROM Client WHERE IdClient = @ClientId;
            SELECT COUNT(*) FROM Trip WHERE IdTrip = @TripId;
            SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId;
            SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId;
        ";
        com.Parameters.AddWithValue("@ClientId", clientId);
        com.Parameters.AddWithValue("@TripId", tripId);

        await con.OpenAsync(cancellationToken);
        var reader = await com.ExecuteReaderAsync(cancellationToken);

        // if client exists
        await reader.ReadAsync(cancellationToken);
        if ((int)reader[0] == 0)
            return "ClientNotFound";
        
        // if trip exists
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        if ((int)reader[0] == 0)
            return "TripNotFound";
        
        // count registrations
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int currentRegistrations = (int)reader[0];

        // get maxPeople
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int maxPeople = (int)reader[0];

        await reader.CloseAsync();

        if (currentRegistrations >= maxPeople)
            return "TripFull";
        
        int registeredAt = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

        // register newClient
        com.CommandText = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
            VALUES (@ClientId, @TripId, @RegisteredAt, @PaymentDate);
        ";
        com.Parameters.Clear();
        com.Parameters.AddWithValue("@ClientId", clientId);
        com.Parameters.AddWithValue("@TripId", tripId);
        com.Parameters.AddWithValue("@RegisteredAt", registeredAt);
        com.Parameters.AddWithValue("@PaymentDate", DBNull.Value); 

        try
        {
            await com.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) 
        {
            if (ex.Number == 2627)
                return "AlreadyRegistered";    
        }

        return "Success";
    }
    
    public async Task<string> UnregisterClientFromTripAsync(int clientId, int tripId, CancellationToken cancellationToken)
    {
        await using var con = new SqlConnection(ConnectionString);
        await using var com = new SqlCommand();
        com.Connection = con;
        com.CommandText = @"DELETE FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId";
        com.Parameters.AddWithValue("@ClientId", clientId);
        com.Parameters.AddWithValue("@TripId", tripId);

        await con.OpenAsync(cancellationToken);
        
        int affectedRows = await com.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows == 0 ? "NotRegistered" : "Success";
    }
}