using System.Data.SqlClient;

namespace SSTCommonClasses.Connection
{
    public class MSConnectionChecker : ConnectionChecker
    {
        public override bool CheckConnection(string connectionString)
        {
            bool state = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                finally 
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        state = true;
                    }
                }

                return state;
            }
        }
    }
}
