using APBD_Tutorial8.Models.DTO_s;
using Microsoft.Data.SqlClient;

namespace APBD_Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string ConnectionString = "Data Source=localhost, 1433; User=sa; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    
    public async Task<List<TripDto>> getTripsWithCountriesAsync(CancellationToken cancellationToken)
    {
        await using var con = new SqlConnection(ConnectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName FROM Trip t JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON ct.IdCountry = c.IdCountry ORDER BY t.IdTrip;";
        
        await con.OpenAsync(cancellationToken);

        SqlDataReader reader = await com.ExecuteReaderAsync(cancellationToken);
        
        var tripDict = new Dictionary<int, TripDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            int idTrip = (int)reader["IdTrip"];

            if (!tripDict.ContainsKey(idTrip))
            {
                tripDict[idTrip] = new TripDto
                {
                    IdTrip = idTrip,
                    Name = (string)reader["Name"],
                    Description = (string)reader["Description"],
                    DateFrom = (DateTime)reader["DateFrom"],
                    DateTo = (DateTime)reader["DateTo"],
                    MaxPeople = (int)reader["MaxPeople"],
                    Countries = new List<string>()
                };

                tripDict[idTrip].DateRange = $"{tripDict[idTrip].DateFrom:yyyy-MM-dd} - {tripDict[idTrip].DateTo:yyyy-MM-dd}";
            }

            tripDict[idTrip].Countries.Add((string)reader["CountryName"]);
        }
        return tripDict.Values.ToList();
    }
}