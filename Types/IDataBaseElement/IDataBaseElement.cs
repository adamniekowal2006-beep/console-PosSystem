namespace posConsole.Types;



interface IDataBaseItem<T>: IEquatable<T> where T: IDataBaseItem<T>
{
    
}