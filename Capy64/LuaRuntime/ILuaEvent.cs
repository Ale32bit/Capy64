using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime;

public interface ILuaEvent
{
    public string Name { get; set; }
    public bool BypassFilter { get; set; }
}
