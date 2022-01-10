using Eight.Extensions;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // LuaState.OpenDebug();
            // LuaState.OpenIO();
            LuaState.OpenMath();
            // LuaState.OpenOS();
            // LuaState.OpenPackage();
            LuaState.OpenString();
            LuaState.OpenTable();
            LuaState.OpenUTF8();

            LuaState.PushString("Eight 2 Alpha");
            LuaState.SetGlobal("_HOST");

            Thread = LuaState.NewThread();

            //LuaState.LoadFile();
        }

        public bool Resume()
        {
            var status = Thread.Resume(null, parametersCount);

            parametersCount = 0;

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
                }

                parametersCount++;
            }
        }
    }
}
