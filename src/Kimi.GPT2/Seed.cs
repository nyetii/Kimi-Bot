using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Kimi.GPT2;

internal enum PremadeSeed
{
    Void,
    [Description("alexandre de moraes")]
    Alexandre,
    Maconha,
    STF,
    Liberdade,
    Capitalismo,
    Flow,
    Ditadura,
    Bolsonaro,
    Lula,
    Dolar,
    [Description("elon musk")]
    ElonMusk
}

internal static class SeedUtil
{
    private static readonly ConcurrentDictionary<string, string?> DisplayValueCache = new();

    public static string? DisplayValue(this Enum value)
    {
        var key = $"{value?.GetType().FullName}.{value}";

        var displayValue = DisplayValueCache.GetOrAdd(key, x =>
        {
            var item = (DescriptionAttribute[])value?
                .GetType()
                .GetTypeInfo()
                .GetField(value?.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);

            return item?.Length > 0 ? item[0]?.Description : value?.ToString();
        });

        return displayValue;
    }

    public static int Length(this PremadeSeed type)
    {
        return type.GetType().GetEnumNames().Length;
    }
}