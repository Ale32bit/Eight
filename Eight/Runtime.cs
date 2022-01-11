using Eight.Extensions;
using KeraLua;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eight
{
    class Runtime
    {
        public Lua LuaState;
        public Lua Thread;

        private int parametersCount = 0;

        public Runtime()
        {
            LuaState = new(false)
            {
                Encoding = Encoding.UTF8,
            };

            // Open standard packages except unsafe ones
            LuaState.OpenBase();
            LuaState.OpenCoroutine();
            LuaState.OpenDebug();
            LuaState.OpenIO();
            LuaState.OpenMath();
            LuaState.OpenOS();
            LuaState.OpenPackage();
            LuaState.OpenString();
            LuaState.OpenTable();
            LuaState.OpenUTF8();

            LuaState.PushString("Eight 2 Alpha");
            LuaState.SetGlobal("_HOST");

            Thread = LuaState.NewThread();
        }

        public void LoadInit()
        {
            var status = Thread.LoadFile("Lua/init.lua");
            if (status != LuaStatus.OK)
            {
                throw new LuaException(Thread.ToString(-1));
            }
        }

        /// <summary>
        /// Resume the Lua thread
        /// </summary>
        /// <returns>Whether is yielding</returns>
        public bool Resume()
        {
            var status = Thread.Resume(null, parametersCount, out int pars);
            parametersCount = 0;
            if (status == LuaStatus.Yield || status == LuaStatus.OK)
            {
                Thread.Pop(pars);
                return status == LuaStatus.Yield;

            }

            var error = Thread.OptString(-1, "Unknown exception");
            Thread.Traceback(Thread);
            var stacktrace = Thread.OptString(-1, "");

            Console.WriteLine("Top thread exception:\n{0}\n{1}", error, stacktrace);

            return false;
        }

        public void PushParameters(params object[] pars)
        {
            foreach (object par in pars)
            {
                switch (par)
                {
                    case string s:
                        Thread.PushString(s);
                        break;

                    case double d:
                        Thread.PushNumber(d);
                        break;

                    case long l:
                        Thread.PushInteger(l);
                        break;

                    case bool b:
                        Thread.PushBoolean(b);
                        break;

                    case null:
                        Thread.PushNil();
                        break;

                    case byte[] b:
                        Thread.PushBuffer(b);
                        break;

                    case LuaFunction func:
                        Thread.PushCFunction(func);
                        break;

                    case IntPtr ptr:
                        Thread.PushLightUserData(ptr);
                        break;

                    default:
                        throw new Exception("Invalid type provided");
                }


                parametersCount++;
            }
        }

        public void PushCClosure(LuaFunction function, int n)
        {
            Thread.PushCClosure(function, n);
            parametersCount++;
        }

        public void PushObject<T>(T obj)
        {
            Thread.PushObject<T>(obj);
            parametersCount++;
        }

    }
}
