using System.ComponentModel;
using System.Reflection;
using System.Text;
using DNTPersianUtils.Core;
using Microsoft.OpenApi.Attributes;

namespace LargeExcelStreaming.Features.Base;

public static class EnumExtensions
{
    public static string GetEnumStringValue(this Enum flags)
    {
        if (flags.GetType().GetTypeInfo().GetCustomAttributes(inherit: true)
            .OfType<FlagsAttribute>()
            .Any())
        {
            string enumFlagsText = getEnumFlagsText(flags);
            if (!string.IsNullOrWhiteSpace(enumFlagsText))
            {
                return enumFlagsText;
            }
        }

        return getEnumValueText(flags);
    }

    private static string getEnumFlagsText(Enum flags)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (object value2 in Enum.GetValues(flags.GetType()))
        {
            if (flags.HasFlag((Enum)value2) && Convert.ToInt64((Enum)value2) != 0L)
            {
                string enumValueText = getEnumValueText((Enum)value2);
                char value = (enumValueText.ContainsFarsi() ? '،' : ',');
                stringBuilder.Append(enumValueText).Append(value).Append(" ");
            }
        }

        return stringBuilder.ToString().Trim().TrimEnd(new char[1] { ',' })
            .TrimEnd(new char[1] { '،' });
    }

    private static string getEnumValueText(Enum value)
    {
        string text = value.ToString();
        FieldInfo field = value.GetType().GetField(text);
        DescriptionAttribute descriptionAttribute = field?.GetCustomAttributes(inherit: true).OfType<DescriptionAttribute>().FirstOrDefault();
        if (descriptionAttribute != null)
        {
            return descriptionAttribute.Description;
        }

        DisplayNameAttribute displayNameAttribute = field?.GetCustomAttributes(inherit: true).OfType<DisplayNameAttribute>().FirstOrDefault();
        if (displayNameAttribute != null)
        {
            return displayNameAttribute.DisplayName;
        }

        DisplayAttribute displayAttribute = field?.GetCustomAttributes(inherit: true).OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttribute != null)
        {
            return displayAttribute.Name;
        }

        return text;
    }

}