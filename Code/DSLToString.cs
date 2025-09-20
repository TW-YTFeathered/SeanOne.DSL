using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeanOne.DSL
{
    partial class DslFormatter
    {
        // 解碼並選擇適當的方法
        private static string Decoder(object obj, string dslInstruction)
        {
            // 從 DSL 指令中提取函數名稱
            string directive = dslInstruction.Contains("/") ? dslInstruction.Substring(0, dslInstruction.IndexOf('/')).Trim() : dslInstruction;

            // 創建函數名稱字典，映射到對應的執行函數
            Dictionary<string, Func<string>> actions = new Dictionary<string, Func<string>>
            {
                ["fe"] = () => FE(obj, dslInstruction),
                ["foreach"] = () => FE(obj, dslInstruction),
                ["basic"] = () => Basic(obj, dslInstruction),
            };

            // 嘗試從字典中獲取對應的執行函數
            if (actions.TryGetValue(directive, out var func))
            {
                // 找到並執行函數
                return func();
            }
            else
            {
                // 如果指令以 '/' 開頭，執行 Basic 方法
                if (dslInstruction.StartsWith("/"))
                {
                    return Basic(obj, dslInstruction);
                }
                else
                {
                    // 未知指令拋出異常
                    throw new ArgumentException($"Unknown functions directive: {directive}");
                }
            }
        }

        #region FE Method
        private static string FE(object obj, string dslInstruction)
        {
            // 確保 obj 是 IEnumerable 且不是 string
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable == null || obj is string)
                throw new ArgumentException("Object must be enumerable (and not a string) for 'fe' code.");

            // 提前提取所有參數
            string end = Get.ParameterValueOrDefault(dslInstruction, "/end:", string.Empty);
            string last_concat_string = Get.ParameterValueOrDefault(dslInstruction, "/last-concat-string:", string.Empty);
            string format = Get.ParameterValueOrDefault(dslInstruction, "/tostring:", string.Empty);
            string dicFormat = Get.ParameterValueOrDefault(dslInstruction, "/dicformat:", string.Empty);
            string keyFormat = Get.ParameterValueOrDefault(dslInstruction, "/keyformat:", string.Empty);
            string valueFormat = Get.ParameterValueOrDefault(dslInstruction, "/valueformat:", string.Empty);

            // 提取並解析 /exclude-last-end: 參數
            bool exclude_last_end = false;
            string excludeLastEndValue = Get.ExtractParameterValue(dslInstruction, "/exclude-last-end:");
            if (!string.IsNullOrEmpty(excludeLastEndValue) &&
                bool.TryParse(excludeLastEndValue, out bool parsedEndPrint))
            {
                exclude_last_end = parsedEndPrint;
            }

            // 驗證格式參數
            if (!string.IsNullOrEmpty(format))
            {
                ValidateEnumerableFormattable(enumerable, format);
            }

            // 處理字典類型
            if (obj is IDictionary dictionary)
            {
                if (!Judge.ValidateCodeParametersAuto(dslInstruction, dictionary.GetType(), out var invalidParams))
                {
                    throw new ArgumentException($"Invalid parameters for dictionary processing: {string.Join(", ", invalidParams)}");
                }
                return FE_ProcessDictionary(dictionary, dicFormat, keyFormat, valueFormat, end, last_concat_string, exclude_last_end);
            }

            // 處理普通集合類型
            if (!Judge.ValidateCodeParametersAuto(dslInstruction, enumerable.GetType(), out var invalidParamsForEnum))
            {
                throw new ArgumentException($"Invalid parameters for enumerable processing: {string.Join(", ", invalidParamsForEnum)}");
            }
            return FE_ProcessEnumerable(enumerable, format, end, last_concat_string, exclude_last_end);
        }

        // 處理字典集合
        private static string FE_ProcessDictionary(IDictionary dictionary, string dicFormat, string keyFormat, string valueFormat, string end, string last_concat_string, bool exclude_last_end)
        {
            if (string.IsNullOrEmpty(dicFormat))
                throw new ArgumentException("'/dicformat:' parameter is required when processing dictionaries.");

            var results = new StringBuilder();

            // 獲取字典的鍵集合
            ICollection keys = dictionary.Keys;
            int count = keys.Count;

            var keyList = keys.Cast<object>().ToList();

            for (int i = 0; i < count; i++)
            {
                var key = keyList[i];
                var value = dictionary[key];

                string keyStr = FormatObject(key, keyFormat);
                string valueStr = FormatObject(value, valueFormat);

                string formatted = string.Format(dicFormat, keyStr, valueStr);

                // 如果是倒數第二個，且 last_concat_string 不為 null 或空字串
                if (i == count - 2 && !string.IsNullOrEmpty(last_concat_string))
                    results.Append(formatted).Append(last_concat_string);
                // 如果是最後一個，且 exclude_last_end 為 true
                else if (i == count - 1 && exclude_last_end)
                    results.Append(formatted); // 不加 end
                else
                    results.Append(formatted).Append(end);
            }

            return results.ToString();
        }

        // 處理普通集合
        private static string FE_ProcessEnumerable(IEnumerable enumerable, string format, string end, string last_concat_string, bool exclude_last_end)
        {
            var results = new StringBuilder();

            // 將 IEnumerable 轉成 IList 以支援索引存取
            var list = enumerable.Cast<object>().ToList();
            int count = list.Count;


            for (int i = 0; i < count; i++)
            {
                string itemString = FormatObject(list[i], format);

                // 如果是倒數第二個，且 last_concat_string 不為 null 或空字串
                if (i == count - 2 && !string.IsNullOrEmpty(last_concat_string))
                    results.Append(itemString).Append(last_concat_string);
                // 如果是最後一個，且 exclude_last_end 為 true
                else if (i == count - 1 && exclude_last_end)
                    results.Append(itemString); // 不加 end
                else
                    results.Append(itemString).Append(end);
            }

            return results.ToString();
        }
        #endregion

        #region Basic Method
        // 處理單個對象的格式化
        private static string Basic(object obj, string dslInstruction)
        {
            string end = string.Empty;
            string format = string.Empty;

            end = Get.ParameterValueOrDefault(dslInstruction, "/end:", string.Empty);

            // 提取並驗證 /tostring: 參數
            if (Judge.HasString(dslInstruction, "/tostring:"))
            {
                format = Get.ExtractParameterValue(dslInstruction, "/tostring:");

                // 驗證 obj 是否實作 IFormattable
                if (obj != null && !Judge.SafeToString(obj))
                {
                    throw new ArgumentException($"Collection elements must implement IFormattable for '/tostring:'. Found: {obj.GetType().Name}");
                }
            }

            if (!Judge.ValidateCodeParameters(dslInstruction, "basic", out var invalidParams))
            {
                throw new ArgumentException($"Invalid parameters for basic processing: {string.Join(", ", invalidParams)}");
            }

            // 格式化對象
            string result = obj is IFormattable formattable && !string.IsNullOrEmpty(format)
                ? formattable.ToString(format, null)
                : obj?.ToString() ?? string.Empty;

            return result + end;
        }
        #endregion

        // 驗證集合元素是否可格式化
        private static void ValidateEnumerableFormattable(IEnumerable enumerable, string format)
        {
            foreach (var element in enumerable)
            {
                if (element != null && !Judge.SafeToString(element))
                {
                    var elementType = element.GetType();
                    throw new ArgumentException($"Collection elements must implement IFormattable for '/tostring:'. Found: {elementType.Name}");
                }
            }
        }

        // 格式化對象 (需支持 IFormattable)
        private static string FormatObject(object obj, string format)
        {
            // 如果對象為null，返回空字符串
            if (obj == null) return string.Empty;

            // 如果提供了格式且對象實現了IFormattable，則使用該格式
            if (!string.IsNullOrEmpty(format) && obj is IFormattable formattable)
                return formattable.ToString(format, null);

            // 否則，使用默認的ToString方法
            return obj.ToString() ?? string.Empty;
        }
    }
}
