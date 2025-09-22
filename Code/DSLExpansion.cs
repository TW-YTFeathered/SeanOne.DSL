using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeanOne.DSL
{
    internal static class Judge
    {
        /// <summary>
        /// 檢查物件是否實作 IFormattable 介面
        /// </summary>
        public static bool SafeToString(object obj)
        {
            return obj is IFormattable;
        }

        /// <summary>
        /// 檢查 fullStr 是否包含 searchStr
        /// </summary>
        public static bool HasString(string fullStr, string searchStr)
        {
            if (string.IsNullOrWhiteSpace(fullStr) || string.IsNullOrWhiteSpace(searchStr)) // 如果任一字串為空，直接回傳 false
                return false;

            return fullStr.Contains(searchStr);
        }

        /// <summary>
        /// 檢查某個 參數 是否在 dslInstruction 中出現多次
        /// </summary>
        public static bool ValidateSingleParameter(string dslInstruction, string parameterName)
        {
            int count = CountParameterOccurrences(dslInstruction, parameterName); // 數數參數出現次數

            return count > 1;
        }

        /// <summary>
        /// 計算 參數 在 dslInstruction 中出現的次數 (使用正則表達式)
        /// </summary>
        /// <param name="dslInstruction">完整的參數字串</param>
        /// <param name="parameterName">要計數的參數名稱</param>
        private static int CountParameterOccurrences(string dslInstruction, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(dslInstruction) || string.IsNullOrWhiteSpace(parameterName))
                return 0;

            // 1. 還原跳脫引號 \" → "
            string unescaped = dslInstruction.Replace("\\\"", "\"");

            // 2. 移除所有雙引號內的內容（允許任何字元，非貪婪）
            string withoutQuotes = Regex.Replace(unescaped, "\"[^\"]*\"", string.Empty);

            // 3. 使用更精確的模式匹配參數名稱（避免部分匹配）
            // 確保參數名稱前面是空格或字符串開頭，後面是空白、冒號或字符串結尾
            string pattern = $@"(?<=^|\s){Regex.Escape(parameterName)}(?=\s|:|$)";
            return Regex.Matches(withoutQuotes, pattern).Count;
        }

        // 定義每個方法支援的參數
        private static readonly Dictionary<string, HashSet<string>> MethodParameters = new Dictionary<string, HashSet<string>>
        {
            ["basic"] = new HashSet<string> { "end", "tostring" },
            ["FE_ProcessEnumerable"] = new HashSet<string> { "end", "last-concat-string", "exclude-last-end", "tostring" },
            ["FE_ProcessDictionary"] = new HashSet<string> { "end", "last-concat-string", "exclude-last-end", "dict-format", "key-format", "value-format" }
        };

        /// <summary>
        /// 根據方法類型驗證參數
        /// </summary>
        /// <param name="dslInstruction">參數字符串</param>
        /// <param name="methodType">方法類型 (basic, FE_ProcessEnumerable, FE_ProcessDictionary)</param>
        /// <param name="invalidParams">無效參數列表</param>
        /// <returns>是否所有參數都有效</returns>
        public static bool ValidateCodeParameters(string dslInstruction, string methodType, out List<string> invalidParams)
        {
            invalidParams = new List<string>();

            if (string.IsNullOrWhiteSpace(dslInstruction))
                return true;

            if (!MethodParameters.ContainsKey(methodType))
                throw new ArgumentException($"Unsupported method types: {methodType}");

            var validParams = MethodParameters[methodType];

            // 1. 還原跳脫引號 \" → "
            string unescaped = dslInstruction.Replace("\\\"", "\"");

            // 2. 移除所有引號內的內容（允許跳脫字元）
            string withoutQuotes = Regex.Replace(unescaped, "\"(?:\\\\.|[^\"])*\"", string.Empty);

            // 3. 正則匹配引號外的參數
            var parameterPattern = @"(?<=/)([\w-]+)(?::([^/\s]*))?";
            var matches = Regex.Matches(withoutQuotes, parameterPattern);

            foreach (Match match in matches)
            {
                string paramName = match.Groups[1].Value;
                if (!validParams.Contains(paramName))
                {
                    invalidParams.Add(paramName);
                }
            }

            return invalidParams.Count == 0;
        }

        /// <summary>
        /// 自動偵測方法類型並驗證參數
        /// </summary>
        /// <param name="dslInstruction">參數字符串</param>
        /// <param name="objectType">物件類型，用於自動判斷方法</param>
        /// <param name="invalidParams">無效參數列表</param>
        /// <returns>是否所有參數都有效</returns>
        public static bool ValidateCodeParametersAuto(string dslInstruction, Type objectType, out List<string> invalidParams)
        {
            string methodType = DetermineMethodType(objectType); // 根據物件類型自動判斷方法
            return ValidateCodeParameters(dslInstruction, methodType, out invalidParams); // 驗證參數
        }

        /// <summary>
        /// 根據物件類型自動判斷應該使用的方法
        /// </summary>
        private static string DetermineMethodType(Type objectType)
        {
            if (objectType == null)
                return "basic"; // 預設為 basic

            // 如果是字典類型
            if (objectType.IsGenericType &&
                (objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                 objectType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                 objectType.GetInterfaces().Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
            {
                return "FE_ProcessDictionary";
            }

            // 如果是可枚舉類型（但不是字符串）
            if (objectType != typeof(string) &&
                (objectType.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)) ||
                 objectType.IsArray))
            {
                return "FE_ProcessEnumerable";
            }

            // 預設為 basic
            return "basic";
        }
    }

    internal static class Get
    {
        /// <summary>
        /// 從 dslInstruction 中提取參數的值
        /// </summary>
        public static string ExtractParameterValue(string dslInstruction, string parameterName)
        {
            if (string.IsNullOrEmpty(dslInstruction) || string.IsNullOrEmpty(parameterName)) // 如果任一字串為空，直接回傳空字串
                return string.Empty;

            if (Judge.ValidateSingleParameter(dslInstruction, parameterName)) // 檢查參數是否出現多次
            {
                // 如果參數出現多次，拋出例外
                throw new ArgumentException($"Parameter '{parameterName}' is specified multiple times.");
            }

            int startIndex = dslInstruction.IndexOf(parameterName); // 找到參數名稱的位置
            if (startIndex == -1) // 如果找不到參數名稱，回傳空字串
                return string.Empty;

            startIndex += parameterName.Length;

            // 跳過空白字符
            while (startIndex < dslInstruction.Length && char.IsWhiteSpace(dslInstruction[startIndex]))
            {
                startIndex++;
            }

            if (startIndex >= dslInstruction.Length) // 如果已經到達字串末尾，回傳空字串
                return string.Empty;

            // 檢查是否以引號開頭
            if (dslInstruction[startIndex] == '"')
            {
                startIndex++; // 跳過開頭的引號
                int endIndex = dslInstruction.IndexOf('"', startIndex);
                if (endIndex == -1)
                {
                    // 如果找不到結尾的引號，則取到字串末尾
                    endIndex = dslInstruction.Length;
                }

                string value = dslInstruction.Substring(startIndex, endIndex - startIndex); // 擷取參數值
                value = ConvertToUnicode.DecodeUnicodeEscapes(value); // 轉換 Unicode
                return Regex.Unescape(value); // 處理轉義字元
            }
            else
            {
                // 非引號開頭，取到下一個空白或 '/' 為止
                int endIndex = FindNextTerminator(dslInstruction, startIndex); // 尋找終止符位置
                string value = dslInstruction.Substring(startIndex, endIndex - startIndex); // 擷取參數值
                value = ConvertToUnicode.DecodeUnicodeEscapes(value); // 轉換 Unicode
                return Regex.Unescape(value); // 處理轉義字元
            }
        }

        /// <summary>
        /// 取得參數值，如果未找到參數或參數為空，則傳回預設值
        /// </summary>
        public static string ParameterValueOrDefault(string dslInstruction, string parameterName, string defaultValue)
        {
            string value = ExtractParameterValue(dslInstruction, parameterName);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// 尋找下一個終止字元（空格或“/”）
        /// </summary>
        private static int FindNextTerminator(string dslInstruction, int startIndex)
        {
            // 從 startIndex 開始尋找下一個空白或 '/' 字元
            for (int i = startIndex; i < dslInstruction.Length; i++)
            {
                char c = dslInstruction[i];
                // 如果遇到空白或 '/'，則回傳該位置
                if (char.IsWhiteSpace(c) || c == '/')
                {
                    return i;
                }
            }
            return dslInstruction.Length; // 如果沒有找到終止符，則傳回字串的結尾
        }
    }

    internal static class ConvertToUnicode
    {
        /// <summary>
        /// 將字串中的所有 \uXXXX 轉義序列轉換為其對應的 Unicode 字符
        /// </summary>
        public static string DecodeUnicodeEscapes(string input)
        {
            // 使用正則表達式來尋找所有的 \\uXXXX 序列
            return Regex.Replace(input, @"\\u([0-9a-fA-F]{4})", match =>
            {
                try
                {
                    // 將十六進位值轉換為對應的 Unicode 字符
                    return ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString();
                }
                catch
                {
                    // 如果轉換失敗，保留原始字串
                    return match.Value;
                }
            });
        }
    }
}