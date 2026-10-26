using System.Text.Encodings.Web;
using System.Text.Json;

namespace posConsole.StaticMetods;


static class JsonMetods
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    static async Task<JsonMetodsResult> SerializationAsync<T>(string path, List<T> list)
    {
        try
        {
            await using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, list, JsonOptions);
            return JsonMetodsResult.Success;
        }
        catch (DirectoryNotFoundException)  { return JsonMetodsResult.DirectoryNotFound; }
        catch (UnauthorizedAccessException) { return JsonMetodsResult.AccessDenied; }
        catch (IOException)                 { return JsonMetodsResult.IoError; }
        catch (JsonException)               { return JsonMetodsResult.InvalidJson; }

    }

    static async Task<(List<T> list, JsonMetodsResult result)> DeserializationAsync<T>(string path)
    {
        try
        {
            await using FileStream stream = File.OpenRead(path);
            List <T>? templist = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions);
            return(templist!, JsonMetodsResult.Success);
        }
        catch (FileNotFoundException)       { return (new List<T>(), JsonMetodsResult.FileNotFound); }
        catch (DirectoryNotFoundException)  { return (new List<T>(), JsonMetodsResult.DirectoryNotFound); }
        catch (UnauthorizedAccessException) { return (new List<T>(), JsonMetodsResult.AccessDenied); }
        catch (IOException)                 { return (new List<T>(), JsonMetodsResult.IoError); }
        catch (JsonException)               { return (new List<T>(), JsonMetodsResult.InvalidJson); }
        
    }
}

public enum JsonMetodsResult
{
    Success = 0,
    FileNotFound,
    DirectoryNotFound,
    AccessDenied,
    IoError,
    InvalidJson

}