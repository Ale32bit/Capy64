using Capy64.API;
using Capy64.LuaRuntime.Extensions;
using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Capy64.LuaRuntime.Libraries;

public class FileSystem : IPlugin
{
    public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Capy64");
    public static string DataPath = Path.Combine(BasePath, "data");

    public FileSystem()
    {
        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
    }

    // functions to add to the library, always end libraries with null
    private LuaRegister[] FsLib = new LuaRegister[] {
        new()
        {
            name = "list",
            function = L_List,
        },
        new()
        {
            name = "combine",
            function = L_Combine,
        },
        new()
        {
            name = "getName",
            function = L_GetPathName,
        },
        new()
        {
            name = "getDir",
            function = L_GetDirectoryName,
        },
        new()
        {
            name = "getSize",
            function = L_GetFileSize,
        },
        new()
        {
            name = "exists",
            function = L_Exists,
        },
        new()
        {
            name = "isReadOnly",
            function = L_IsReadOnly,
        },
        new()
        {
            name = "makeDir",
            function = L_MakeDir,
        },
        new()
        {
            name = "move",
            function = L_Move,
        },
        new()
        {
            name = "copy",
            function = L_Copy,
        },
        new()
        {
            name = "delete",
            function = L_Delete,
        },
        new()
        {
            name = "getAttributes",
            function = L_GetAttributes,
        },
        new()
        {
            name = "open",
            function = L_Open,
        },
        new(), // NULL
    };

    public void LuaInit(Lua state)
    {
        // Add "fs" library to lua, not global (uses require())
        state.RequireF("fs", Open, false);
    }

    private int Open(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(FsLib);
        return 1;
    }

    public static string SanitizePath(string path)
    {
        // Replace \ to / for cross compatibility in case users prefer to use \
        path = path.Replace("\\", "/");

        // Get drive root (C:\ for Windows, / for *nix)
        var rootPath = Path.GetFullPath(Path.GetPathRoot("/") ?? "/");

        // Join path to rootPath and resolves to absolute path
        // Relative paths are resolved here (es. ../ and ./)
        var absolutePath = Path.GetFullPath(path, rootPath);

        // Trim root from path
        return absolutePath.Remove(0, rootPath.Length);
    }

    public static string Resolve(string path)
    {
        var isolatedPath = SanitizePath(path);

        // Now join the isolatedPath to the Lua directory, always inside of it
        var resolvedPath = Path.Join(DataPath, isolatedPath);

        return resolvedPath;
    }

    public static string CleanOutputPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        var clean = path.Replace(Path.DirectorySeparatorChar, '/');
        return clean;
    }

    public static string TrimBasePath(string path)
    {
        return path;
    }

    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool overwrite = false)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, overwrite);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true, overwrite);
            }
        }
    }

    private static int L_List(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = L.CheckString(1);
        path = Resolve(path);

        if (!Directory.Exists(path))
        {
            L.Error("directory not found");
        }

        var fileList = Directory.EnumerateFileSystemEntries(path);

        var list = new List<string>();
        foreach (var file in fileList)
        {
            var sfile = Path.GetFileName(file);
            list.Add(CleanOutputPath(sfile));
        }

        L.PushArray(list.Order().ToArray());

        return 1;
    }

    private static int L_Combine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var nargs = L.GetTop();

        var parts = new List<string>();
        for (int i = 1; i <= nargs; i++)
        {
            var pathPart = L.CheckString(i);
            parts.Add(pathPart);
        }

        var result = Path.Join(parts.ToArray());
        if (string.IsNullOrEmpty(result))
        {
            L.PushString("");
            return 1;
        }

        result = SanitizePath(result);
        L.PushString(CleanOutputPath(result));

        return 1;
    }

    private static int L_GetPathName(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = L.CheckString(1);

        var result = Path.GetFileName(path);

        L.PushString(CleanOutputPath(result));

        return 1;
    }

    private static int L_GetDirectoryName(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = L.CheckString(1);

        var result = Path.GetDirectoryName(path);

        L.PushString(CleanOutputPath(result));

        return 1;
    }

    private static int L_GetFileSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = L.CheckString(1);

        path = Resolve(path);

        if (!File.Exists(path))
        {
            L.Error("file not found");
        }

        var fileInfo = new FileInfo(path);
        L.PushInteger(fileInfo.Length);

        return 1;
    }

    private static int L_Exists(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = L.CheckString(1);
        var exists = Path.Exists(Resolve(path));

        L.PushBoolean(exists);

        return 1;
    }

    private static int L_IsReadOnly(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var path = Resolve(L.CheckString(1));

        if (File.Exists(path))
        {
            var fileInfo = new FileInfo(path);
            L.PushBoolean(fileInfo.IsReadOnly);
        }
        else if (Directory.Exists(path))
        {
            var dirInfo = new DirectoryInfo(path);
            var isReadOnly = dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            L.PushBoolean(isReadOnly);
        }
        else
        {
            L.Error("path not found");
        }

        return 1;
    }

    private static int L_MakeDir(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = Resolve(L.CheckString(1));

        if (Path.Exists(path))
        {
            L.Error("path already exists");
        }

        Directory.CreateDirectory(path);

        return 0;
    }

    private static int L_Move(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var sourcePath = Resolve(L.CheckString(1));
        var destPath = Resolve(L.CheckString(2));

        if (!Path.Exists(sourcePath))
        {
            L.Error("source path not found");
        }

        if (Path.Exists(destPath))
        {
            L.Error("destination path already exists");
        }

        var attr = File.GetAttributes(sourcePath);
        if (attr.HasFlag(FileAttributes.Directory))
        {
            Directory.Move(sourcePath, destPath);
        }
        else
        {
            File.Move(sourcePath, destPath);
        }

        return 0;
    }

    private static int L_Copy(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var sourcePath = Resolve(L.CheckString(1));
        var destPath = Resolve(L.CheckString(2));

        if (!Path.Exists(sourcePath))
        {
            L.Error("source path not found");
        }

        if (Path.Exists(destPath))
        {
            L.Error("destination path already exists");
        }

        var attr = File.GetAttributes(sourcePath);
        if (attr.HasFlag(FileAttributes.Directory))
        {
            CopyDirectory(sourcePath, destPath, true);
        }
        else
        {
            File.Copy(sourcePath, destPath);
        }

        return 0;
    }

    private static int L_Delete(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = Resolve(L.CheckString(1));
        bool recursive = false;
        if (!L.IsNoneOrNil(2))
        {
            L.CheckType(2, LuaType.Boolean);
            recursive = L.ToBoolean(2);
        }

        if (!Path.Exists(path))
        {
            L.Error("path not found");
        }

        var attr = File.GetAttributes(path);
        if (attr.HasFlag(FileAttributes.Directory))
        {
            Directory.Delete(path, recursive);
        }
        else
        {
            File.Delete(path);
        }

        return 0;
    }

    private static int L_GetAttributes(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = Resolve(L.CheckString(1));

        // { size = number, isDir = boolean, isReadOnly = boolean, created = number, modified = number }
        if (!Path.Exists(path))
        {
            L.Error("path not found");
        }

        var attributes = new Dictionary<string, object>();

        var pathAttributes = File.GetAttributes(path);
        if (pathAttributes.HasFlag(FileAttributes.Directory))
        {
            var dattrs = new DirectoryInfo(path);
            attributes["size"] = 0;
            attributes["isDirectory"] = true;
            attributes["isReadOnly"] = dattrs.Attributes.HasFlag(FileAttributes.ReadOnly);
            attributes["created"] = new DateTimeOffset(dattrs.CreationTimeUtc).ToUnixTimeMilliseconds();
            attributes["modified"] = new DateTimeOffset(dattrs.LastWriteTimeUtc).ToUnixTimeMilliseconds();
        }
        else
        {
            var fattrs = new FileInfo(path);
            attributes["size"] = fattrs.Length;
            attributes["isDirectory"] = false;
            attributes["isReadOnly"] = fattrs.IsReadOnly;
            attributes["created"] = new DateTimeOffset(fattrs.CreationTimeUtc).ToUnixTimeMilliseconds();
            attributes["modified"] = new DateTimeOffset(fattrs.LastWriteTimeUtc).ToUnixTimeMilliseconds();
        }

        L.NewTable();

        foreach (var attribute in attributes)
        {
            L.PushString(attribute.Key);
            L.PushValue(attribute.Value);

            L.SetTable(-3);
        }

        return 1;
    }

    private static int L_Open(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var path = Resolve(L.CheckString(1));
        var mode = L.CheckString(2);


        var errorMessage = "invalid file mode";
        if (mode.Length < 1 && mode.Length > 2)
        {
            L.ArgumentError(2, errorMessage);
            return 0;
        }

        FileMode fileMode;
        FileAccess fileAccess;
        switch (mode[0])
        {
            case 'r':
                if (!File.Exists(path))
                {
                    L.Error("file not found");
                    return 0;
                }
                fileMode = FileMode.Open;
                fileAccess = FileAccess.Read;
                break;
            case 'w':
                fileMode = FileMode.CreateNew;
                fileAccess = FileAccess.Write;
                break;
            case 'a':
                fileMode = FileMode.Append;
                fileAccess = FileAccess.Write;
                break;
            default:
                L.ArgumentError(2, errorMessage);
                return 0;
        }

        bool binaryMode = false;
        if (mode.Length == 2)
        {
            if (mode[1] == 'b')
            {
                binaryMode = true;
            }
            else
            {
                L.ArgumentError(2, errorMessage);
                return 0;
            }
        }

        var handle = File.Open(path, fileMode, fileAccess, FileShare.ReadWrite);
        bool isClosed = false;


        var functions = new Dictionary<string, Func<nint, int>>();
        if (fileAccess == FileAccess.Read)
        {
            var reader = new StreamReader(handle);
            functions["read"] = (IntPtr state) =>
            {
                var L = Lua.FromIntPtr(state);

                var count = (int)L.OptNumber(1, 1);

                if (isClosed)
                {
                    L.Error("file handle is closed");
                    return 0;
                }

                if (reader.EndOfStream)
                {
                    L.PushNil();
                    return 1;
                }

                if (binaryMode)
                {
                    var data = new byte[count];
                    handle.Read(data, 0, count);
                    L.PushBuffer(data);
                }
                else
                {
                    var data = new char[count];
                    reader.ReadBlock(data, 0, count);
                    L.PushString(new string(data));
                }

                return 1;
            };

            functions["readLine"] = (IntPtr state) =>
            {
                var L = Lua.FromIntPtr(state);

                var count = (int)L.OptNumber(1, 1);

                if (isClosed)
                {
                    L.Error("file handle is closed");
                    return 0;
                }

                if (reader.EndOfStream)
                {
                    L.PushNil();
                    return 1;
                }

                if (binaryMode)
                {
                    var line = reader.ReadLine();
                    L.PushBuffer(Encoding.UTF8.GetBytes(line));
                }
                else
                {
                    var line = reader.ReadLine();
                    L.PushString(line);
                }

                return 1;
            };

            functions["readAll"] = (IntPtr state) =>
            {
                var L = Lua.FromIntPtr(state);

                var count = (int)L.OptNumber(1, 1);

                if (isClosed)
                {
                    L.Error("file handle is closed");
                    return 0;
                }

                if (reader.EndOfStream)
                {
                    L.PushNil();
                    return 1;
                }

                if (binaryMode)
                {
                    var content = reader.ReadToEnd();
                    L.PushBuffer(Encoding.UTF8.GetBytes(content));
                }
                else
                {
                    var content = reader.ReadToEnd();
                    L.PushString(content);
                }

                return 1;
            };

        }
        else
        {
            var writer = new StreamWriter(handle);
            functions["write"] = (IntPtr state) =>
            {
                var L = Lua.FromIntPtr(state);

                L.ArgumentCheck(L.IsStringOrNumber(1), 1, "string or number value expected");

                if (isClosed)
                {
                    L.Error("file handle is closed");
                    return 0;
                }

                if (L.IsString(1))
                {
                    if (binaryMode)
                    {
                        handle.Write(L.ToBuffer(1));
                    }
                    else
                    {
                        writer.Write(L.ToString(1));
                    }
                }
                else
                {
                    handle.WriteByte((byte)L.ToInteger(1));
                }

                return 0;
            };

            functions["writeLine"] = (IntPtr state) =>
            {
                var L = Lua.FromIntPtr(state);

                L.ArgumentCheck(L.IsStringOrNumber(1), 1, "string or number value expected");

                if (isClosed)
                {
                    L.Error("file handle is closed");
                    return 0;
                }

                if (L.IsString(1))
                {
                    if (binaryMode)
                    {
                        handle.Write(L.ToBuffer(1));
                        handle.WriteByte((byte)'\n');
                    }
                    else
                    {
                        writer.WriteLine(L.ToString(1));
                    }
                }
                else
                {
                    handle.WriteByte((byte)L.ToInteger(1));
                    handle.WriteByte((byte)'\n');
                }

                return 0;
            };

            functions["flush"] = (IntPtr state) =>
            {
                var L = Lua.FromIntPtr(state);

                if (isClosed)
                {
                    L.Error("file handle is closed");
                    return 0;
                }

                handle.Flush();

                return 0;
            };
        }


        functions["close"] = (IntPtr state) =>
        {
            var L = Lua.FromIntPtr(state);

            if (isClosed)
            {
                L.Error("file handle is closed");
                return 0;
            }

            if (fileAccess != FileAccess.Read)
            {
                handle.Flush();
            }
            handle.Dispose();

            isClosed = true;

            return 0;
        };


        L.NewTable();
        foreach (var pair in functions)
        {
            L.PushString(pair.Key);
            L.PushCFunction(new LuaFunction(pair.Value));
            L.SetTable(-3);
        }

        return 1;
    }

}
