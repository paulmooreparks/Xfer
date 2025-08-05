using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace Xfer.Service.Services;

public class XferService {
    // Synchronous methods (preserved for backward compatibility)
    public static string Serialize<T>(T data) {
        return XferConvert.Serialize(data, Formatting.Pretty);
    }

    public static string SerializeCompact<T>(T data) {
        return XferConvert.Serialize(data, Formatting.None);
    }

    public static T? Deserialize<T>(string xfer) {
        return XferConvert.Deserialize<T>(xfer);
    }

    public static object? Deserialize(string xfer, Type type) {
        return XferConvert.Deserialize(xfer, type);
    }

    public static bool TryDeserialize<T>(string xfer, out T? result) {
        try {
            result = XferConvert.Deserialize<T>(xfer);
            return true;
        }
        catch {
            result = default;
            return false;
        }
    }

    public static bool TrySerialize<T>(T data, out string? result) {
        try {
            result = XferConvert.Serialize(data, Formatting.Pretty);
            return true;
        }
        catch {
            result = null;
            return false;
        }
    }

    // New async methods for improved performance and scalability
    public static async Task<string> SerializeAsync<T>(T data, CancellationToken cancellationToken = default) {
        using var writer = new StringWriter();
        await XferConvert.SerializeAsync(data, writer, Formatting.Pretty, cancellationToken);
        return writer.ToString();
    }

    public static async Task<string> SerializeCompactAsync<T>(T data, CancellationToken cancellationToken = default) {
        using var writer = new StringWriter();
        await XferConvert.SerializeAsync(data, writer, Formatting.None, cancellationToken);
        return writer.ToString();
    }

    public static async Task<T?> DeserializeAsync<T>(string xfer, CancellationToken cancellationToken = default) {
        using var reader = new StringReader(xfer);
        return await XferConvert.DeserializeAsync<T>(reader, cancellationToken);
    }

    public static Task<object?> DeserializeAsync(string xfer, Type type, CancellationToken cancellationToken = default) {
        // For non-generic deserialization, we need to use reflection or a different approach
        // Since XferConvert.DeserializeAsync is generic, we'll use the synchronous method for this case
        return Task.FromResult(XferConvert.Deserialize(xfer, type));
    }

    public static async Task<(bool Success, T? Result)> TryDeserializeAsync<T>(string xfer, CancellationToken cancellationToken = default) {
        try {
            using var reader = new StringReader(xfer);
            var result = await XferConvert.DeserializeAsync<T>(reader, cancellationToken);
            return (true, result);
        }
        catch {
            return (false, default);
        }
    }

    public static async Task<(bool Success, string? Result)> TrySerializeAsync<T>(T data, CancellationToken cancellationToken = default) {
        try {
            using var writer = new StringWriter();
            await XferConvert.SerializeAsync(data, writer, Formatting.Pretty, cancellationToken);
            return (true, writer.ToString());
        }
        catch {
            return (false, null);
        }
    }

    // File operations using new async APIs
    public static async Task SerializeToFileAsync<T>(string filePath, T data, Formatting formatting = Formatting.Pretty, CancellationToken cancellationToken = default) {
        await XferConvert.SerializeToFileAsync(data, filePath, formatting, cancellationToken);
    }

    public static async Task<T?> DeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default) {
        return await XferConvert.DeserializeFromFileAsync<T>(filePath, cancellationToken);
    }

    public static async Task<(bool Success, T? Result)> TryDeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default) {
        return await XferConvert.TryDeserializeFromFileAsync<T>(filePath, cancellationToken);
    }

    public static async Task<bool> TrySerializeToFileAsync<T>(string filePath, T data, CancellationToken cancellationToken = default) {
        return await XferConvert.TrySerializeToFileAsync(data, filePath, cancellationToken);
    }

    // Stream operations using new async APIs
    public static async Task SerializeToStreamAsync<T>(Stream stream, T data, Formatting formatting = Formatting.Pretty, CancellationToken cancellationToken = default) {
        await XferConvert.SerializeToStreamAsync(data, stream, formatting, cancellationToken);
    }

    public static async Task<T?> DeserializeFromStreamAsync<T>(Stream stream, CancellationToken cancellationToken = default) {
        return await XferConvert.DeserializeFromStreamAsync<T>(stream, cancellationToken);
    }
}
