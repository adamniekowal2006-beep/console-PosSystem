
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
    
    public async Task<IOStatus> TrySaveAsync()
    {
        List<T> DatabaseList = new List<T>(GetList());
        IOStatus JsonDataBaseCode = await JsonMethods.SerializationAsync(Path.Combine(_path,"savetemp.json"), DatabaseList);
        if(JsonDataBaseCode != IOStatus.Success)
        {
            ClearSaveChanges(temporarySave: true);
            return JsonDataBaseCode;
        }

        List<T> TempList;
        IOStatus JsonTempCode;
        (TempList, JsonTempCode) = await JsonMethods.DeserializationAsync<T>(Path.Combine(_path,"savetemp.json"));

        
        if(JsonTempCode != IOStatus.Success)
        {
            ClearSaveChanges(temporarySave: true);
            return JsonTempCode;
        }

        if (!(TempList.Count() == DatabaseList.Count()))
        {
            ClearSaveChanges(temporarySave: true);
            return IOStatus.InvalidSave;
        }
        
        foreach(var item in TempList)
        {
            if(!DatabaseList.Any(u => u.Equals(item)))
            {
                ClearSaveChanges(temporarySave: true);
                return IOStatus.InvalidSave;

            }
        }

        string time = $"{DateTime.Now:yyyy-MM-dd-HH-mm}";
        string backupName = $"backup_{time}.json";
        string backupHashName = $"backupHash_{time}.hash";

        IOStatus CopyBackupStatus = await CopySave(Path.Combine(_path, "save.json"), Path.Combine(_path, Path.Combine( "backups" , backupName)), Path.Combine(_path, Path.Combine( "backups" , backupHashName)));
        if (CopyBackupStatus != IOStatus.Success)
        {
            //Wywolanie czyszczenia 
            return CopyBackupStatus;
        }

        File.Move(Path.Combine(_path, "save.json"),Path.Combine(_path, "saveold.json"));
        File.Move(Path.Combine(_path, "save.hash"),Path.Combine(_path, "saveold.hash"));

        IOStatus CopyNowSaveStatus = await CopySave(Path.Combine(_path,"savetemp.json"),Path.Combine(_path, "save.json"),Path.Combine(_path, "save.hash"));
        if (CopyNowSaveStatus != IOStatus.Success)
        {
            //Wywolanie czysczczenia
            //Odzyskanien Save-u
            return CopyNowSaveStatus;
        }

        File.Delete(Path.Combine(_path, "saveold.json"));
        File.Delete(Path.Combine(_path, "saveold.hash"));

        return IOStatus.Success;
    }

    public void ClearSaveChanges(bool temporarySave, string? backupName = null, string? hsBackupName = null, bool NowSave = false, bool NowHashSave = false)
    {
        if(temporarySave) File.Delete(Path.Combine(_path,"savetemp.json"));
        if(backupName!=null) File.Delete(Path.Combine(_path,Path.Combine( "backups" , backupName)));
        if(hsBackupName!=null) File.Delete(Path.Combine(_path,Path.Combine( "backups" , hsBackupName)));
        if(NowSave) File.Delete(Path.Combine(_path, "save.json"));
        if(NowHashSave) File.Delete(Path.Combine(_path, "save.hash"));
    }
    
    public void RestoreSaves()
    {
        File.Move(Path.Combine(_path, "saveold.json"),Path.Combine(_path, "save.json"));
        File.Move(Path.Combine(_path, "saveold.hash"),Path.Combine(_path, "save.hash"));

    }

    public async Task<IOStatus> CopySave(string pathFrom, string pathTo, string? pathHash = null)
    {
        IOStatus CopyStatus = await IOMethods.TryCopyFile(pathFrom, pathTo);
        if (CopyStatus != IOStatus.Success || !File.Exists(pathTo))
        {
            if(CopyStatus == IOStatus.Success) return IOStatus.FileNotFound;
            return CopyStatus;
        }

        byte[]? HashFrom;
        byte[]? HashTo;
        IOStatus HashFromStatus;
        IOStatus HashToStatus;

        (HashFrom, HashFromStatus) = await IOMethods.TryCalculateSHA256(pathTo);
        (HashTo, HashToStatus) = await IOMethods.TryCalculateSHA256(pathFrom);

        if(HashFromStatus!=IOStatus.Success) return HashFromStatus;
        if(HashToStatus!=IOStatus.Success) return HashToStatus;
        if(HashFrom == null || HashTo == null || !IOMethods.HashCalculate(HashFrom, HashTo))
        {
            return IOStatus.InvalidSave;
        }

        if (pathHash != null)
        {
            IOStatus HashSaveStatus = await IOMethods.TrySaveHash(pathHash, HashTo);
            if( HashSaveStatus != IOStatus.Success)
            {
                return HashSaveStatus;
            }
        }

        return IOStatus.Success;
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

}




// ===== USTALENIA (TrySaveAsync) =====
//
// Decyzje:
//  - TrySaveAsync NIE obsluguje pierwszego zapisu. Inicjalizacja bazy musi utworzyc
//    OBA pliki: save.json i save.hash (inaczej File.Move na save.hash rzuci wyjatek).
//    Brak pliku -> metoda zwraca FileNotFound (nie odroznia "bazy jeszcze nie ma" od "plik zniknal").
//  - File.Move (save.* -> saveold.* oraz RestoreSaves) jest TYMCZASOWY.
//    Do zmiany na kopiowanie (2026-11-16) + wspolna metoda do kopiowania.
//
// Do przemyslenia przy zmianie Move -> Copy:
//  - kopiowanie temp -> save.json: cel juz istnieje; File.Copy bez nadpisywania rzuci wyjatek,
//    a kopiowanie z nadpisaniem nie jest atomowe (moze zostac polowiczny save.json)
//  - pozostale saveold.* po przerwanym zapisie blokuja kolejne zapisy
//  - czy saveold jest potrzebny, skoro w backups/ jest juz kopia save.json sprawdzona hashem?
//  - TryCopyFile juz istnieje: sprawdzic, czym roznia sie 3 przypadki (nadpisywanie? hash?)
//  - nowa metoda i sprzatanie musza radzic sobie z brakiem katalogu
//
// Otwarte bledy:
//  - ClearSaveChanges: File.Delete rzuca DirectoryNotFoundException, gdy brak katalogu
//    (np. brak backups/ -> TryCopyFile zwraca DirectoryNotFound, a potem sprzatanie rzuca wyjatek)
//  - File.Delete saveold.* na koncu bez obslugi bledow (udany zapis moze wygladac na nieudany)
//  - savetemp.json zostaje po udanym zapisie
//  - nazwa backupu z dokladnoscia do sekundy -> kolizja przy 2 zapisach w tej samej sekundzie
//  - porownanie list (Count + Any(Equals)) nie wykrywa duplikatow
//
// Projekt:
//  - te same wywolania ClearSaveChanges/RestoreSaves skopiowane w kazdej galezi
//  - async bez await w TryCopyFile i TrySaveHash (CS1998, synchroniczny Write)
//  - ClearSaveChanges i RestoreSaves sa public (wewnetrzne kroki zapisu)
//  - (byte[]?, IOStatus) wymusza nadmiarowe sprawdzanie null
//
// Styl:
//  - zmienne lokalne i parametry w PascalCase (konwencja: camelCase)
//  - zagniezdzone Path.Combine + powtarzane te same sciezki
//  - HashCalculate porownuje, a nie liczy -> mylaca nazwa
//  - stary komentarz "Tuataj wstawic try/catch" w IOMethods.cs
