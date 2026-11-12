
namespace posConsole.DataBases;
using posConsole.Types;

abstract class DataBaseJson<T>: DataBase<T> where T: IDataBaseItem<T>
{
    private string _path;

    public DataBaseJson(string path): base(new List<T>()){
        _path = path;
    }
    
    public async Task<bool> TrySaveAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TryLoadAsync()
    {
        throw new NotImplementedException();
    }
}