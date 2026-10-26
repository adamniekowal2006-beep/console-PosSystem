namespace posConsole.Types;

sealed class Product: IDataBaseItem<Product>
{
    private string _name;
    private decimal _price;
    private long _ean;

    public Product(string name, decimal price, long ean)
    {
        _name = name;
        _price = price;
        _ean = ean;
        
    }

    public bool Equals(Product? other)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        return _ean.GetHashCode();
    }
}