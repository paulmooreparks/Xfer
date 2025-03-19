using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Diagnostics.Services.Impl;

public class AppDiagnostics<T> : IAppDiagnostics<T> {
    private readonly DiagnosticSource _diagnosticSource;

    public AppDiagnostics(DiagnosticSource diagnosticSource) {
        _diagnosticSource = diagnosticSource;
    }

    public void Emit(string eventName, object? data = null) {
        if (_diagnosticSource.IsEnabled(eventName)) {
            _diagnosticSource.Write($"{typeof(T).FullName}.{eventName}", data);
        }
    }

    public bool IsEnabled(string eventName) => _diagnosticSource.IsEnabled(eventName);
}