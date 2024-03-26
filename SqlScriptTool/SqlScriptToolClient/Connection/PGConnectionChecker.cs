using Npgsql;

namespace SqlScriptToolClient.Connection
{
    public class PGConnectionChecker : ConnectionChecker
    {
        public override bool CheckConnection(string connectionString)
        {
            try
            {
                using (var source = NpgsqlDataSource.Create(connectionString))
                {
                    var connection = source.OpenConnection();

                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch 
            {
                return false;
            }
        }
    }
}
