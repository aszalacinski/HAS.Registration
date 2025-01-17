﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HAS.Registration
{
    public static class TempDataExtensions
    {
        public static void Set<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonSerializer.Serialize(value, DefaultJsonSettings.Settings);
        }
        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            tempData.TryGetValue(key, out object o);
            return o == null ? null : JsonSerializer.Deserialize<T>((string)o, DefaultJsonSettings.Settings);
        }

        public static T Peek<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o;
            o = tempData.Peek(key);
            return o == null ? null : JsonSerializer.Deserialize<T>((string)o, DefaultJsonSettings.Settings);
        }
    }
}
