using System.Text;

namespace SourceLocalizationGenerator.SourceGenerators;

public static class LiteralConversion
{
    // Copied from
    // https://github.com/dotnet/runtime/blob/9b57a265c7efd3732b035bade005561a04767128/src/libraries/System.CodeDom/src/Microsoft/CSharp/CSharpCodeGenerator.cs#L3185
    public static string QuoteSnippetStringCStyle(string value)
    {
        var b = new StringBuilder(value.Length + 5);

        b.Append('\"');

        int i = 0;
        while (i < value.Length)
        {
            switch (value[i])
            {
                case '\r':
                    b.Append("\\r");
                    break;
                case '\t':
                    b.Append("\\t");
                    break;
                case '\"':
                    b.Append("\\\"");
                    break;
                case '\'':
                    b.Append("\\\'");
                    break;
                case '\\':
                    b.Append("\\\\");
                    break;
                case '\0':
                    b.Append("\\0");
                    break;
                case '\n':
                    b.Append("\\n");
                    break;
                case '\u2028':
                case '\u2029':
                case '\u0084':
                case '\u0085':
                    AppendEscapedChar(b, value[i]);
                    break;

                default:
                    b.Append(value[i]);
                    break;
            }

            i++;
        }

        b.Append('\"');

        return b.ToString();
    }

    public static void AppendEscapedChar(StringBuilder b, char value)
    {
        b.Append("\\u");
        b.Append(((int)value).ToString("X4"));
    }

    public static string EscapedCharContents(char value)
    {
        switch (value)
        {
            case '\r':
                return "\\r";
            case '\t':
                return "\\t";
            case '\"':
                return "\\\"";
            case '\'':
                return "\\\'";
            case '\\':
                return "\\\\";
            case '\0':
                return "\\0";
            case '\n':
                return "\\n";

            case '\u2028':
            case '\u2029':
            case '\u0084':
            case '\u0085':
                return UnicodeEscapeChar(value);

            default:
                return value.ToString();
        }
    }

    public static string UnicodeEscapeChar(char value)
    {
        return $@"\u{(int)value:X4}";
    }
}
