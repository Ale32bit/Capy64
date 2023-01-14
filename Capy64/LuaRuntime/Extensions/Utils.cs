using KeraLua;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Capy64.LuaRuntime.Extensions;
public static class Utils
{
    public static void PushArray(this Lua L, object obj)
    {
        var iterable = obj as IEnumerable;

        L.NewTable();
        long i = 1;
        foreach (var item in iterable)
        {
            L.PushValue(item);
            L.RawSetInteger(-2, i++);
        }
        L.SetTop(-1);
    }
#nullable enable
    public static int PushValue(this Lua L, object? obj)
    {
        var type = obj?.GetType();
        switch (obj)
        {
            case string str:
                L.PushString(str);
                break;

            case char:
                L.PushString(obj.ToString());
                break;

            case byte:
            case sbyte:
            case short:
            case ushort:
            case int:
            case uint:
            case double:
                L.PushNumber(Convert.ToDouble(obj));
                break;

            case long l:
                L.PushInteger(l);
                break;

            case bool b:
                L.PushBoolean(b);
                break;

            case null:
                L.PushNil();
                break;

            case byte[] b:
                L.PushBuffer(b);
                break;

            case LuaFunction func:
                L.PushCFunction(func);
                break;

            case IntPtr ptr:
                L.PushLightUserData(ptr);
                break;

            default:
                if (type is not null && type.IsArray)
                {
                    L.PushArray(obj);
                }
                else
                {
                    throw new Exception("Invalid type provided");
                }
                break;

        }
        return 1;
    }

    [Obsolete("This method does not work as intended and requires more research")]
    public static void PushManagedObject<T>(this Lua L, T obj)
    {
        var type = obj.GetType();
        var members = type.GetMembers().Where(m => m.MemberType == MemberTypes.Method);
        L.CreateTable(0, members.Count());
        foreach (var m in members)
        {
            L.PushCFunction(L => (int)type.InvokeMember(m.Name, BindingFlags.InvokeMethod, null, obj, new object[] { L }));
            L.SetField(-2, m.Name);
        }
    }
}
