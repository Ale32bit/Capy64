using KeraLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Extensions;
public static class Utils
{
    public static void PushArray(this Lua state, object obj)
    {
        var iterable = obj as IEnumerable;

        state.NewTable();
        long i = 1;
        foreach (var item in iterable)
        {
            state.PushValue(item);
            state.RawSetInteger(-2, i++);
        }
        state.SetTop(-1);
    }
#nullable enable
    public static int PushValue(this Lua state, object? obj)
    {
        var type = obj?.GetType();
        switch (obj)
        {
            case string str:
                state.PushString(str);
                break;

            case char:
                state.PushString(obj.ToString());
                break;

            case byte:
            case sbyte:
            case short:
            case ushort:
            case int:
            case uint:
            case double:
                state.PushNumber(Convert.ToDouble(obj));
                break;

            case long l:
                state.PushInteger(l);
                break;

            case bool b:
                state.PushBoolean(b);
                break;

            case null:
                state.PushNil();
                break;

            case byte[] b:
                state.PushBuffer(b);
                break;
            
            case LuaFunction func:
                state.PushCFunction(func);
                break;

            case IntPtr ptr:
                state.PushLightUserData(ptr);
                break;

            default:
                if (type is not null && type.IsArray)
                {
                    state.PushArray(obj);
                }
                else
                {
                    throw new Exception("Invalid type provided");
                }
                break;

        }
        return 1;
    }

    public static void PushManagedObject<T>(this Lua state, T obj)
    {
        var type = obj.GetType();
        var members = type.GetMembers().Where(m => m.MemberType == MemberTypes.Method);
        state.CreateTable(0, members.Count());
        foreach (var m in members)
        {
            state.PushCFunction(L => (int)type.InvokeMember(m.Name, BindingFlags.InvokeMethod, null, obj, new object[] { L }));
            state.SetField(-2, m.Name);
        }
    }
}
