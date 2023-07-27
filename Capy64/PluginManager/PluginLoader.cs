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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Capy64.PluginManager;

internal class PluginLoader
{
    private static Assembly LoadPlugin(string fileName)
    {
        var path = Path.Combine(Environment.CurrentDirectory, fileName);

        var loadContext = new PluginLoadContext(path);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
    }

    public static List<IComponent> LoadAllPlugins(string pluginsPath)
    {
        if (!Directory.Exists(pluginsPath))
            Directory.CreateDirectory(pluginsPath);

        var plugins = new List<IComponent>();
        foreach (var fileName in Directory.GetFiles(pluginsPath).Where(q => q.EndsWith(".dll")))
        {
            var assembly = LoadPlugin(fileName);

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IComponent).IsAssignableFrom(type))
                {
                    IComponent result = Activator.CreateInstance(type, Capy64.Instance) as IComponent;
                    plugins.Add(result);
                }
            }

        }

        return plugins;
    }
}
