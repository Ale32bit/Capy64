using Capy64.API;
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
    private static ConcurrentDictionary<nint, object> _objects = new();
    private static IGame _game;
    public ObjectManager(IGame game)
    {
        _game = game;
        _game.EventEmitter.OnClose += OnClone;
    }

    public static void PushObject<T>(Lua L, T obj)
    {
        if (obj == null)
        {
            L.PushNil();
            return;
        }

        L.NewTable();
        var rp = L.ToPointer(-1);

        //var handle = GCHandle.Alloc(obj);
        //var op = GCHandle.ToIntPtr(handle);

        _objects[rp] = obj;
    }

    public static T ToObject<T>(Lua L, int index, bool freeGCHandle = true)
    {
        if (L.IsNil(index) || !L.IsTable(index))
            return default(T);

        var rp = L.ToPointer(index);
        if (rp == IntPtr.Zero)
            return default(T);

        if (!_objects.ContainsKey(rp))
            return default(T);

        var obj = _objects[rp];
        if(obj == null)
            return default(T);

        if (freeGCHandle)
            _objects.Remove(rp, out _);

        return (T)obj;

        /*var handle = GCHandle.FromIntPtr(Objects[rp]);
        if (!handle.IsAllocated)
            return default(T);

        var reference = (T)handle.Target;

        if (freeGCHandle)
            handle.Free();

        return reference;*/
    }

    public static T CheckObject<T>(Lua L, int argument, string typeName, bool freeGCHandle = true)
    {
        if (L.IsNil(argument) || !L.IsTable(argument))
            return default(T);

        if(L.GetMetaField(argument, "__name") != LuaType.String)
            return default(T);

        var mtName = L.ToString(-1);
        L.Pop(1);

        if(mtName != typeName)
            return default(T);

        var rp = L.ToPointer(argument);
        if (rp == IntPtr.Zero)
            return default(T);

        if (!_objects.ContainsKey(rp))
            return default(T);

        var obj = _objects[rp];
        if (obj == null)
            return default(T);

        if (freeGCHandle)
            _objects.Remove(rp, out _);

        return (T)obj;

        /*

        var handle = GCHandle.FromIntPtr(Objects[rp]);
        if (!handle.IsAllocated)
            return default(T);

        var reference = (T)handle.Target;

        if (freeGCHandle)
            handle.Free();

        return reference;*/
    }

    private void OnClone(object sender, EventArgs e)
    {
        _objects.Clear();
    }
}
