using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eight.Module {
    interface IModule {
        bool ThreadReady {
            get;
        }
        public void Init(Lua state);
    }
}
