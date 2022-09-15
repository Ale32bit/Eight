using System.Reflection;
using Eight.Libraries;

namespace Eight.LibraryLoader;

public class LibraryLoader
{
    public const string PluginFolder = "Plugins";

    private static Assembly LoadPlugin(string fileName)
    {
        var path = Path.Combine(Environment.CurrentDirectory, fileName);

        var loadContext = new LibraryLoadContext(path);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
    }

    public static async Task<IEnumerable<ILibrary>> LoadPlugins(IServiceProvider serviceProvider)
    {
        if (!Directory.Exists(PluginFolder))
            Directory.CreateDirectory(PluginFolder);
        
        var connections = new List<ILibrary>();
        foreach(var fileName in Directory.GetFiles(PluginFolder).Where(q => q.EndsWith(".dll")))
        {
            var assembly = LoadPlugin(fileName);

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ILibrary).IsAssignableFrom(type))
                {
                    ILibrary result = Activator.CreateInstance(type) as ILibrary;
                    connections.Add(result);
                }
            }

        }

        // Pre init
        foreach(var conn in connections)
        {
            await conn.PreInitAsync();
        }

        // Init
        foreach(var conn in connections)
        {
            await conn.InitAsync();
        }

        return connections;
    }
}