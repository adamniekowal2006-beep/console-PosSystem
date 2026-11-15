using System.Text.Encodings.Web;
using System.Text.Json;
using posConsole.Types;

namespace posConsole.StaticMethods;


internal static class JsonMethods
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public static async Task<IOStatus> SerializationAsync<T>(string path, List<T> list)
    {
        try
        {
            await using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, list, JsonOptions);
            return IOStatus.Success;
        }
        catch (DirectoryNotFoundException)  { return IOStatus.DirectoryNotFound; }
        catch (UnauthorizedAccessException) { return IOStatus.AccessDenied; }
        catch (IOException)                 { return IOStatus.IoError; }
        catch (JsonException)               { return IOStatus.InvalidJson; }

    }

    public static async Task<(List<T> list, IOStatus result)> DeserializationAsync<T>(string path)
    {
        try
        {
            await using FileStream stream = File.OpenRead(path);
            List <T>? templist = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions);
            if(templist == null)              return (new List<T>(), IOStatus.NullList);
            return(templist, IOStatus.Success);
        }
        catch (FileNotFoundException)       { return (new List<T>(), IOStatus.FileNotFound); }
        catch (DirectoryNotFoundException)  { return (new List<T>(), IOStatus.DirectoryNotFound); }
        catch (UnauthorizedAccessException) { return (new List<T>(), IOStatus.AccessDenied); }
        catch (IOException)                 { return (new List<T>(), IOStatus.IoError); }
        catch (JsonException)               { return (new List<T>(), IOStatus.InvalidJson); }
        
    }
}

public enum JsonMethodsResult
{
    Success = 0,
    FileNotFound,
    DirectoryNotFound,
    AccessDenied,
    IoError,
    InvalidJson,
    NullList

}