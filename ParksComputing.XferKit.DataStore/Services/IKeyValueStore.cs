
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.Sqlite;

namespace ParksComputing.XferKit.DataStore.Services;

public interface IKeyValueStore : IDictionary<string, object?> {
}
