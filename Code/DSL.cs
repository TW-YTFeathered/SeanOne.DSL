using System;
using System.Threading.Tasks;

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
        /// <param name="obj">
        /// The object to format.
        /// </param>
        /// <param name="dslInstruction">
        /// The formatting code that specifies how to format the object.
        /// </param>
        /// <returns>
        /// Returns the formatted string.
        /// </returns>
        public static string Format(object obj, string dslInstruction)
        {
            // 檢查 物件 是否是 null
            if (obj == null)
                throw new ArgumentNullException("Input object must not be null.", nameof(obj));

            // 檢查 DSL 指令是否為空或 null
            if (string.IsNullOrWhiteSpace(dslInstruction))
                throw new ArgumentNullException("DSL instruction cannot be null or empty");

            dslInstruction = dslInstruction.Trim(); // 去除前後空白
            string result = Decoder(obj, dslInstruction); // 呼叫 Decoder 方法
            return result;
        }

        /// <summary>
        /// Asynchronously formats the specified object according to the provided DSL instruction.
        /// </summary>
        /// <param name="obj">
        /// The object to be formatted. Must not be <c>null</c>.
        /// </param>
        /// <param name="dslInstruction">
        /// A DSL (domain-specific language) instruction string that defines the formatting rules.
        /// Must not be <c>null</c> or empty.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the formatted string.
        /// </returns>
        public static async Task<string> FormatAsync(object obj, string dslInstruction)
        {
            if (obj == null)
                throw new ArgumentNullException("Input object must not be null.", nameof(obj));

            if (string.IsNullOrWhiteSpace(dslInstruction))
                throw new ArgumentNullException("DSL instruction cannot be null or empty");

            dslInstruction = dslInstruction.Trim(); // 去除前後空白

            return await Decoder_Async(obj, dslInstruction);
        }
    }
}