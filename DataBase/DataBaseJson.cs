
namespace posConsole.DataBases;
using posConsole.Types;

abstract class DataBaseJson<T>: DataBase<T> where T: IDataBaseItem<T>
{
    private string _path;

    public DataBaseJson(string path){

    }
    
    public override async Task<bool> TrySaveAsync()
    {
        
    }

    public override async Task<bool> TryLoadAsync()
    {
        
    }
}