using System;
using System.Text;

namespace SeanOne.DSL
{
    internal static class DslSyntaxBuilder
    {
        // 新增參數值
        public static StringBuilder AppendQuoted(this StringBuilder sb, string value)
            => sb.Append(DslSymbols.QuoteSymbol).Append(value).Append(DslSymbols.QuoteSymbol);

        // 新增參數
        public static StringBuilder AppendParam(this StringBuilder sb, string param)
            => sb.Append(DslSymbols.ParamPrefix).Append(param).Append(DslSymbols.ParamSeparator);

        // 將值可能導致解析錯誤的改為解析不會錯誤
        public static string EscapeDslValue(string value)
        {
            StringBuilder escaped = new StringBuilder();

            foreach (char ch in value)
            {
                if (ch == '\"')
                    escaped.Append("\\u0022");
                else
                    escaped.Append(ch);
            }
            return escaped.ToString();
        }

        /// <summary>
        /// 建立 DSL 參數鍵
        /// </summary>
        /// <param name="paramName"> 要建置的字串 </param>
        public static string BuildParamKey(string paramName)
        {
            return DslSymbols.ParamPrefix + paramName + DslSymbols.ParamSeparator;
        }
    }

    // 建置公開參數
    public class DslFormatBuilder
    {
        public enum FunctionName
        {
            FeSequence,
            FeDictionary,
            Basic
        }

        // 泛型工廠方法
        public static IDslFunction<TParam> SelectFunction<TParam>(FunctionName funcName)
        {
            switch (funcName)
            {
                case FunctionName.FeSequence:
                    return (IDslFunction<TParam>)new FeSequenceFunc();
                case FunctionName.FeDictionary:
                    return (IDslFunction<TParam>)new FeDictionaryFunc();
                case FunctionName.Basic:
                    return (IDslFunction<TParam>)new BasicFunc();
                default:
                    throw new ArgumentOutOfRangeException(nameof(funcName), funcName, null);
            }
        }

        public static IBasicDslFunction<BasicParam> SelectBasic() => new BasicFunc();
        public static ISequenceDslFunction<FeSeqParam> SelectFeSeq() => new FeSequenceFunc();
        public static IDictionaryDslFunction<FeDictParam> SelectFeDict() => new FeDictionaryFunc();
    }

    // 執行run的介面
    public class DslExecutable
    {
        private readonly string _dsl;

        internal DslExecutable(string dsl)
        {
            _dsl = dsl;
        }

        public string Run(object obj)
        {
            return DslFormatter.Format(obj, _dsl);
        }

        public override string ToString() => _dsl;
    }

    #region Generics
    // 泛型基底介面
    public interface IDslFunction<TParam>
    {
        IDslFunction<TParam> With(TParam param, string value);
        DslExecutable Build();
    }

    public interface IBasicDslFunction<TParam> : IDslFunction<TParam> { }
    public interface ISequenceDslFunction<TParam> : IDslFunction<TParam> { }
    public interface IDictionaryDslFunction<TParam> : IDslFunction<TParam> { }
    #endregion

    // 實作 BuildRun 介面，後續估計全部語法糖也會放這裡
    public static class DslFunctionExtensions
    {
        public static string BuildRun<TParam>(this IDslFunction<TParam> func, object obj)
            => func.Build().Run(obj);
    }

    #region Fe Method
    // Fe - ISequenceDslFunction
    public enum FeSeqParam
    {
        End, ToString, FinalPairSeparator, ExcludeLastEnd
    }
    public class FeSequenceFunc : ISequenceDslFunction<FeSeqParam>
    {
        private readonly StringBuilder _sb = new StringBuilder();

        internal FeSequenceFunc()
        {
            _sb.Append("fe ");
        }

        public IDslFunction<FeSeqParam> With(FeSeqParam param, string value)
        {
            value = DslSyntaxBuilder.EscapeDslValue(value);

            switch (param)
            {
                case FeSeqParam.End:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("end")).AppendQuoted(value);
                    break;
                case FeSeqParam.ExcludeLastEnd:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("exclude-last-end")).AppendQuoted(value);
                    break;
                case FeSeqParam.ToString:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("tostring")).AppendQuoted(value);
                    break;
                case FeSeqParam.FinalPairSeparator:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("final-pair-separator")).AppendQuoted(value);
                    break;
            }
            return this; // 回傳自己，支援 Fluent DSL
        }

        public DslExecutable Build()
        {
            return new DslExecutable(_sb.ToString());
        }
    }

    public enum FeDictParam
    {
        End, FinalPairSeparator, ExcludeLastEnd, DictFormat, KeyFormat, ValueFormat
    }
    // Fe - IDictionaryDslFunction
    public class FeDictionaryFunc : IDictionaryDslFunction<FeDictParam>
    {
        private readonly StringBuilder _sb = new StringBuilder();

        internal FeDictionaryFunc()
        {
            _sb.Append("fe ");
        }

        public IDslFunction<FeDictParam> With(FeDictParam param, string value)
        {
            value = DslSyntaxBuilder.EscapeDslValue(value);

            switch (param)
            {
                case FeDictParam.DictFormat:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("dict-format")).AppendQuoted(value);
                    break;
                case FeDictParam.End:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("end")).AppendQuoted(value);
                    break;
                case FeDictParam.ExcludeLastEnd:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("exclude-last-end")).AppendQuoted(value);
                    break;
                case FeDictParam.FinalPairSeparator:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("final-pair-separator")).AppendQuoted(value);
                    break;
                case FeDictParam.KeyFormat:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("key-format")).AppendQuoted(value);
                    break;
                case FeDictParam.ValueFormat:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("value-format")).AppendQuoted(value);
                    break;
            }
            return this; // 回傳自己，支援 Fluent DSL
        }

        public DslExecutable Build()
        {
            return new DslExecutable(_sb.ToString());
        }
    }
    #endregion

    #region Basic Method
    // Basic
    public enum BasicParam { ToString, End }
    public class BasicFunc : IBasicDslFunction<BasicParam>
    {
        private readonly StringBuilder _sb = new StringBuilder();

        internal BasicFunc()
        {
            _sb.Append("basic ");
        }

        public IDslFunction<BasicParam> With(BasicParam param, string value)
        {
            value = DslSyntaxBuilder.EscapeDslValue(value);

            switch (param)
            {
                case BasicParam.ToString:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("tostring")).AppendQuoted(value);
                    break;
                case BasicParam.End:
                    _sb.Append(DslSyntaxBuilder.BuildParamKey("end")).AppendQuoted(value);
                    break;
            }
            return this;
        }

        public DslExecutable Build()
        {
            return new DslExecutable(_sb.ToString());
        }
    }
    #endregion
}
