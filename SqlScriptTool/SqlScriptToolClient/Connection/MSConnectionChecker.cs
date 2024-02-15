using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SqlScriptToolClient.Connection
{
    public class MSConnectionChecker : ConnectionChecker
    {
        public override bool CheckConnection(string connectionString)
        {
            bool state = false;

            using (var connection = new SqlConnection(connectionString))
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
