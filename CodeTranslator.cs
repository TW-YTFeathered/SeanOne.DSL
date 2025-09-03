using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeanOne.DSL
{
    public class CodeTranslator
    {
        // 主執行方法
        public static string Run(object obj, string code)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            code = code.Trim(); // 去除前後空白
                                // 解碼並執行
                                // 選擇適當的方法
            string result = Decoder(obj, code);
            return result;
        }

        // 解碼並選擇適當的方法
        private static string Decoder(object obj, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return string.Empty;
            }

            string directive = code.Contains("/") ? code.Substring(0, code.IndexOf('/')).Trim() : code;

            Dictionary<string, Func<string>> actions = new Dictionary<string, Func<string>>
            {
                { "fe", () => FE(obj, code) },
                { "print", () => Print(obj, code) }
            };

            if (actions.TryGetValue(directive, out var func))
            {
                return func();
            }
            else
            {
                if (code.StartsWith("/"))
                {
                    return Print(obj, code);
                }
                else
                {
                    throw new ArgumentException($"Unknown functions directive: {directive}");
                }
            }
        }

        #region FE Method
        private static string FE(object obj, string code)
        {
            // 確保 obj 是 IEnumerable 且不是 string
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable == null || obj is string)
                throw new ArgumentException("Object must be enumerable (and not a string) for 'fe' code.");

            // 提前提取所有參數
            string end = Get.ParameterValueOrDefault(code, "/end:", string.Empty);
            string format = Get.ParameterValueOrDefault(code, "/tostring:", string.Empty);
            string dicFormat = Get.ParameterValueOrDefault(code, "/dicformat:", string.Empty);
            string keyFormat = Get.ParameterValueOrDefault(code, "/keyformat:", string.Empty);
            string valueFormat = Get.ParameterValueOrDefault(code, "/valueformat:", string.Empty);

            // 提取並解析 /exclude-last-end: 參數
            bool shouldExcludeLastEnd = false;
            string excludeLastEndValue = Get.ExtractParameterValue(code, "/exclude-last-end:");
            if (!string.IsNullOrEmpty(excludeLastEndValue) &&
                bool.TryParse(excludeLastEndValue, out bool parsedEndPrint))
            {
                shouldExcludeLastEnd = parsedEndPrint;
            }

            // 驗證格式參數
            if (!string.IsNullOrEmpty(format))
            {
                ValidateEnumerableFormattable(enumerable, format);
            }

            // 處理字典類型
            if (obj is IDictionary dictionary)
            {
                if (!Judge.ValidateCodeParametersAuto(code, dictionary.GetType(), out var invalidParams))
                {
                    throw new ArgumentException($"Invalid parameters for dictionary processing: {string.Join(", ", invalidParams.Select(p => "/" + p))}");
                }
                return FE_ProcessDictionary(dictionary, dicFormat, keyFormat, valueFormat, end, shouldExcludeLastEnd);
            }

            // 處理普通集合類型
            if (!Judge.ValidateCodeParametersAuto(code, enumerable.GetType(), out var invalidParamsForEnum))
            {
                throw new ArgumentException($"Invalid parameters for enumerable processing: {string.Join(", ", invalidParamsForEnum.Select(p => "/" + p))}");
            }
            return FE_ProcessEnumerable(enumerable, format, end, shouldExcludeLastEnd);
        }

        // 處理字典集合
        private static string FE_ProcessDictionary(IDictionary dictionary, string dicFormat, string keyFormat, string valueFormat, string end, bool shouldExcludeLastEnd)
        {
            if (string.IsNullOrEmpty(dicFormat))
                throw new ArgumentException("'/dicformat:' parameter is required when processing dictionaries.");

            var results = new StringBuilder();

            foreach (DictionaryEntry item in dictionary)
            {
                string keyStr = FormatObject(item.Key, keyFormat);
                string valueStr = FormatObject(item.Value, valueFormat);

                results.Append(string.Format(dicFormat, keyStr, valueStr)).Append(end);
            }

            return RemoveLastEndIfNeeded(results, end, shouldExcludeLastEnd);
        }

        // 處理普通集合
        private static string FE_ProcessEnumerable(IEnumerable enumerable, string format, string end, bool shouldExcludeLastEnd)
        {
            var results = new StringBuilder();

            foreach (var item in enumerable)
            {
                string itemString = FormatObject(item, format);
                results.Append(itemString).Append(end);
            }

            return RemoveLastEndIfNeeded(results, end, shouldExcludeLastEnd);
        }
        #endregion

        #region Print Method
        // 處理單個對象的打印
        private static string Print(object obj, string code)
        {
            string end = string.Empty;
            string format = string.Empty;

            // Extract /end: parameter
            if (Judge.HasString(code, "/end:"))
            {
                end = Get.ExtractParameterValue(code, "/end:");
            }

            // Extract /tostring: parameter
            if (Judge.HasString(code, "/tostring:"))
            {
                format = Get.ExtractParameterValue(code, "/tostring:");

                // Only validate numeric type if the object is numeric
                if (obj != null && !Judge.SafeToString(obj))
                {
                    throw new ArgumentException($"Collection elements must implement IFormattable for '/tostring:'. Found: {obj.GetType().Name}");
                }
            }

            if (!Judge.ValidateCodeParametersAuto(code, obj?.GetType() ?? typeof(object), out var invalidParams))
            {
                throw new ArgumentException($"Invalid parameters for print processing: {string.Join(",", invalidParams.Select(p => "/" + p))}");
            }

            // Handle formatting
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

        // 根據需要移除最後的end字符串
        private static string RemoveLastEndIfNeeded(StringBuilder builder, string end, bool shouldExcludeLastEnd)
        {
            // 如果不需要移除，或者end是空的，或者builder長度小於end長度，直接返回
            if (!shouldExcludeLastEnd || string.IsNullOrEmpty(end) || builder.Length < end.Length)
                return builder.ToString();

            // 移除最後的end字符串
            string result = builder.ToString();

            // 確保最後的end字符串存在於結果中
            if (result.EndsWith(end))
                return result.Substring(0, result.Length - end.Length);

            return result;
        }
    }
}