using Eight;
using System.Text;

var runtime = new Runtime();

while(runtime.Resume()) {
    runtime.PushParameters(Console.ReadLine() ?? "");
}