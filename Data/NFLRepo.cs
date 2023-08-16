using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;

namespace AgilitySportsAPI.Data;
public class NFLRepo : INFLRepo
{
    private readonly IConfiguration configuration;

    public NFLRepo(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    #region NFL

    public async Task<IEnumerable<NFLRoster>> GetAllNFLRoster()
    {
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.GetAllAsync<NFLRoster>();
        }

    }

    #endregion
}