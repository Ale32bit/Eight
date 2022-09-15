using Eight.Extensions;
using Eight.Libraries;
using KeraLua;
using System.Text;

namespace Eight;

public class Runtime : IDisposable
{
    public Lua LuaState;
    public Lua Thread;

    private int parametersCount = 0;

    public Runtime()
    {
        LuaState = new Lua(false)
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

    /// <summary>
    /// Load all classes that implement the interface ILibrary and calls the method ILibrary.Register( LuaState )
    /// </summary>
    public void LoadEightLibraries()
    {
        var iType = typeof(ILibrary);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => iType.IsAssignableFrom(p) && !p.IsInterface);

        foreach (var type in types)
        {
            var instance = (ILibrary)Activator.CreateInstance(type)!;

            instance.Register(LuaState);
        }
    }

    public void LoadInit()
    {
        var status = Thread.LoadFile("Assets/Lua/init.lua");
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

        throw new LuaException($"Top thread exception:\n{error}\n{stacktrace}");
    }

    /// <summary>
    /// Push an object to the stack
    /// </summary>
    /// <param name="par"></param>
    /// <exception cref="Exception"></exception>
    public void Push(object? par)
    {
        var type = par.GetType();
        switch (par)
        {
            case string s:
                Thread.PushString(s);
                break;

            case byte:
            case sbyte:
            case short:
            case ushort:
            case int:
            case uint:
            case double:
                Thread.PushNumber(Convert.ToDouble(par));
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

            case LuaBuffer b:
                Thread.PushBuffer(b.Buffer);
                break;

            case LuaFunction func:
                Thread.PushCFunction(func);
                break;

            case IntPtr ptr:
                Thread.PushLightUserData(ptr);
                break;

            default:
                if (type.IsArray)
                {
                    var mi = typeof(Runtime).GetMethod("PushArray");
                    var method = mi.MakeGenericMethod(type.GetElementType()!);
                    method.Invoke(this, new object[] { par, false });
                }
                else
                {
                    throw new Exception("Invalid type provided");
                }
                break;
        }
    }

    /// <summary>
    /// Push an array of parameters into the stack and increases the parameters count
    /// </summary>
    /// <param name="pars"></param>
    public void PushParameters(object?[] pars)
    {
        foreach (var par in pars)
        {
            Push(par);
            parametersCount++;
        }
    }

    /// <summary>
    /// Push a C closure to the stack and increases the parameters count
    /// </summary>
    /// <param name="function"></param>
    /// <param name="n"></param>
    public void PushCClosure(LuaFunction function, int n)
    {
        Thread.PushCClosure(function, n);
        parametersCount++;
    }

    /// <summary>
    /// Push an instantiated object to the stack and increases the parameters count
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public void PushObject<T>(T obj)
    {
        Thread.PushObject<T>(obj);
        parametersCount++;
    }

    /// <summary>
    /// Push a table array of T elements to the stack and increases the parameters count
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    public void PushArray<T>(T[] arr, bool increase = true)
    {
        Thread.NewTable();

        for (int i = 0; i < arr.Length; i++)
        {
            Push(arr[i]);
            Thread.RawSetInteger(-2, i + 1);
        }

        Thread.SetTop(-1);

        if (increase)
            parametersCount++;
    }

    public void Dispose()
    {
        Thread.Dispose();
        LuaState.Dispose();
    }
}

/// <summary>
/// Represents a byte[] to pass to Lua.PushBuffer
/// </summary>
public class LuaBuffer
{
    public readonly byte[] Buffer;
    public LuaBuffer(byte[] buffer)
    {
        Buffer = buffer;
    }

}