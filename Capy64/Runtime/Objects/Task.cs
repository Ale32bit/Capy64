﻿// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
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
using KeraLua;
using System;

namespace Capy64.Runtime.Objects;

public class TaskMeta : IComponent
{
    private static Capy64 _game;
    public TaskMeta(Capy64 game)
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
        public RuntimeTask(string name = "object")
        {
            Name = name;
        }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public nint Pointer { get; set; } = nint.Zero;
        public string Name { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Running;
        public string Error { get; private set; }
        public int DataIndex { get; private set; } = 0;
        public bool UserTask { get; set; } = false;

        /// <summary>
        /// Pops one element at the top of the container (lk) stack and uses it as task value.
        /// 
        /// The container must contain a value and cannot be nil.
        /// </summary>
        /// <param name="lk"></param>
        /// <exception cref="Exception">Throws if the value nil or the stack is empty</exception>
        public void Fulfill(Action<Lua> lk)
        {
            Status = TaskStatus.Succeeded;

            var container = tasks.NewThread();
            lk(container);
            if (container.IsNoneOrNil(-1))
            {
                throw new Exception("Task result cannot be nil");
            }
            container.XMove(tasks, 1);
            tasks.Remove(-2);
            var emptySpot = FindSpot();
            if (emptySpot > -1)
            {
                tasks.Replace(emptySpot);
                DataIndex = emptySpot;
            }
            else
            {
                DataIndex = tasks.GetTop();
            }

            _game.LuaRuntime.QueueEvent("task_finish", LK =>
            {
                LK.PushString(Guid.ToString());

                tasks.PushCopy(DataIndex);
                tasks.XMove(LK, 1);

                LK.PushNil();

                return 3;
            });
        }

        /// <summary>
        /// Rejects the task with an error message.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="args"></param>
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

    private static readonly LuaRegister[] Methods = new LuaRegister[]
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
            function = L_GetError,
        },
        new(),
    };

    private static readonly LuaRegister[] MetaMethods = new LuaRegister[]
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

    private static Lua tasks;
    public void LuaInit(Lua L)
    {
        tasks = _game.LuaRuntime.Parent.NewThread();

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
        if (!tasks.CheckStack(1))
        {
            L.Error("tasks limit exceeded");
        }

        var task = new RuntimeTask(typeName);

        L.NewTable();
        task.Pointer = ObjectManager.PushObject(L, task);
        L.SetMetaTable(ObjectType);
        L.SetField(-2, "task");

        L.SetMetaTable(ObjectType);

        return task;
    }

    public static RuntimeTask ToTask(Lua L, int index = 1, bool gc = false)
    {
        RuntimeTask task;
        if (L.Type(index) != LuaType.Table)
        {
            if (L.TestUserData(index, ObjectType) == nint.Zero)
            {
                return null;
            }
            else
            {
                task = ObjectManager.ToObject<RuntimeTask>(L, index, gc);

            }
        }
        else
        {
            L.GetField(index, "task");
            task = task = ObjectManager.ToObject<RuntimeTask>(L, -1, gc);
        }
        return task;
    }

    public static RuntimeTask CheckTask(Lua L, int index = 1, bool gc = false)
    {
        RuntimeTask task;
        if (L.Type(index) != LuaType.Table)
        {
            task = ObjectManager.CheckObject<RuntimeTask>(L, index, ObjectType, gc);
        }
        else
        {
            L.GetField(index, "task");
            task = ObjectManager.CheckObject<RuntimeTask>(L, -1, ObjectType, gc);
        }

        if (task is null)
        {
            L.Error("attempt to use a closed task");
            return null;
        }
        return task;
    }

    private static int FindSpot()
    {
        var top = tasks.GetTop();
        for (int i = 1; i <= top; i++)
        {
            if (tasks.IsNil(i))
                return i;
        }

        return -1;
    }

    private static void GCTasks()
    {
        var top = tasks.GetTop();
        int peak = 0;
        for (int i = top; i > 0; i--)
        {
            if (!tasks.IsNil(i))
            {
                peak = i;
                break;
            }
        }

        tasks.SetTop(peak);
    }

    private static void WaitForTask(Lua L)
    {
        L.PushString("coroutine");
        L.GetTable(-1);
        L.GetField(-1, "yield");
        L.PushString("task_finish");
        L.CallK(1, 4, 0, LK_Await);
    }

    private static int L_Await(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.Warning("Native task awaiter should be avoided", false);

        var task = CheckTask(L, 1, false);

        if (task.Status == TaskStatus.Succeeded)
        {
            // Push copy of value on top and move it to L
            tasks.PushCopy(task.DataIndex);
            tasks.XMove(L, 1);

            L.PushNil();
        }
        else if (task.Status == TaskStatus.Failed)
        {
            L.PushNil();
            L.PushString(task.Error);
        }
        else if (task.Status == TaskStatus.Running)
        {
            WaitForTask(L);
            return 0;
        }

        return 2;
    }

    private static int LK_Await(IntPtr state, int status, nint ctx)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, 1, false);

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

        var task = CheckTask(L, 1, false);

        L.PushString(task.Guid.ToString());

        return 1;
    }

    private static int L_GetType(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, 1, false);

        L.PushString(task.Name);

        return 1;
    }

    private static int L_GetStatus(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, 1, false);

        L.PushString(task.Status.ToString().ToLower());

        return 1;
    }

    private static int L_GetResult(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = CheckTask(L, 1, false);

        if (task.Status == TaskStatus.Succeeded)
        {
            // Push copy of value on top and move it to L
            tasks.PushCopy(task.DataIndex);
            tasks.XMove(L, 1);
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

        var task = CheckTask(L, 1, false);

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
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);

        L.GetField(1, "task");

        var task = CheckTask(L, -1, true);
        if (task is null)
            return 0;

        if (task.DataIndex >= 0)
        {
            tasks.PushNil();
            tasks.Replace(task.DataIndex);
        }

        GCTasks();

        return 0;
    }

    private static int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var task = ToTask(L);

        L.PushString("Task<{0}>: {1} ({2})", task?.Name, task?.Guid, task?.Status);

        return 1;
    }
}
