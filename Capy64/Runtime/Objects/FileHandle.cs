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
using System.IO;
using System.Linq;

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
        var str = ReadNumberHelper.ReadNumber(stream);
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

        var nargs = L.GetTop() - 1;
        if (nargs == 0)
        {
            L.PushString("l");
            nargs = 1;
        }

        for (int i = 2; i <= nargs + 1; i++)
        {
            bool success;
            if (L.Type(i) == LuaType.Number)
            {
                success = ReadChars(L, stream, (int)L.ToNumber(2));
            }
            else
            {
                var p = L.CheckString(i);
                var mode = CheckMode(p);
                switch (mode)
                {
                    case 'n':
                        success = ReadNumber(L, stream);
                        break;
                    case 'l':
                        success = ReadLine(L, stream, true);
                        break;
                    case 'L':
                        success = ReadLine(L, stream, false);
                        break;
                    case 'a':
                        ReadAll(L, stream);
                        success = true;
                        break;
                    default:
                        return L.ArgumentError(i, "invalid format");
                }

            }

            if (!success)
            {
                L.Pop(1);
                L.PushNil();
            }
        }

        return nargs;
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
        return 0;
        var L = Lua.FromIntPtr(state);
        var maxargn = 250;

        var n = L.GetTop() - 1;
        var stream = CheckStream(L, false);
        L.ArgumentCheck(n <= maxargn, maxargn + 2, "too many arguments");
        L.PushCopy(1);
        L.PushInteger(n);
        L.PushBoolean(false);
        L.Rotate(2, 3);
        L.PushCClosure(null, 3 + n); // todo

        return 1;
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

    private static class ReadNumberHelper
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
