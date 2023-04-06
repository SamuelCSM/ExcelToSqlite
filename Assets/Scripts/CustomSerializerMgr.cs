
using System;
using System.Collections.Generic;
using SQLite4Unity3d;

// 自定义序列化管理器(TODO 这个管理器可以直接放在SQLiteConnection中，这样更合理)
public class CustomSerializerMgr : Singleton<CustomSerializerMgr>
{
    private Dictionary<Type, IColumnSerializer> _customSerializerDic = null;

    // 初始化
    public void initCustomSerializer()
    {
        registerSerializer(typeof(CommonItem), new CommonItemSerializer());
    }

    //  注册自定义序列化信息
    public void registerSerializer(Type _type, IColumnSerializer _serializer)
    {
        if (_type == null || _serializer == null)
            return;

        if (_customSerializerDic == null)
            _customSerializerDic = new Dictionary<Type, IColumnSerializer>();

        _customSerializerDic[_type] = _serializer;
    }

    // 获取序列化器
    public IColumnSerializer getColumnSerializer(Type _type)
    {
        if (_customSerializerDic == null)
            return null;

        IColumnSerializer serializer = null;
        _customSerializerDic.TryGetValue(_type, out serializer);
        return serializer;
    }
}
