namespace posConsole.DataBases;
using posConsole.Types

abstract class DataBase<T> where T: IDataBaseItem<T>
{
    private List<T> _dataBaseList;
    private HashSet<T> _dataBaseHS;

    public DataBase()
    {
        _dataBaseHS = new HashSet<T>();
        _dataBaseList = new List<T>();
    }

    public abstract Task<bool> TrySave();

    public abstract Task<bool> TryLoad(); 


}