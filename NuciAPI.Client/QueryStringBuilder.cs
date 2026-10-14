using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            object fromQuery =
                property.GetCustomAttributes()
                    .FirstOrDefault(a => a.GetType().Name.Equals("FromQueryAttribute"));

            if (fromQuery is not null)
            {
                PropertyInfo nameProperty =
                    fromQuery.GetType().GetProperty("Name");

                string name = nameProperty?.GetValue(fromQuery) as string;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            JsonPropertyNameAttribute jsonName = property
                .GetCustomAttribute<JsonPropertyNameAttribute>();

            if (jsonName is not null)
            {
                return jsonName.Name;
            }

            return JsonNamingPolicy.CamelCase.ConvertName(property.Name);
        }

        static bool IsSimple(Type type) =>
            type.IsPrimitive ||
            type == typeof(string) ||
            type == typeof(Guid) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(decimal);
    }
}