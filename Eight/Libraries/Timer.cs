using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eight.Libraries;
class Timer : ILibrary
{
    public string Name => "timer";
    public bool Global => false;

    public LuaRegister[] Registers => new LuaRegister[] {
        new() {
            name = "start",
            function = L_Start,
        },
        new() // NULL
    };

    private static int _timerId = 0;

    private static int L_Start(IntPtr luaState)
    {
        var state = Lua.FromIntPtr(luaState);

        var delay = state.CheckNumber(1);
        state.ArgumentCheck(delay > 0, 1, "delay must be greater than 0");

        var timerId = _timerId++;
        var timer = new System.Timers.Timer
        {
            AutoReset = false,
            Enabled = true,
            Interval = delay,
        };

        timer.Elapsed += (o, e) => {
            Program.EnqueueEvent("timer", timerId);
        };

        state.PushNumber(timerId);

        return 1;
    }
}