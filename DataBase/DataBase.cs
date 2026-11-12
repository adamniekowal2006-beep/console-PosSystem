namespace posConsole.DataBases;
using posConsole.Types;

abstract class DataBase<T> where T: IDataBaseItem<T>
{
    private List<T>? _dataBaseList;
    private HashSet<T>? _dataBaseHS;

    public DataBase(List<T> dbList)
    {
        _dataBaseList = dbList;
        _dataBaseHS = new HashSet<T>(dbList);
    }




}