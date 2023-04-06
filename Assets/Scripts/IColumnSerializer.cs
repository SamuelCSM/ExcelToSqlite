

    public interface IColumnSerializer
    {
        object Deserialize(string value);
        string Serialize(object value);
    }
