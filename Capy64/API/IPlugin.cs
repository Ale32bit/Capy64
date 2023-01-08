using KeraLua;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.API;

public interface IPlugin
{
    void ConfigureServices(IServiceCollection services) { }
    void LuaInit(Lua L) { }

}
