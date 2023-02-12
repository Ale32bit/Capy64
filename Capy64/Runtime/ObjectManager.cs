using Capy64.API;
using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime;

public class ObjectManager : IPlugin
{
    private static IGame _game;
    public ObjectManager(IGame game)
    {
        _game = game;
    }

    public static unsafe void PushObject<T>(Lua L, T obj)
    {
        if (obj == null)
        {
            L.PushNil();
            return;
        }

        var p = (nint*)L.NewUserData(sizeof(nint));
        var handle = GCHandle.Alloc(obj);
        var op = GCHandle.ToIntPtr(handle);
        *p = op;
        //_objects[p] = obj;
    }

    public static unsafe T ToObject<T>(Lua L, int index, bool freeGCHandle = true)
    {
        if (L.IsNil(index) || !L.IsUserData(index))
            return default(T);

        /*if (p == IntPtr.Zero)
            return default(T);

        /*if (!_objects.ContainsKey(data))
            return default(T);

        var reference = (T)_objects[data];*/

        var p = (nint*)L.ToUserData(index);
        var op = *p;
        var handle = GCHandle.FromIntPtr(op);

        var reference = handle.Target;
        if (reference == null)
            return default(T);

        T value;
        try
        {
            value = (T)reference;
        }
        catch (Exception ex)
        {
            value = default;
        }

        if (freeGCHandle)
            handle.Free();

        return value;
    }

    public static unsafe T CheckObject<T>(Lua L, int argument, string typeName, bool freeGCHandle = true)
    {
        if (L.IsNil(argument) || !L.IsUserData(argument))
            return default(T);

        var p = (nint*)L.CheckUserData(argument, typeName);
        var op = *p;
        var handle = GCHandle.FromIntPtr(op);

        if (!handle.IsAllocated)
            return default;

        var reference = handle.Target;
        if (reference == null)
            return default(T);

        T value;
        try
        {
            value = (T)reference;
        }
        catch (Exception ex)
        {
            value = default;
        }

        if (freeGCHandle)
            handle.Free();

        return value;
    }
}
