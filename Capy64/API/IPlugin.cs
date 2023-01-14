using KeraLua;
using Microsoft.Extensions.DependencyInjection;

namespace Capy64.API;

public interface IPlugin
{
    void ConfigureServices(IServiceCollection services) { }
    void LuaInit(Lua L) { }

}
