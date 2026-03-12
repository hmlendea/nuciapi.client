using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NuciAPI.Client
{
    static class QueryStringBuilder
    {
        public static Dictionary<string, string> Build(object obj)
        {
            Dictionary<string, string> result = [];
            AppendObject(result, obj, null);

            return result;
        }

        static void AppendObject(
            Dictionary<string, string> result,
            object obj,
            string prefix)
        {
            if (obj is null)
            {
                return;
            }

            Type type = obj.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                object value = property.GetValue(obj);

                if (value is null)
                {
                    continue;
                }

                string name = GetQueryName(property);

                string key =
                    prefix is null
                    ? name
                    : $"{prefix}.{name}";

                if (IsSimple(value.GetType()))
                {
                    result[key] = Convert.ToString(value);
                }
                else
                {
                    AppendObject(result, value, key);
                }
            }
        }

        static string GetQueryName(PropertyInfo property)
        {
            FromQueryAttribute attribute =
                property.GetCustomAttribute<FromQueryAttribute>();

            if (attribute is not null &&
                !string.IsNullOrWhiteSpace(attribute.Name))
            {
                return attribute.Name;
            }

            return JsonNamingPolicy.CamelCase.ConvertName(property.Name);
        }

        static bool IsSimple(Type type)
        {
            return
                type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(Guid) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(decimal);
        }
    }
}