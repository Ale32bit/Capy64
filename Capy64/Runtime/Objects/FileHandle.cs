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
using KeraLua;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Capy64.Runtime.Constants;

namespace Capy64.Runtime.Objects;

public class FileHandle : IComponent
{
    public const string ObjectType = "file";

    private static readonly LuaRegister[] Methods = new LuaRegister[]
    {
        new()
        {
            name = "read",
            function = L_Read,
        },
        new()
        {
            name = "write",
            function = L_Write,
        },
        new()
        {
            name = "lines",
            function = L_Lines,
        },
        new()
        {
            name = "flush",
            function = L_Flush,
        },
        new()
        {
            name = "seek",
            function = L_Seek,
        },
        new()
        {
            name = "close",
            function = L_Close,
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

    private static char CheckMode(string mode)
    {
        var modes = "nlLa";
        mode = mode.TrimStart('*');
        if (string.IsNullOrEmpty(mode))
        {
            return '\0';
        }

        var i = modes.IndexOf(mode[0]);
        if (i == -1)
            return '\0';

        return modes[i];
    }

    private static Stream ToStream(Lua L, bool gc = false)
    {
        return ObjectManager.ToObject<Stream>(L, 1, gc);
    }

    private static Stream CheckStream(Lua L, bool gc = false)
    {
        var obj = ObjectManager.CheckObject<Stream>(L, 1, ObjectType, gc);
        if (obj is null)
        {
            L.Error("attempt to use a closed file");
            return null;
        }
        return obj;
    }

    private static bool ReadNumber(Lua L, Stream stream)
    {
        var str = ReadHelper.ReadNumber(stream);
        if (L.StringToNumber(str))
        {
            return true;
        }
        else
        {
            L.PushNil();
            return false;
        }
    }

    private static bool ReadLine(Lua L, Stream stream, bool chop)
    {
        var buffer = new byte[stream.Length];
        int i = 0;
        int c = 0;
        for (; i < stream.Length; i++)
        {
            c = stream.ReadByte();
            if (c == -1 || c == '\n')
                break;
            buffer[i] = (byte)c;
        }

        if (!chop && c == '\n')
            buffer[i++] = (byte)c;

        Array.Resize(ref buffer, i);
        L.PushBuffer(buffer);

        return c == '\n' || L.RawLen(-1) > 0;
    }

    private static void ReadAll(Lua L, Stream stream)
    {
        var buffer = new byte[stream.Length];
        var read = stream.Read(buffer, 0, buffer.Length);
        Array.Resize(ref buffer, read);
        L.PushBuffer(buffer);
    }

    private static bool ReadChars(Lua L, Stream stream, int n)
    {
        var buffer = new byte[n];
        var read = stream.Read(buffer, 0, n);
        Array.Resize(ref buffer, read);
        L.PushBuffer(buffer);
        return read != 0;
    }

    private static int L_Read(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var stream = CheckStream(L);

        if (!stream.CanRead)
        {
            L.PushNil();
            L.PushString("Attempt to read from a write-only stream");
            return 2;
        }

        return G_Read(L, stream, 2);
    }

    private static int G_Read(Lua L, Stream f, int first)
    {
        var nargs = L.GetTop() - 1;
        int n;
        bool success;

        if (nargs == 0) // no arguments?
        {
            success = ReadLine(L, f, true);
            n = first + 1; // to return 1 result
        }
        else
        {
            // ensure stack space for all results and for auxlib's buffer
            L.CheckStack(nargs + MINSTACK, "too many arguments");
            success = true;
            for (n = first; (nargs-- > 0) && success; n++)
            {
                if (L.Type(n) == LuaType.Number)
                {
                    var l = (int)L.CheckInteger(n);
                    success = (l == 0) ? f.Position == f.Length : ReadChars(L, f, l);
                }
                else
                {
                    var p = L.CheckString(n);
                    var mode = CheckMode(p);
                    switch (mode)
                    {
                        case 'n': // number
                            success = ReadNumber(L, f);
                            break;
                        case 'l': // line
                            success = ReadLine(L, f, true);
                            break;
                        case 'L': // line with end-of-line
                            success = ReadLine(L, f, false);
                            break;
                        case 'a': // file
                            ReadAll(L, f); // read entire file
                            success = true; // always success
                            break;
                        default:
                            return L.ArgumentError(n, "invalid format");
                    }

                }
            }
        }

        if (!success)
        {
            L.Pop(1);
            L.PushNil();
        }

        return n - first;
    }

    private static int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var nargs = L.GetTop();

        var stream = CheckStream(L);
        if (!stream.CanWrite)
        {
            L.PushNil();
            L.PushString("Attempt to write to a read-only stream");
            return 2;
        }

        for (int arg = 2; arg <= nargs; arg++)
        {
            if (L.Type(arg) == LuaType.Number)
            {
                stream.WriteByte((byte)L.ToNumber(arg));
            }
            else
            {
                var buffer = L.CheckBuffer(arg);
                stream.Write(buffer);
            }
        }

        return L.FileResult(1, null);
    }

    private static int L_Lines(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var maxargn = 250;

        var n = L.GetTop() - 1;
        var stream = CheckStream(L, false);
        L.ArgumentCheck(n <= maxargn, maxargn + 2, "too many arguments");
        L.PushCopy(1);
        L.PushInteger(n);
        L.PushBoolean(false);
        L.Rotate(2, 3);
        L.PushCClosure(IO_ReadLine, 3 + n);

        return 1;
    }

    private static int IO_ReadLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var stream = ObjectManager.ToObject<Stream>(L, Lua.UpValueIndex(1), false);
        int i;
        int n = (int)L.ToInteger(Lua.UpValueIndex(2));
        if (stream is null)
        {
            return L.Error("file is already closed");
        }
        L.SetTop(1);
        L.CheckStack(n, "too many arguments");
        for (i = 1; i <= n; i++)
        {
            L.PushCopy(Lua.UpValueIndex(3 + i));
        }
        n = G_Read(L, stream, 2);
        Debug.Assert(n > 0);
        if (L.ToBoolean(-n))
        {
            return n;
        }
        else
        {
            if (n > 1)
            {
                return L.Error(L.ToString(-n + 1));
            }
            if (L.ToBoolean(Lua.UpValueIndex(3)))
            {
                L.SetTop(0);
                L.PushCopy(Lua.UpValueIndex(1));
                stream.Close();
            }
        }


        return 0;
    }

    private static int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = CheckStream(L, false);

        stream.Flush();

        return 0;
    }

    private static int L_Seek(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = CheckStream(L, false);

        var whence = L.CheckOption(2, "cur", new[]
        {
            "set", // begin 0
            "cur", // current 1
            "end", // end 2
            null,
        });

        var offset = L.OptInteger(3, 0);

        var newPosition = stream.Seek(offset, (SeekOrigin)whence);

        L.PushInteger(newPosition);

        return 1;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = CheckStream(L, true);
        stream.Close();

        return 0;
    }

    private static unsafe int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var stream = ToStream(L);
        if (stream is not null)
        {
            L.PushString("file ({0:X})", (ulong)&stream);
        }
        else
        {
            L.PushString("file (closed)");
        }
        return 1;
    }

    private static int LM_GC(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = ToStream(L, true);
        stream?.Close();

        return 0;
    }

    private static class ReadHelper
    {
        static bool isdigit(char c)
        {
            return "0123456789"
                .Contains(c, StringComparison.CurrentCultureIgnoreCase);
        }

        static bool isxdigit(char c)
        {
            return "0123456789abcdef"
                .Contains(c, StringComparison.CurrentCultureIgnoreCase);
        }

        static bool isspace(char c)
        {
            return new char[] {
                ' ',
                '\n',
                '\t',
                '\v',
                '\f',
                '\r',
            }.Contains(c);
        }

        static bool test_eof(Lua L, Stream f)
        {
            L.PushString("");
            return f.Position != f.Length;
        }

        static bool nextc(RN rn)
        {
            if (rn.n >= 200) // buffer overflow?
            {
                rn.buff[0] = '\0'; // invalidate result
                return false; // fail
            }
            else
            {
                rn.buff[rn.n] = rn.c; // save current char
                rn.n++;
                rn.c = (char)rn.f.ReadByte(); // read next one
                return true;
            }
        }

        static bool test2(RN rn, string set)
        {
            if (rn.c == set[0] || rn.c == set[1])
            {
                return nextc(rn);
            }
            return false;
        }

        static int readdigits(RN rn, bool hex)
        {
            int count = 0;
            while ((hex ? isxdigit(rn.c) : isdigit(rn.c)) && nextc(rn))
                count++;
            return count;
        }


        /// <summary>
        /// https://www.lua.org/source/5.4/liolib.c.html#read_number
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadNumber(Stream stream)
        {
            RN rn = new()
            {
                buff = new char[201]
            };

            int count = 0;
            bool hex = false;
            string decp = "..";
            rn.f = stream;
            rn.n = 0;

            do
            {
                rn.c = (char)rn.f.ReadByte();
            } while (isspace(rn.c)); /* skip spaces */

            test2(rn, "+-"); /* optional sign */
            if (test2(rn, "00"))
            {
                if (test2(rn, "xX")) hex = true; /* numeral is hexadecimal */
                else count = 1; /* count initial '0' as a valid digit */
            }
            count += readdigits(rn, hex); // integral part
            if (test2(rn, decp))  // decimal point?
                count += readdigits(rn, hex);  // fractional part
            if (count > 0 && test2(rn, (hex ? "pP" : "eE")))
            {  /* exponent mark? */
                test2(rn, "-+");  /* exponent sign */
                readdigits(rn, false);  /* exponent digits */
            }
            rn.f.Position += -1;
            return new string(rn.buff);
        }

        public class RN
        {
            public Stream f;
            public char c;
            public int n;
            public char[] buff;
        }
    }
}
