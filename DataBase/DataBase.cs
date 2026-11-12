namespace posConsole.DataBases;

using System.Collections.ObjectModel;
using posConsole.Types;

abstract class DataBase<T> where T: IDataBaseItem<T>
{
    private List<T> _dataBaseList;
    private HashSet<T> _dataBaseHS;

    public DataBase(List<T> dbList)
    {
        _dataBaseList = new List<T>(dbList);
        _dataBaseHS = new HashSet<T>(dbList);
    }

    protected ReadOnlyCollection<T> GetList()
    {
        return _dataBaseList.AsReadOnly();
    }

    protected void SetList(List<T> list)
    {
        _dataBaseList = new List<T>(list);
        _dataBaseHS = new HashSet<T>(list);
    }

    public void Add(T item)
    {
        if(!this.IsAny(item))
        {
            _dataBaseHS.Add(item);
            _dataBaseList.Add(item);
        }
    }

    public void Delete(T item)
    {
        _dataBaseHS.Remove(item);
        _dataBaseList.Remove(item);
    }

    public bool IsAny(T item)
    {
        return _dataBaseHS.Contains(item);
    }

    public List<T>  Where(Func<T,bool> comparator)
    {
        var ValueList = new List<T>();
        foreach(var item in _dataBaseList)
        {
            if (comparator(item)) ValueList.Add(item);
        }
        return ValueList;
    }




}