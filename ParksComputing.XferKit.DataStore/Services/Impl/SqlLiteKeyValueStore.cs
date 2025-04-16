// SqliteKeyValueStore.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.Sqlite;

using Newtonsoft.Json;

using ParksComputing.XferKit.DataStore.Services;

namespace ParksComputing.XferKit.DataStore.Services.Impl;

public class SqliteKeyValueStore : IKeyValueStore {
    private readonly string _connectionString;

    public SqliteKeyValueStore(string databasePath) {
        _connectionString = $"Data Source={databasePath}";
        InitializeDatabase();
    }

    private void InitializeDatabase() {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS kv (
                key TEXT PRIMARY KEY,
                value TEXT
            );";
        command.ExecuteNonQuery();
    }

    public object? this[string key] {
        get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
        set => Set(key, value);
    }

    public ICollection<string> Keys => GetKeys();
    public ICollection<object?> Values => GetValues();
    public int Count => GetCount();
    public bool IsReadOnly => false;

    public void Add(string key, object? value) {
        if (ContainsKey(key))
            throw new ArgumentException("Key already exists.");
        Set(key, value);
    }

    public bool ContainsKey(string key) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM kv WHERE key = $key";
        command.Parameters.AddWithValue("$key", key);
        using var reader = command.ExecuteReader();
        return reader.Read();
    }

    public bool Remove(string key) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM kv WHERE key = $key";
        command.Parameters.AddWithValue("$key", key);
        return command.ExecuteNonQuery() > 0;
    }

    public bool TryGetValue(string key, out object? value) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM kv WHERE key = $key";
        command.Parameters.AddWithValue("$key", key);

        using var reader = command.ExecuteReader();
        if (reader.Read()) {
            var json = reader.GetString(0);
            value = JsonConvert.DeserializeObject<object?>(json);
            return true;
        }

        value = null;
        return false;
    }

    public void Set(string key, object? value) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO kv (key, value) VALUES ($key, $value) ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", JsonConvert.SerializeObject(value));
        command.ExecuteNonQuery();
    }

    public void Clear() {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM kv";
        command.ExecuteNonQuery();
    }

    public void Add(KeyValuePair<string, object?> item) => Add(item.Key, item.Value);
    public bool Contains(KeyValuePair<string, object?> item) => TryGetValue(item.Key, out var value) && Equals(value, item.Value);
    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) {
        foreach (var kv in this) {
            array[arrayIndex++] = kv;
        }
    }
    public bool Remove(KeyValuePair<string, object?> item) => Contains(item) && Remove(item.Key);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT key, value FROM kv";
        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            var key = reader.GetString(0);
            var json = reader.GetString(1);
            yield return new KeyValuePair<string, object?>(key, JsonConvert.DeserializeObject<object?>(json));
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private ICollection<string> GetKeys() => this.Select(kv => kv.Key).ToList();
    private ICollection<object?> GetValues() => this.Select(kv => kv.Value).ToList();
    private int GetCount() {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM kv";
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
