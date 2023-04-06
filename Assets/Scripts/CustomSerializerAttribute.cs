using System;

// 自定义序列化特性
[AttributeUsage(AttributeTargets.Class)]
public class CustomSerializerAttribute : Attribute
{
    public Type type { get; set; }

    public CustomSerializerAttribute(Type _type)
    {
        type = _type;
    }
}