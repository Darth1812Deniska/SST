using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptToolClient.Connection
{
    public class ConnectionChecker
    {
        public virtual bool CheckConnection(string connectionString) { return false; }
    }
}
