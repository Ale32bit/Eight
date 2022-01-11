using Eight;

var runtime = new Runtime();

runtime.LoadInit();

while(runtime.Resume()) {
    Console.Write("> ");
    runtime.PushParameters(Console.ReadLine() ?? "");
}
