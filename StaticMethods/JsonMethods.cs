using System.Text.Encodings.Web;
using System.Text.Json;

namespace posConsole.StaticMethods;


internal static class JsonMethods
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public static async Task<JsonMethodsResult> SerializationAsync<T>(string path, List<T> list)
    {
        try
        {
            await using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, list, JsonOptions);
            return JsonMethodsResult.Success;
        }
        catch (DirectoryNotFoundException)  { return JsonMethodsResult.DirectoryNotFound; }
        catch (UnauthorizedAccessException) { return JsonMethodsResult.AccessDenied; }
        catch (IOException)                 { return JsonMethodsResult.IoError; }
        catch (JsonException)               { return JsonMethodsResult.InvalidJson; }

    }

    public static async Task<(List<T> list, JsonMethodsResult result)> DeserializationAsync<T>(string path)
    {
        try
        {
            await using FileStream stream = File.OpenRead(path);
            List <T>? templist = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions);
            if(templist == null)              return (new List<T>(), JsonMethodsResult.NullList);
            return(templist, JsonMethodsResult.Success);
        }
        catch (FileNotFoundException)       { return (new List<T>(), JsonMethodsResult.FileNotFound); }
        catch (DirectoryNotFoundException)  { return (new List<T>(), JsonMethodsResult.DirectoryNotFound); }
        catch (UnauthorizedAccessException) { return (new List<T>(), JsonMethodsResult.AccessDenied); }
        catch (IOException)                 { return (new List<T>(), JsonMethodsResult.IoError); }
        catch (JsonException)               { return (new List<T>(), JsonMethodsResult.InvalidJson); }
        
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