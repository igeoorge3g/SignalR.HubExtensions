namespace SignalR.HubExtensions
{
    using System.Reflection;
    using Microsoft.AspNetCore.SignalR;

    public static class HubExtensions
    {
        public static ISignalRServerBuilder CreateJs(this ISignalRServerBuilder builder, string js_filename)
        {
            Console.WriteLine($"Generating {js_filename}");

            var path = Path.Combine("wwwroot/js/", js_filename);

            var file = File.CreateText(path);

            var assembly = Assembly.GetExecutingAssembly();

            var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Hub))).ToArray();

#if DEBUG
            var log_level = "None"; // Information
#else
        var log_level = "None";
#endif

            file.WriteLine($"hubs.js - Auto-Generated");
            file.WriteLine($"var hubs = {{}};");

            foreach (var type in types)
            {
                file.WriteLine(@$"
hubs.{type.Name} = {{}};
hubs.{type.Name}.connection = new signalR.HubConnectionBuilder()
    .withUrl(""/{type.Name}"")
    .configureLogging(signalR.LogLevel.{log_level})
    .build();

hubs.{type.Name}.start = async function () {{
    try {{
        await hubs.{type.Name}.connection.start();
        console.log(""%c{type.Name} Connected"", ""background:green;color:white"");
    }} catch (err) {{
        console.log(""%c{type.Name} Connection Error"", ""background:red;color:white"");
        setTimeout(hubs.{type.Name}.start, 5000);
    }}
}};

hubs.{type.Name}.connection.onclose(async () => {{
    console.log(""%c{type.Name} Connection Closed"", ""background:red;color:white"");
    await hubs.{type.Name}.start();
}});

hubs.{type.Name}.start();
            ");

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    var parameters = string.Join(", ", method.GetParameters().Select(e => e.Name));
                    var parameters_name = string.Join(", ", method.GetParameters().Select(e => e.Name + ": " + e.ParameterType.Name));

                    file.WriteLine(@$"
hubs.{type.Name}.{method.Name} = async function ({parameters}) {{
    try {{
        return await hubs.{type.Name}.connection.invoke(""{method.Name}"", {parameters});
    }} catch (err) {{
        console.log(""%c{type.Name}.{method.Name}({parameters_name}) Server Error"", ""background:red;color:white"");
    }}
}};
                ");
                }

                // Interface Hub<Interface>
                var interface_type = type.BaseType?.GetGenericArguments()[0];
                var interface_methods = interface_type?.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (interface_methods != null)
                {
                    foreach (var method in interface_methods)
                    {
                        var parameters = string.Join(", ", method.GetParameters().Select(e => e.Name));

                        file.WriteLine(@$"
hubs.{type.Name}.on_{method.Name} = async ({parameters}) => {{console.log(""%con_{method.Name} not implemented."",""background:orange;color:black"")}};

hubs.{type.Name}.connection.on(""{method.Name}"", async ({parameters}) => {{
    await hubs.{type.Name}.on_{method.Name}({parameters});
}});
                ");
                    }
                }
            }

            file.Close();

            return builder;
        }
    }
}