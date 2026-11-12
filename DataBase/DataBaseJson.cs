
namespace posConsole.DataBases;

using posConsole.StaticMethods;
using posConsole.Types;
using System.IO;
using System.Security.Cryptography;

abstract class DataBaseJson<T>: DataBase<T> where T: IDataBaseItem<T>
{
    private string _path;

    public DataBaseJson(string path): base(new List<T>()){
        _path = path;
    }
    
    public async Task<LoadStatus> TrySaveAsync()
    {
        List<T> DatabaseList = new List<T>(GetList());
        JsonMethodsResult JsonDataBaseCode = await JsonMethods.SerializationAsync(Path.Combine(_path,"savetemp.json"), DatabaseList);
        if(JsonDataBaseCode != JsonMethodsResult.Success)
        {
            File.Delete(Path.Combine(_path,"savetemp.json"));
            return JsonCodetoLoadStatus(JsonDataBaseCode);
        }

        List<T> TempList;
        JsonMethodsResult JsonTempCode;
        (TempList, JsonTempCode) = await JsonMethods.DeserializationAsync<T>(Path.Combine(_path,"savetemp.json"));

        if(JsonTempCode != JsonMethodsResult.Success)
        {
            File.Delete(Path.Combine(_path,"savetemp.json"));
            return JsonCodetoLoadStatus(JsonDataBaseCode);
        }

        foreach(var item in TempList)
        {
            if(!DatabaseList.Any(u => u.Equals(item)))
            {
                File.Delete(Path.Combine(_path,"savetemp.json"));
                return LoadStatus.InvalidSave;

            }
        }

        string backupName = $"backup_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";
        File.Copy(Path.Combine(_path, "save.json"), Path.Combine(_path, Path.Combine( "backups" , backupName)));
        if(!File.Exists(Path.Combine(_path, Path.Combine( "backups" , backupName))))
        {
            File.Delete(Path.Combine(_path,"savetemp.json"));
            File.Delete(Path.Combine( "backups" , backupName));
            return LoadStatus.InvalidSave;
        }

        try{
            await using FileStream streamSave = File.OpenRead(Path.Combine(_path, "save.json"));
            await using FileStream streamBackup = File.OpenRead(Path.Combine( "backups" , backupName));
            Byte[] HashSave =  await SHA256.HashDataAsync(streamSave);
            Byte[] HashBackup = await SHA256.HashDataAsync(streamBackup);
            
        }
        catch (IOException) {return LoadStatus.IoError;}



        return LoadStatus.success;
    }



    public async Task<bool> TryLoadAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TryLoadBackupAsync(string name)
    {
        throw new NotImplementedException();
    }

    public List<string> ListBackups()
    {
        var tempList = new List<string>();
        foreach (string filePath in Directory.GetFiles(Path.Combine(_path, "backups")))
        {
            tempList.Add(Path.GetFileName(filePath));
        }
        return tempList;
    }

    static public LoadStatus JsonCodetoLoadStatus(JsonMethodsResult code)
    {
        if(code == JsonMethodsResult.IoError) return LoadStatus.IoError;
        else if(code == JsonMethodsResult.AccessDenied) return LoadStatus.AccessDenied;
        else if(code == JsonMethodsResult.InvalidJson) return LoadStatus.InvalidJson;
        else return LoadStatus.InvalidError;
    }
}


public enum LoadStatus
{
    success = 0,
    InvalidSave,
    InvalidError,
    IoError,
    AccessDenied,
    InvalidJson

}

// TODO: wyjatki do obsluzenia w TrySaveAsync (liczenie hasha save.json i backupu)
//
// File.OpenRead:
//  - FileNotFoundException      brak save.json lub backupu
//  - DirectoryNotFoundException brak katalogu (np. backups)
//  - UnauthorizedAccessException brak uprawnien / sciezka wskazuje na katalog
//  - IOException                ogolny blad I/O (np. plik zablokowany); po nim dziedzicza
//                               FileNotFound, DirectoryNotFound, PathTooLong
//  - ArgumentException / NotSupportedException zla sciezka (raczej blad programisty)
//
// SHA256.HashDataAsync (statyczna metoda SHA256, przyjmuje Stream, zwraca byte[]):
//  - IOException                blad odczytu w trakcie liczenia
//  - OperationCanceledException tylko przy CancellationToken
//
// Uwagi:
//  - kolejnosc catch: FileNotFound i DirectoryNotFound PRZED IOException
//  - File.Copy tez rzuca (FileNotFound, DirectoryNotFound, IOException, UnauthorizedAccess)
//    i jest poza try
//  - nie lapac ogolnego Exception
//  - LoadStatus nie ma FileNotFound / DirectoryNotFound -> rozszerzyc enum
//  - porownanie hashy: byte[] nie porownuje sie przez ==, tylko zawartoscia