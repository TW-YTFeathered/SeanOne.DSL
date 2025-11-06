using System;

namespace SeanOne.DSL
{
    /// <summary>
    /// DSL 符號定義類別，集中管理 DSL 語法解析中使用的各種符號常數。
    /// </summary>
    internal class DslSymbols
    {
        /// <summary>
        /// 引號符號，用於包裹字串值，標示其開始與結束。
        /// </summary>
        public static string QuoteSymbol { get; } = "\"";

        /// <summary>
        /// 參數前綴符號，用於標示 DSL 參數的起始。
        /// </summary>
        public static string ParamPrefix { get; } = "/";

        /// <summary>
        /// 參數分隔符號，用於分隔參數名稱與其對應的值。
        /// </summary>
        public static string ParamSeparator { get; } = ":";
    }
}
