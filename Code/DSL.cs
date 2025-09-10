using System;

namespace SeanOne.DSL
{
    /// <summary>
    /// A DSL (Domain-Specific Language) class used for formatting objects.
    /// </summary>
    public partial class DslFormatter
    {
        /// <summary>
        /// Formats an object.
        /// </summary>
        /// <param name="obj">The object to format.</param>
        /// <param name="dslInstruction">The formatting code that specifies how to format the object.</param>
        /// <returns>Returns the formatted string.</returns>
        public static string Format(object obj, string dslInstruction)
        {
            // 檢查 物件 是否是 null
            if (obj == null)
                throw new ArgumentException("Input object must not be null.", nameof(obj));

            // 檢查 DSL 指令是否為空或 null
            if (string.IsNullOrWhiteSpace(dslInstruction))
                throw new ArgumentException("DSL instruction cannot be null or empty");

            dslInstruction = dslInstruction.Trim(); // 去除前後空白
                                                    // 解碼並執行
                                                    // 選擇適當的方法
            string result = Decoder(obj, dslInstruction); // 呼叫 Decoder 方法
            return result;
        }
    }
}