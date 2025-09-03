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
        public static bool SafeToString(Type type)
        {
            return type is IFormattable;
        }

        /// <summary>
        /// 檢查 fullStr 是否包含 searchStr
        /// </summary>
        public static bool HasString(string fullStr, string searchStr)
        {
            if (string.IsNullOrEmpty(fullStr) || string.IsNullOrEmpty(searchStr)) // 如果任一字串為空，直接回傳 false
                return false;

            return fullStr.Contains(searchStr);
        }

        /// <summary>
        /// 檢查某個 參數 是否在 code 中出現多次
        /// </summary>
        public static bool ValidateSingleParameter(string code, string parameterName)
        {
            int count = CountParameterOccurrences(code, parameterName); // 數數參數出現次數
            return count > 1;
        }

        /// <summary>
        /// 計算 參數 在 code 中出現的次數
        /// </summary>
        private static int CountParameterOccurrences(string code, string parameterName)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(parameterName))
                return 0;

            bool insideQuotes = false;
            var validPositions = code
                .Select((c, i) => new { Char = c, Index = i })
                .Where(x =>
                {
                    // 追蹤是否在引號內
                    if (x.Char == '"') insideQuotes = !insideQuotes;

                    // 只考慮不在引號內且是 '/' 的位置
                    return x.Char == '/' && !insideQuotes;
                })
                .Select(x => x.Index)
                .Where(pos =>
                {
                    // 檢查這個位置是否匹配參數名稱
                    return pos + parameterName.Length <= code.Length &&
                           code.Substring(pos, parameterName.Length)
                               .Equals(parameterName, StringComparison.OrdinalIgnoreCase);
                })
                .Count();

            return validPositions;
        }

        // 定義每個方法支援的參數
        private static readonly Dictionary<string, HashSet<string>> MethodParameters = new Dictionary<string, HashSet<string>>
        {
            ["print"] = new HashSet<string> { "end", "tostring" },
            ["FE_ProcessEnumerable"] = new HashSet<string> { "end", "exclude-last-end", "tostring" },
            ["FE_ProcessDictionary"] = new HashSet<string> { "end", "exclude-last-end", "dicformat", "keyformat", "valueformat" }
        };

        /// <summary>
        /// 根據方法類型驗證參數
        /// </summary>
        /// <param name="code">參數字符串</param>
        /// <param name="methodType">方法類型 (print, FE_ProcessEnumerable, FE_ProcessDictionary)</param>
        /// <param name="invalidParams">無效參數列表</param>
        /// <returns>是否所有參數都有效</returns>
        public static bool ValidateCodeParameters(string code, string methodType, out List<string> invalidParams)
        {
            invalidParams = new List<string>(); // 用於存儲無效參數

            if (string.IsNullOrWhiteSpace(code)) // 如果 code 為空，則沒有參數需要驗證
                return true;

            // 檢查方法類型是否存在
            if (!MethodParameters.ContainsKey(methodType))
            {
                throw new ArgumentException($"不支援的方法類型: {methodType}");
            }

            var validParams = MethodParameters[methodType]; // 取得該方法的有效參數列表

            // 正則表達式匹配 /參數名:值 或 /參數名 的格式
            var parameterPattern = @"/([\w-]+)(?::([^/\s]*))?";
            // 使用 Regex 找出所有參數
            var matches = Regex.Matches(code, parameterPattern);

            // 檢查每個參數是否有效
            foreach (Match match in matches)
            {
                string paramName = match.Groups[1].Value;

                // 檢查參數名是否在該方法的有效列表中
                if (!validParams.Contains(paramName))
                {
                    invalidParams.Add(paramName);
                }
            }

            return invalidParams.Count == 0; // 如果沒有無效參數，則所有參數都有效
        }

        /// <summary>
        /// 自動偵測方法類型並驗證參數
        /// </summary>
        /// <param name="code">參數字符串</param>
        /// <param name="objectType">物件類型，用於自動判斷方法</param>
        /// <param name="invalidParams">無效參數列表</param>
        /// <returns>是否所有參數都有效</returns>
        public static bool ValidateCodeParametersAuto(string code, Type objectType, out List<string> invalidParams)
        {
            string methodType = DetermineMethodType(objectType); // 根據物件類型自動判斷方法
            return ValidateCodeParameters(code, methodType, out invalidParams); // 驗證參數
        }

        /// <summary>
        /// 根據物件類型自動判斷應該使用的方法
        /// </summary>
        private static string DetermineMethodType(Type objectType)
        {
            if (objectType == null)
                return "print"; // 預設為 print

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

            // 預設為 print
            return "print";
        }
    }

    internal static class Get
    {
        /// <summary>
        /// 從 code 中提取參數的值
        /// </summary>
        public static string ExtractParameterValue(string code, string parameterName)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(parameterName)) // 如果任一字串為空，直接回傳空字串
                return string.Empty;

            if (Judge.ValidateSingleParameter(code, parameterName)) // 檢查參數是否出現多次
            {
                // 如果參數出現多次，拋出例外
                throw new ArgumentException($"Parameter '{parameterName}' is specified multiple times.");
            }

            int startIndex = code.IndexOf(parameterName); // 找到參數名稱的位置
            if (startIndex == -1) // 如果找不到參數名稱，回傳空字串
                return string.Empty;

            startIndex += parameterName.Length;

            // 跳過空白字符
            while (startIndex < code.Length && char.IsWhiteSpace(code[startIndex]))
            {
                startIndex++;
            }

            if (startIndex >= code.Length) // 如果已經到達字串末尾，回傳空字串
                return string.Empty;

            // 檢查是否以引號開頭
            if (code[startIndex] == '"')
            {
                startIndex++; // 跳過開頭的引號
                int endIndex = code.IndexOf('"', startIndex);
                if (endIndex == -1)
                {
                    // 如果找不到結尾的引號，則取到字串末尾
                    endIndex = code.Length;
                }

                string value = code.Substring(startIndex, endIndex - startIndex); // 擷取參數值
                value = ConvertToUnicode.DecodeUnicodeEscapes(value); // 轉換 Unicode
                return Regex.Unescape(value); // 處理轉義字元
            }
            else
            {
                // 非引號開頭，取到下一個空白或 '/' 為止
                int endIndex = FindNextTerminator(code, startIndex); // 尋找終止符位置
                string value = code.Substring(startIndex, endIndex - startIndex); // 擷取參數值
                value = ConvertToUnicode.DecodeUnicodeEscapes(value); // 轉換 Unicode
                return Regex.Unescape(value); // 處理轉義字元
            }
        }

        /// <summary>
        /// 取得參數值，如果未找到參數或參數為空，則傳回預設值
        /// </summary>
        public static string ParameterValueOrDefault(string code, string parameterName, string defaultValue)
        {
            string value = ExtractParameterValue(code, parameterName);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// 尋找下一個終止字元（空格或“/”）
        /// </summary>
        private static int FindNextTerminator(string code, int startIndex)
        {
            // 從 startIndex 開始尋找下一個空白或 '/' 字元
            for (int i = startIndex; i < code.Length; i++)
            {
                char c = code[i];
                // 如果遇到空白或 '/'，則回傳該位置
                if (char.IsWhiteSpace(c) || c == '/')
                {
                    return i;
                }
            }
            return code.Length; // 如果沒有找到終止符，則傳回字串的結尾
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