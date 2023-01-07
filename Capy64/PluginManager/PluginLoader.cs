﻿using Capy64.API;
using Microsoft.Extensions.DependencyInjection;
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

    public static List<IPlugin> LoadAllPlugins(string pluginsPath, IServiceProvider provider)
    {
        if (!Directory.Exists(pluginsPath))
            Directory.CreateDirectory(pluginsPath);

        var plugins = new List<IPlugin>();
        foreach (var fileName in Directory.GetFiles(pluginsPath).Where(q => q.EndsWith(".dll")))
        {
            var assembly = LoadPlugin(fileName);

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    IPlugin result = ActivatorUtilities.CreateInstance(provider, type) as IPlugin;
                    plugins.Add(result);
                }
            }

        }

        return plugins;
    }
}
