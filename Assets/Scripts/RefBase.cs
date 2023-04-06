using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;
using UnityEngine;

public class RefBase<TKey>
{
    public virtual TKey Key
    {
        get
        {
            return default(TKey);
        }
    }
}

public class SQLDataBase<TKey, TValue> : RefBase<TKey> where TValue : RefBase<TKey>, new()
{

    [Ignore]
    TableMapping map;

    public override TKey Key
    {
        get
        {
            if (map == null)
            {
                map = SQLDataMgr.Instance?.GetTableMapping(typeof(TValue));
                if (map != null && map.PK != null && map.PK.ColumnType == typeof(TKey))
                {
                    return (TKey)map.PK.GetValue(this);
                }
                else
                {
                    Debug.LogError("sqlite 配置表不支持字符串做主键");
                }
            }
            return base.Key;
        }
    }

    public SQLDataBase()
    {

    }

    [Ignore]
    public static Dictionary<TKey, TValue> cacheMap = new Dictionary<TKey, TValue>();
    [Ignore]
    public static object notExistKey = Orm.ParseValue("-1", typeof(TKey));
    [Ignore]
    public static bool isLoadedAll = false;

    public static TValue GetDataByKey(TKey _key)
    {
        TValue value = null;

        if (cacheMap.TryGetValue(_key, out value))
        {
            return value;
        }

        if (SQLDataMgr.Instance.SQL == null)
        {
            return null;
        }
        value = new TValue();
        SQLDataMgr.Instance.Query(ref value, _key);
        if (value.Key.Equals(notExistKey))
        {
            //Debug.LogError(string.Format("{0} doesn't exist key : {1}", typeof(TValue).FullName, _key));
            return null;
        }
        cacheMap[_key] = value;
        return value;
    }

    public static int Count()
    {
        return SQLDataMgr.Instance.Count<TValue>();
    }

    /// <summary>
    /// 重新获得所有的数据, 
    /// 注意：调用这个方法后会填充到缓存中
    /// </summary>
    /// <returns></returns>
    public static void ReloadAll(bool _needClear = true)
    {
        if (_needClear)
        {
            cacheMap.Clear();
        }
        List<TValue> cache = SQLDataMgr.Instance.GetAll<TValue>();
        if (cache.Count <= 0)
        {
            return;
        }
        for (int i = 0, max = cache.Count; i < max; i++)
        {
            TValue value = cache[i];

            cacheMap[value.Key] = value;
        }
        isLoadedAll = true;
    }

    /// <summary>
    /// 获得所有的数据, 如有缓存直接从缓存取
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<TValue> GetAll()
    {
        if (!isLoadedAll) {
            ReloadAll(isLoadedAll);
        }

        return cacheMap.Values;
    }

    public static IEnumerator<TValue> GetEnumeratorT()
    {
        return SQLDataMgr.Instance.GetEnumerator<TValue>();
    }
}
