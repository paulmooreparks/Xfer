using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace ParksComputing.Xfer.Cli.Services;
internal class Utility {
    private static IServiceProvider? _serviceProvider;

    internal static void SetServiceProvider(IServiceProvider provider) {
        _serviceProvider = provider;
    }

    internal static IServiceProvider GetServiceProvider() {
        if (_serviceProvider is null) {
            throw new InvalidOperationException("Service provider is not set.");
        }

        return _serviceProvider;
    }

    internal static T? GetService<T>() {
        if (_serviceProvider is null) {
            throw new InvalidOperationException("Service provider is not set.");
        }

        return _serviceProvider.GetService<T>();
    }

    internal static int ShowMenu(string[] options, int defaultOptionIndex = 0, string markerText = "(*)") {
        // Display the menu with the marker for the default option
        for (int i = 0; i < options.Length; i++) {
            Console.WriteLine($"[{i + 1}] {options[i]}{(i + 1 == defaultOptionIndex ? " " + markerText : "")}");
        }

        Console.Write($"Please enter a selection (default is {defaultOptionIndex}, 0 to cancel): ");

        while (true) {
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput)) {
                if (defaultOptionIndex >= 0) {
                    return defaultOptionIndex; // Return default selection
                }

                return 0; // Indicate no selection
            }


            if (int.TryParse(userInput, out int selection) && selection >= 0 && selection <= options.Length) {
                return selection; // Return user selection
            }

            Console.WriteLine($"Invalid selection. Please enter a number between 1 and {options.Length}.");

            while (Console.KeyAvailable) {
                Console.ReadKey(true); // Clear input buffer
            }
        }
    }
}
