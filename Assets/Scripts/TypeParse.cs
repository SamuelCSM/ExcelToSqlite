using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SQLite4Unity3d;
using UnityEngine;


public static class TypeParse 
{
    public static Type C_T_List = typeof(List<>);
    public static Type C_T_Int = typeof(int);
    public static Type C_T_Long = typeof(long);
    public static Type C_T_String = typeof(string);
    public static Type C_T_Short = typeof(short);
    public static Type C_T_Byte = typeof(byte);
    public static Type C_T_Bool = typeof(bool);
    public static Type C_T_Float = typeof(float);

    public static Type C_T_Rect = typeof(Rect);
    public static Type C_T_Color = typeof(Color);
    public static Type C_T_Vector2 = typeof(Vector2);
    public static Type C_T_Vector3 = typeof(Vector3);
    public static Type C_T_Vector2Int = typeof(Vector2Int);
    public static Type C_T_Vector3Int = typeof(Vector3Int);

    /// <summary>
    /// 解析字段值，返回是否读取成功，如失败则需要通过自定义方式读取
    /// </summary>
    /// <param name="_value"></param>
    /// <param name="_type"></param>
    /// <param name="_obj"></param>
    /// <returns></returns>
    public static bool ParseValue(string _value, Type _type, ref object _obj,  string _errMsg = "")
    {
        try
        {
            if (_value.Equals(string.Empty))
            {
                if (_type == C_T_String)
                {
                    _obj = "";
                    return true;
                }
                if (_type == C_T_Float)
                {
                    _obj = 0f;
                    return true;
                }
                if (_type == C_T_Int)
                {
                    _obj = 0;
                    return true;
                }
                if (_type == C_T_Bool)
                {
                    _obj = false;
                    return true;
                }
                if (_type == C_T_Long)
                {
                    _obj = 0;
                    return true;
                }
                return false;
            }
            else
            {
                _value = _value.Trim();

                // 枚举 暂不支持
                if (_type.IsEnum)
                {
                    _obj = Enum.Parse(_type, _value, true);
                    return true;
                }
                else if (_type == C_T_String)
                {
                    _obj = _value;
                    return true;
                }
                else if (_type == C_T_Float)
                {
                    if (_value == "0" || _value == "" || _value == string.Empty)
                        _obj = 0f;
                    else
                        _obj = float.Parse(_value);

                    return true;
                }
                else if (_type == C_T_Int)
                {
                    if (_value == "")
                        _obj = 0;
                    else
                        _obj = int.Parse(_value);

                    return true;
                }
                else if (_type == C_T_Bool)
                {
                    _obj = bool.Parse(_value);

                    return true;
                }
                else if (_type == C_T_Long)
                {
                    _obj = long.Parse(_value);

                    return true;
                }
                //以下是扩展
                else if (_type == C_T_Rect)
                {
                    _obj = GetRect(_value, _errMsg);

                    return true;
                }
                else if (_type == C_T_Vector2)
                {
                    _obj = GetVector2(_value, _errMsg);

                    return true;
                }
                else if (_type == C_T_Vector3)
                {
                    _obj = GetVector3(_value, _errMsg);

                    return true;
                }
                else if (_type == C_T_Vector2Int)
                {
                    _obj = GetVector2Int(_value, _errMsg);

                    return true;
                }
                else if (_type == C_T_Vector3Int)
                {
                    _obj = GetVector3Int(_value, _errMsg);

                    return true;
                }
                else if (_type == C_T_Color)
                {
                    _obj = GetColor(_value);

                    return true;
                }
                else if (IsCustomSerializer(_type))
                {
                    IColumnSerializer serializer =  CustomSerializerMgr.Instance?.getColumnSerializer(_type);
                    if (serializer != null)
                    {
                        _obj = serializer.Deserialize(_value);
                        return true;
                    }
                    return false;
                }
                // TODO 还有数组跟枚举需处理
                else if (HasImplementedRawGeneric(_type, C_T_List))
                {
                    string[] strings = GetListString(_value);
                    if (strings != null)
                    {
                        // 这边用反射处理list,待优化
                        Type singleType = _type.GetGenericArguments()[0];
                        Type listType = typeof(List<>).MakeGenericType(singleType);
                        var list = Activator.CreateInstance(listType);
                        MethodInfo addMethod = listType.GetMethod("Add");

                        for (int i = 0, count = strings.Length; i < count; i++)
                        {
                            object value = null;
                            if ( ParseValue(strings[i], singleType, ref value))
                                addMethod?.Invoke(list, new object[] { value });
                        }

                        _obj = list;

                        return true;
                    }

                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("ParseValue type:{0}, value:{1}, failed: {2}", _type.ToString(), _value,
                ex.ToString()));
            return false;
        }
    }

    public static bool IsCustomSerializer(Type _fieldType)
    {
        CustomSerializerAttribute customAttribute =
            (CustomSerializerAttribute)Attribute.GetCustomAttribute(_fieldType,
                typeof(CustomSerializerAttribute));

        return customAttribute != null;
    }

    /// <summary>
    /// 判断指定的类型 <paramref name="_judgeType"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
    /// </summary>
    /// <param name="_judgeType">需要测试的类型。</param>
    /// <param name="_basicType">泛型接口类型，传入 typeof(IXxx&lt;&gt;)</param>
    /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
    public static bool HasImplementedRawGeneric(Type _judgeType, Type _basicType)
    {
        if (_judgeType == null)
            throw new ArgumentNullException(_judgeType.Name);
        if (_basicType == null)
            throw new ArgumentNullException(_basicType.Name);

        // 测试接口。
        bool isTheRawGenericType = false;
        Type[] all = _judgeType.GetInterfaces();
        for (int i = 0; i < all.Length; i++)
        {
            if (_isTheRawGenericType(_basicType, all[i]))
            {
                isTheRawGenericType = true;
                break;
            }
        }

        if (isTheRawGenericType)
            return true;

        // 测试类型。
        while (_judgeType != null && _judgeType != typeof(object))
        {
            isTheRawGenericType = _isTheRawGenericType(_basicType, _judgeType);
            if (isTheRawGenericType)
                return true;
            _judgeType = _judgeType.BaseType;
        }

        // 没有找到任何匹配的接口或类型。
        return false;
    }

    // 测试某个类型是否是指定的原始接口。
    private static bool _isTheRawGenericType(Type generic, Type test)
    {
        return generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
    }

    public static string[] GetListString(string _str, string _errMsg = "")
    {
        if (!_str.Trim().Equals(""))
        {
            string[] strs = _str.Split(new char[] { ';' });

            return strs;
        }

        return Array.Empty<string>();
    }

    public static T GetCustomSerializer<T>(string _str) where T : IColumnSerializer, new ()
    {
        if (!_str.Trim().Equals(""))
        {
            T info = new T();
            return (T)info.Deserialize(_str);
        }

        return default;
    }

    public static Vector2 GetVector2(string _str, string _errMsg = "")
    {
        Vector2 v2 = new Vector2();
        if (!_str.Trim().Equals(""))
        {
            string[] strs = _str.Split(new char[] { ';', ':' });
            if (strs.Length < 2)
            {
                Debug.LogError(string.Format("{0}数据填写错误，格式 [x;y] : {1}", _errMsg, _str));
                return v2;
            }

            v2.Set(float.Parse(strs[0]), float.Parse(strs[1]));
        }

        return v2;
    }

    public static Vector3 GetVector3(string _str, string _errMsg = "")
    {
        Vector3 v3 = new Vector3();
        if (!_str.Trim().Equals(""))
        {
            string[] strs = _str.Split(new char[] { ';', ':' });
            if (strs.Length < 3)
            {
                Debug.LogError(string.Format("{0}数据填写错误，格式 [x;y:z] : {1}", _errMsg, _str));
                return v3;
            }

            v3.Set(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
        }

        return v3;
    }

    public static Vector2Int GetVector2Int(string _str, string _errMsg = "")
    {
        Vector2Int v2 = new Vector2Int();
        if (!_str.Trim().Equals(""))
        {
            string[] strs = _str.Split(new char[] { ';', ':' });
            if (strs.Length < 2)
            {
                Debug.LogError(string.Format("{0}数据填写错误，格式 [x;y] : {1}", _errMsg, _str));
                return v2;
            }

            v2.Set(int.Parse(strs[0]), int.Parse(strs[1]));
        }
        return v2;
    }

    public static Vector3Int GetVector3Int(string _str, string _errMsg = "")
    {
        Vector3Int v3 = new Vector3Int();
        if (!_str.Trim().Equals(""))
        {
            string[] strs = _str.Split(new char[] { ';', ':' });
            if (strs.Length < 3)
            {
                Debug.LogError(string.Format("{0}数据填写错误，格式 [x;y:z] : {1}", _errMsg, _str));
                return v3;
            }

            v3.Set(int.Parse(strs[0]), int.Parse(strs[1]), int.Parse(strs[2]));
        }
        return v3;
    }

    public static Rect GetRect(string _str, string _errMsg = "")
    {
        Rect rect = new Rect();
        if (!_str.Trim().Equals(""))
        {
            string[] strs = _str.Split(new char[] { ';', ':' });
            if (strs.Length < 4)
            {
                Debug.LogError(string.Format("{0}数据填写错误，格式 [x;y;width;height] : {1}", _errMsg, _str));
                return rect;
            }

            rect.Set(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]));
        }

        return rect;
    }

    public static Color GetColor(string _str)
    {
        byte br = byte.Parse(_str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte bg = byte.Parse(_str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte bb = byte.Parse(_str.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte cc = byte.Parse(_str.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        float r = br / 255f;
        float g = bg / 255f;
        float b = bb / 255f;
        float a = cc / 255f;
        return new Color(r, g, b, a);
    }
}
