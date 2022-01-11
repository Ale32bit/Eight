using Eight;

var runtime = new Runtime();

runtime.LoadEightLibraries();

runtime.LoadInit();

while(runtime.Resume()) {
    Console.Write("> ");
    runtime.PushParameters(Console.ReadLine() ?? "");
}
