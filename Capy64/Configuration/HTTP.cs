using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Configuration;

class HTTP
{
    public bool Enable { get; set; } = true;
    public string[] Blacklist { get; set; }
    public WebSockets WebSockets { get; set; }
}
