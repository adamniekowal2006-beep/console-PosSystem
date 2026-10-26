namespace posConsole.DataBases;
using posConsole.Types;

abstract class DataBase<T> where T: IDataBaseItem<T>
{
    private List<T>? _dataBaseList;
    private HashSet<T>? _dataBaseHS;

    public DataBase()
    {
        _dataBaseHS = null;
        _dataBaseList = null;
    }

    public abstract Task<bool> TrySave();

    public abstract Task<bool> TryLoad(); 


}