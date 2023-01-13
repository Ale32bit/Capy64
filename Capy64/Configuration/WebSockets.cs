using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Configuration;

class WebSockets
{
    public bool Enable { get; set; } = true;
    public int MaxActiveConnections { get; set; } = 5;
}
