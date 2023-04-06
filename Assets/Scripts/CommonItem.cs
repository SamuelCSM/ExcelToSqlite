// 定义 CommonItem 类型
using SQLite4Unity3d;

[CustomSerializer(typeof(CommonItemSerializer))]
public class CommonItem 
{
    public int type;
    public int subId;

    public override string ToString()
    {
        return string.Format("{0}:{1}", type, subId); ;
    }
}

// 定义 CommonItem 的序列化器
public class CommonItemSerializer : IColumnSerializer
{
    public object Deserialize(string value)
    {
        // 将从SQLite中检索的字符串转换为 CommonItem 对象
        string[] parts = value.Split(':');
        int type = int.Parse(parts[0]);
        int subId = int.Parse(parts[1]);
        return new CommonItem { type = type, subId = subId };
    }

    public string Serialize(object value)
    {
        // 将 CommonItem 对象转换为存储在SQLite中的字符串
        CommonItem item = (CommonItem)value;
        return string.Format("{0}:{1}", item.type, item.subId);
    }
}