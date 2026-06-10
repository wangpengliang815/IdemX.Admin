using System.ComponentModel;

namespace Core.Infrastructure.Utility;

public class EnumHelper
{
    /// <summary>
    /// 将枚举转成List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<EnumEntity> EnumToList<T>()
    {
        List<EnumEntity> list = new();

        foreach (var e in Enum.GetValues(typeof(T)))
        {
            EnumEntity m = new();
            object[] objArr = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (objArr != null && objArr.Length > 0)
            {
                DescriptionAttribute da = objArr[0] as DescriptionAttribute;
                m.Description = da.Description;
            }
            m.Value = Convert.ToInt32(e);
            list.Add(m);
        }
        return list;
    }

    /// <summary>
    /// 将枚举转成List（通过Type参数）
    /// </summary>
    /// <param name="enumType">枚举类型</param>
    /// <returns></returns>
    public static List<EnumEntity> EnumToList(Type enumType)
    {
        if (enumType == null)
        {
            throw new ArgumentNullException(nameof(enumType), "枚举类型不能为空");
        }

        if (!enumType.IsEnum)
        {
            throw new ArgumentException("参数必须是枚举类型", nameof(enumType));
        }

        List<EnumEntity> list = [];

        foreach (var e in Enum.GetValues(enumType))
        {
            EnumEntity m = new();
            object[] objArr = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (objArr != null && objArr.Length > 0)
            {
                DescriptionAttribute da = objArr[0] as DescriptionAttribute;
                m.Description = da.Description;
            }
            m.Value = Convert.ToInt32(e);
            list.Add(m);
        }
        return list;
    }

    /// <summary>
    /// 根据枚举值来获取单个枚举实体
    /// </summary>
    /// <typeparam name="T">枚举</typeparam>
    /// <param name="value">value</param>
    /// <returns></returns>
    public static EnumEntity GetEnumberEntity<T>(int value)
    {
        foreach (var e in Enum.GetValues(typeof(T)))
        {
            EnumEntity m = new EnumEntity();
            object[] objArr = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (objArr != null && objArr.Length > 0)
            {
                DescriptionAttribute da = objArr[0] as DescriptionAttribute;
                m.Description = da.Description;
            }
            m.Value = Convert.ToInt32(e);
            if (value == m.Value)
            {
                return m;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据枚举值value来获取单个枚举实体的文字描述内容
    /// </summary>
    /// <typeparam name="T">枚举</typeparam>
    /// <param name="value">value</param>
    /// <returns></returns>
    public static string GetEnumDescriptionByValue<T>(int value)
    {
        foreach (var e in Enum.GetValues(typeof(T)))
        {
            EnumEntity m = new();
            object[] objArr = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (objArr != null && objArr.Length > 0)
            {
                DescriptionAttribute da = objArr[0] as DescriptionAttribute;
                m.Description = da.Description;
            }
            m.Value = Convert.ToInt32(e);
            if (value == m.Value)
            {
                return m.Description;
            }
        }
        return "";
    }

    /// <summary>
    /// 根据枚举key来获取单个枚举实体的文字描述内容
    /// </summary>
    /// <typeparam name="T">枚举</typeparam>
    /// <param name="key">value</param>
    /// <returns></returns>
    public static string GetEnumDescriptionByKey<T>(string key)
    {
        foreach (var e in Enum.GetValues(typeof(T)))
        {
            EnumEntity m = new();
            object[] objArr = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (objArr != null && objArr.Length > 0)
            {
                DescriptionAttribute da = objArr[0] as DescriptionAttribute;
                m.Description = da.Description;
            }
            m.Value = Convert.ToInt32(e);
            if (key == e.ToString())
            {
                return m.Description;
            }
        }
        return "";
    }
}

/// <summary>
/// 枚举实体
/// </summary>
public class EnumEntity
{
    /// <summary>
    /// 枚举的描述
    /// </summary>
    public string Description { set; get; }

    /// <summary>
    /// 枚举对象的值
    /// </summary>
    public int Value { set; get; }
}
