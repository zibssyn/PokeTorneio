using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class EnumHelper
{
    public static SelectList GetEnumSelectList<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().Select(e => new
        {
            Value = e.ToString(),
            Text = e.GetDescription()
        });

        return new SelectList(values, "Value", "Text");
    }

    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                              .FirstOrDefault() as DescriptionAttribute;
        return attribute == null ? value.ToString() : attribute.Description;
    }
}
