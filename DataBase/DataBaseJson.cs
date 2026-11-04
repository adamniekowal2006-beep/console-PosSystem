
namespace posConsole.DataBases;
using posConsole.Types;

abstract class DataBaseJson<T>: DataBase<T> where T: IDataBaseItem<T>
{
    private string _path;

    public DataBaseJson(string path){

    }
    
    public async Task<bool> TrySaveAsync() 
    {
        
    }

    public async Task<bool> TryLoadAsync()
    {
        
    }
}