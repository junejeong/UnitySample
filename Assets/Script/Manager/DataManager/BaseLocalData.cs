public enum DataType
{
    None,
    MAX,
}

public class BaseLocalData
{
    string _savePath;
    DataType _dataType;

    public string SavePath
    {
        get { return _savePath; }
    }

    public BaseLocalData(string path, DataType dataType)
    {
        _savePath = $"/{path}";
        _dataType = dataType;
    }

    public void Save()
    {
        OffLineDataUtility.SaveData(_savePath, this);
    }

    public virtual void Claer()
    {

    }
}