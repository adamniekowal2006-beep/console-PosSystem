
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

        string time = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
        string backupName = $"backup_{time}.json";
        string backupHashName = $"backupHash_{time}.hash";
        
        IOStatus CopyBackupStatus =  await IOMethods.TryCopyFile(Path.Combine(_path, "save.json"), Path.Combine(_path, Path.Combine( "backups" , backupName)));

        if (CopyBackupStatus != IOStatus.Success || !File.Exists(Path.Combine(_path, Path.Combine( "backups" , backupName))))
        {
            ClearSaveChanges(true, backupName: backupName);
            if(CopyBackupStatus == IOStatus.Success) return IOStatus.FileNotFound;
            return CopyBackupStatus;
        }

        byte[]? HashSave;
        byte[]? HashBackup;
        IOStatus HashOldStatus;
        IOStatus HashBackupStatus;

        (HashSave, HashOldStatus) = await IOMethods.TryCalculateSHA256(Path.Combine(_path, "save.json"));
        (HashBackup, HashBackupStatus) = await IOMethods.TryCalculateSHA256(Path.Combine(_path, Path.Combine( "backups" , backupName)));

        if(HashOldStatus != IOStatus.Success)
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName);
            return HashOldStatus;
        }

        if(HashBackupStatus != IOStatus.Success)
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName);
            return HashBackupStatus;
        }

        if(HashSave == null || HashBackup == null || !IOMethods.HashCalculate(HashSave, HashBackup))
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName);
            return IOStatus.InvalidHash;
        }

        HashBackupStatus = await IOMethods.TrySaveHash(Path.Combine(_path, Path.Combine( "backups" , backupHashName)), HashBackup);

        if(HashBackupStatus != IOStatus.Success)
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName);
            return HashBackupStatus;
        }
        

        File.Move(Path.Combine(_path, "save.json"),Path.Combine(_path, "saveold.json"));
        File.Move(Path.Combine(_path, "save.hash"),Path.Combine(_path, "saveold.hash"));


        IOStatus CopyNowStatus = await IOMethods.TryCopyFile(Path.Combine(_path,"savetemp.json"), Path.Combine(_path, "save.json"));
        if (CopyNowStatus != IOStatus.Success || !File.Exists( Path.Combine(_path, "save.json")))
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName, NowSave: true);
            RestoreSaves();
            if(CopyNowStatus == IOStatus.Success) return IOStatus.FileNotFound;
            return CopyNowStatus;
        }

        byte[]? HashTemp;
        byte[]? HashNow;
        IOStatus HashTempStatus;
        IOStatus HashNowStatus;

        (HashTemp, HashTempStatus) = await IOMethods.TryCalculateSHA256(Path.Combine(_path,"savetemp.json"));
        (HashNow, HashNowStatus) = await IOMethods.TryCalculateSHA256(Path.Combine(_path, "save.json"));

        if(HashTempStatus != IOStatus.Success)
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName, NowSave: true);
            RestoreSaves();
            return HashTempStatus;
        }

        if(HashNowStatus != IOStatus.Success)
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName, NowSave: true);
            RestoreSaves();
            return HashNowStatus;
        }

        if(HashTemp == null || HashNow == null || !IOMethods.HashCalculate(HashTemp, HashNow))
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName, NowSave: true);
            RestoreSaves();
            return IOStatus.InvalidHash;
        }

        HashNowStatus = await IOMethods.TrySaveHash(Path.Combine(_path, "save.hash"), HashNow);
        if(HashNowStatus != IOStatus.Success)
        {
            ClearSaveChanges(true, backupName: backupName, hsBackupName: backupHashName, NowSave: true, NowHashSave: true);
            RestoreSaves();
            return HashNowStatus;
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
