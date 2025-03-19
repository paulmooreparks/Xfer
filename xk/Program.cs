using System.Text;

using Cliffer;

using Microsoft.Extensions.DependencyInjection;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Http;
using ParksComputing.XferKit.Scripting;
using ParksComputing.XferKit.Diagnostics;
using ParksComputing.XferKit.Cli.Services.Impl;
using ParksComputing.XferKit.Cli;
using System.Diagnostics;
using System.Reflection;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.Xfer.Cli;

internal class Program {
    static Program() {
    }

    static async Task<int> Main(string[] args) {
        DiagnosticListener.AllListeners.Subscribe(new MyObserver());

        var cli = new ClifferBuilder()
            .ConfigureServices(services => {
                services.AddSingleton<PersistenceService>();
                services.AddXferKitWorkspaceServices();
                services.AddXferKitHttpServices();
                services.AddXferKitApiServices();
                services.AddXferKitScriptingServices();
                services.AddXferKitDiagnosticsServices("XferKit");
                services.AddSingleton<ICommandSplitter, CommandSplitter>();
            })
            .Build();

        Cliffer.Macro.CustomMacroArgumentProcessor += CustomMacroArgumentProcessor;

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferEventHandler.OnExit += () => {
            var persistenceService = Utility.GetService<PersistenceService>()!;
        };

        Console.OutputEncoding = Encoding.UTF8;

        return await cli.RunAsync(args);
    }

    private static string[] CustomMacroArgumentProcessor(string[] args) {
        for (int i = 0; i < args.Length; i++) {
            // Find the first instance of the baseurl option flag and its argument. 
            if (args[i] == "-b" || args[i] == "--baseurl") {
                // Index 'i' now points to the first occurrence.
                // Continue the loop with index 'j' starting at 'i + 2'.
                for (int j = i + 2; j < args.Length; ++j) {
                    // If there is a second instance of the baseurl option flag, 
                    // remove the first instance and its argument.
                    if (args[j] == "-b" || args[j] == "--baseurl") {
                        var newArgs = new List<string>();

                        // Copy all arguments to a new collection, except the 
                        // first and second occurrences.
                        for (int k = 0; k < args.Length; k++) {
                            if (k == i || k == i + 1) {
                                continue;
                            }

                            newArgs.Add(args[k]);
                        }

                        // We only expect to find another instance after the 
                        // first one, so early termination is okay.
                        return newArgs.ToArray();
                    }
                }
            }
        }

        return args;
    }
}

public class MyObserver : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>> {
    public void OnNext(DiagnosticListener listener) {
        if (listener.Name == Constants.XferDiagnosticsName) {
            // Explicitly subscribe only to events matching specific criteria:
            // listener.Subscribe(this, eventName => eventName.StartsWith("MyEventPrefix"));
            listener.Subscribe(this);
        }
    }

    public void OnNext(KeyValuePair<string, object?> evt) {
        Console.WriteLine($"{evt.Key}: ");
        Console.WriteLine($"  {evt.Value}");
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }
}
