using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Server.Services.EntityScope.Data
{
    public class ClientScope
    {
        public Guid ClientId { get; }

        public List<string> Entities { get; } = new List<string>();

        public ClientScope(Guid clientId) 
        {
            ClientId = clientId;
        }
    }
}
