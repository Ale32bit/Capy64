// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Capy64.API;
using Cyotek.Drawing.BitmapFont;
using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Capy64.Runtime.Objects;

public class TaskMeta : IComponent
{
    private static IGame _game;
    public TaskMeta(IGame game)
    {
        _game = game;
    }

    public const string ObjectType = "Task";

    public enum TaskStatus
    {
        Running,
        Succeeded,
        Failed,
    }
    public class RuntimeTask
    {
        public RuntimeTask(string typeName)
        {
            TypeName = typeName;
        }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string TypeName { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Running;
        public object Result { get; private set; }
        public string Error { get; private set; }

        public void Fulfill<T>(T obj)
        {
            Status = TaskStatus.Succeeded;

            Result = obj;

            _game.LuaRuntime.QueueEvent("task_finish", LK =>
            {
                LK.PushString(Guid.ToString());

                ObjectManager.PushObject(LK, obj);
                LK.SetMetaTable(TypeName);

                LK.PushNil();

                return 3;
            });
        }

        public void Fulfill(Action<Lua> lk)
        {
            Status = TaskStatus.Succeeded;

            Result = lk;

            _game.LuaRuntime.QueueEvent("task_finish", LK =>
            {
                LK.PushString(Guid.ToString());

                lk(LK);

                LK.PushNil();

                return 3;
            });
        }

        public void Reject(string error, params object[] args)
        {
            Status = TaskStatus.Failed;
            Error = string.Format(error, args);

            _game.LuaRuntime.QueueEvent("task_finish", LK =>
            {
                LK.PushString(Guid.ToString());

                LK.PushNil();

                LK.PushString(Error);

                return 3;
            });
        }
    }

    private static LuaRegister[] Methods = new LuaRegister[]
    {
        new()
        {
            name = "await",
            function = L_Await,
        },
        new()
        {
            name = "getID",
            function = L_GetID,
        },
        new()
        {
            name = "getType",
            function = L_GetType,
        },
        new()
        {
            name = "getStatus",
            function = L_GetStatus,
        },
        new()
        {
            name = "getResult",
            function = L_GetResult,
        },
        new()
        {
            name = "getError",
            function = L_GetResult,
        },
        new(),
    };

    private static LuaRegister[] MetaMethods = new LuaRegister[]
    {
        new()
        {
            name = "__index",
        },
        new()
        {
            name = "__gc",
            function = LM_GC,
        },
        new()
        {
            name = "__close",
            function = LM_GC,
        },
        new()
        {
            name = "__tostring",
            function = LM_ToString,
        },

        new(),
    };

    public void LuaInit(Lua L)
    {
        CreateMeta(L);
    }

    public static void CreateMeta(Lua L)
    {
        L.NewMetaTable(ObjectType);
        L.SetFuncs(MetaMethods, 0);
        L.NewLibTable(Methods);
        L.SetFuncs(Methods, 0);
        L.SetField(-2, "__index");
        L.Pop(1);
    }

    public static RuntimeTask Push(Lua L, string typeName)
    {
        var task = new RuntimeTask(typeName);

        ObjectManager.PushObject(L, task);
        L.SetMetaTable(ObjectType);

        return task;
    }

    private static RuntimeTask ToTask(Lua L, bool gc = false)
    {
        return ObjectManager.ToObject<RuntimeTask>(L, 1, gc);
    }

    private static RuntimeTask CheckTask(Lua L, bool gc = false)
    {
        var obj = ObjectManager.CheckObject<RuntimeTask>(L, 1, ObjectType, gc);
        if (obj is null)
        {
            L.Error("attempt to use a closed file");
            return null;
        }
        return obj;
    }

    private static void WaitForTask(Lua L)
    {
        L.PushCFunction(Libraries.Event.L_Pull);
        L.PushString("task_finish");
        L.CallK(1, 4, 0, LK_Await);
    }

    private static int L_Await(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        WaitForTask(L);

        return 0;
    }

    private static int LK_Await(IntPtr state, int status, nint ctx)
    {
        var L = Lua.FromIntPtr(state);
        var task = CheckTask(L, false);
        var taskId = L.CheckString(3);

        if (task.Guid.ToString() != taskId)
        {
            WaitForTask(L);
            return 0;
        }

        return 2;
    }

    private static int L_GetID(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, false);

        L.PushString(task.Guid.ToString());

        return 1;
    }

    private static int L_GetType(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, false);

        L.PushString(task.TypeName);

        return 1;
    }

    private static int L_GetStatus(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, false);

        L.PushString(task.Status.ToString().ToLower());

        return 1;
    }

    private static int L_GetResult(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, false);

        if (task.Status == TaskStatus.Succeeded)
        {
            if (task.Result is Action<Lua> lk)
            {
                // todo: make use of first-generated data
                lk(L);
            }
            else
            {
                ObjectManager.PushObject(L, task.Result);
                L.SetMetaTable(task.TypeName);
            }
        }
        else
        {
            L.PushNil();
        }

        return 1;
    }

    private static int L_GetError(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, false);

        if (task.Status == TaskStatus.Failed)
        {
            L.PushString(task.Error);
        }
        else
        {
            L.PushNil();
        }

        return 1;
    }

    private static int LM_GC(IntPtr state)
    {
        return 0;
    }

    private static int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var task = ToTask(L);
        L.PushString("Task<{0}>: {1} ({2})", task?.TypeName, task?.Guid, task?.Status);

        return 1;
    }
}
