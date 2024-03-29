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

using Capy64.Runtime.Extensions;
using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Capy64.Runtime;

internal class Sandbox
{
    internal static void OpenLibraries(Lua L)
    {
        L.OpenBase();
        L.OpenCoroutine();
        L.OpenDebug();
        L.OpenMath();
        L.OpenString();
        L.OpenTable();
        L.OpenUTF8();
        L.OpenOS();

        L.OpenPackage();

        L.SetTop(0);
    }
    internal static void Patch(Lua L)
    {
        // patch package
        L.GetGlobal("package");

        // package.cpatch: empty paths
        L.PushString("cpath");
        L.PushString("");
        L.SetTable(-3);

        // package.path: local paths only
        L.PushString("path");
        L.PushString("./?.lua;./?/init.lua");
        L.SetTable(-3);

        // package.loadlib: remove func to load C libs
        L.PushString("loadlib");
        L.PushNil();
        L.SetTable(-3);

        L.PushString("searchpath");
        L.PushCFunction(L_PatchedSearchpath);
        L.SetTable(-3);

        // package.config: apply unix directory separator
        L.PushString("config");
        L.GetTable(-2);
        var packageConfig = L.ToString(-1);
        packageConfig = string.Concat("/", packageConfig.AsSpan(1));
        L.PushString("config");
        L.PushString(packageConfig);
        L.SetTable(-4);
        L.Pop(1);

        // delete 3 and 4 searchers
        L.PushString("searchers");
        L.GetTable(-2);

        L.PushNil();
        L.SetInteger(-2, 3);
        L.PushNil();
        L.SetInteger(-2, 4);

        // replace searcher 2 with a custom sandboxed one
        L.PushCFunction(L_Searcher);
        L.SetInteger(-2, 2);

        L.Pop(2);

        // Replace loadfile with sandboxed one
        L.PushCFunction(L_Loadfile);
        L.SetGlobal("loadfile");

        // Replace dofile with sandboxed one
        L.PushCFunction(L_Dofile);
        L.SetGlobal("dofile");

        // yeet dangerous os functions
        L.GetGlobal("os");

        L.PushString("execute");
        L.PushNil();
        L.SetTable(-3);

        L.PushString("tmpname");
        L.PushNil();
        L.SetTable(-3);

        L.PushString("remove");
        L.PushNil();
        L.SetTable(-3);

        L.PushString("rename");
        L.PushNil();
        L.SetTable(-3);

        L.PushString("getenv");
        L.PushNil();
        L.SetTable(-3);

        L.Pop(1);

        // Replace debug.debug with a dummy function to avoid stalling the program
        L.GetGlobal("debug");

        L.PushString("debug");
        L.PushCFunction(L_Dummy);
        L.SetTable(-3);

        L.Pop(1);
    }
    
    internal static int L_Dummy(IntPtr state)
    {
        return 0;
    }

    internal static int L_Searcher(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var libname = L.CheckString(1);

        L.GetGlobal("package");

        // get searcher path as arg for package.searchpath
        L.PushString("path");
        L.GetTable(-2);
        var searchpath = L.ToString(-1);

        // get package.searchpath
        L.PushString("searchpath");
        L.GetTable(-3);

        L.PushString(libname);
        L.PushString(searchpath);

        L.Call(2, 2);

        if (L.IsNil(-2))
        {
            return 1;
        }

        // path resolved by package.searchpath
        var foundPath = L.ToString(-2);

        L.GetGlobal("loadfile");
        L.PushString(foundPath);
        L.Call(1, 2);

        if (L.IsNil(-2))
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendFormat("error loading module '{0}' from file '{1}':", libname, foundPath);
            errorMessage.Append('\t');
            errorMessage.AppendLine(L.ToString(-1));

            L.PushString(errorMessage.ToString());

            return 1;
        }

        L.Remove(-1);

        L.PushString(foundPath);

        return 2;
    }

    internal static int L_PatchedSearchpath(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var libname = L.CheckString(1);
        var searchpath = L.CheckString(2);

        libname = libname.Replace('.', '/');

        var possiblePaths = searchpath
            .Split(';')
            .Select(p => p.Replace("?", libname));

        var errorMessage = new StringBuilder();
        foreach (var possiblePath in possiblePaths)
        {
            var path = FileSystemLib.Resolve(possiblePath);
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                errorMessage.AppendLine(string.Format("no file '{0}'", possiblePath));
                continue;
            }

            try
            {
                File.Open(path, FileMode.Open, FileAccess.Read).Dispose();
            }
            catch
            {
                errorMessage.AppendLine(string.Format("error opening file '{0}'", possiblePath));
                continue;
            }

            L.PushString(possiblePath);
            return 1;
        }

        L.PushNil();
        L.PushString(errorMessage.ToString());

        return 2;
    }

    internal static int L_Loadfile(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var filename = L.CheckString(1);

        bool hasMode = !L.IsNone(2);
        bool hasEnv = !L.IsNone(3);

        var path = FileSystemLib.Resolve(filename);

        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            L.PushNil();
            L.PushString($"cannot open {filename}: No such file or directory");
            return 2;
        }
        else if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
        {
            L.PushNil();
            L.PushString($"cannot read {filename}: Is a directory");
            return 2;
        }

        var chunk = File.ReadAllBytes(path);

        L.GetGlobal("load");
        L.PushBuffer(chunk);
        L.PushString("@" + filename);

        var values = 2;

        if (hasMode)
        {
            L.PushCopy(2);
            values++;
        }

        if (hasEnv)
        {
            L.PushCopy(3);
            values++;
        }

        L.Call(values, 2);

        return 2;
    }

    internal static int LK_Dofile(IntPtr state, int status, IntPtr ctx)
    {
        var L = Lua.FromIntPtr(state);

        var nargs = L.GetTop();

        return nargs;
    }

    internal static int L_Dofile(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var filename = L.CheckString(1);

        L.GetGlobal("loadfile");
        L.PushString(filename);

        L.Call(1, 2);

        if (L.IsNil(-2))
        {
            L.Error(L.ToString(-1));
            return 0;
        }

        L.PushCopy(-2);
        L.Insert(1);
        L.SetTop(1);

        L.CallK(0, Constants.MULTRET, 0, LK_Dofile);
        return 0;
    }

    internal static void DumpStack(Lua state)
    {
        var n = state.GetTop();
        for (int i = n; i >= 1; i--)
        {
            Console.WriteLine("{0,4}: {1}", -i, state.ToString(-i));
        }
        Console.WriteLine("--------------");
    }

}