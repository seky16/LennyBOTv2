using System;

namespace LennyBOTv2
{
    public static class MarkdownExtensions
    {
        private static string Escape(this string text, char ch)
            => text.Replace(ch.ToString(), @"\" + ch);

        private static string SurroundWith(this string text, string symbol)
            => symbol + text + symbol;

        public static string BlockQuote(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return "> " + text.Replace(Environment.NewLine, "\n").Replace("\n", "\n> ");
        }

        public static string Bold(this string? text, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('*');

            return text.SurroundWith("**");
        }

        public static string BoldItalics(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Bold().Italics(false);
        }

        public static string Code(this string? text, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('`');

            return text.SurroundWith("`");
        }

        public static string CodeMultiline(this string? text, string? lang = null, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('`');

            return (lang + Environment.NewLine + text).SurroundWith("```");
        }

        public static string HideLinkPreview(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = Uri.EscapeDataString(text);

            return "<" + text + ">";
        }

        public static string HideLinkPreview(this Uri? url)
        {
            var text = url?.ToString();

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = Uri.EscapeDataString(text);

            return "<" + text + ">";
        }

        public static string Hyperlink(this string? text, string? url)
        {
            if (string.IsNullOrEmpty(text))
                return HideLinkPreview(url);

            if (string.IsNullOrEmpty(url))
                return text;

            url = Uri.EscapeDataString(url);

            return $"[{text}]({url})";
        }

        public static string Hyperlink(this string? text, Uri? url)
        {
            if (string.IsNullOrEmpty(text))
                return HideLinkPreview(url);

            var urlStr = url?.ToString();
            if (string.IsNullOrEmpty(urlStr))
                return text;

            urlStr = Uri.EscapeDataString(urlStr);

            return $"[{text}]({urlStr})";
        }

        public static string Italics(this string? text, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('*');

            return text.SurroundWith("*");
        }

        public static string Spoiler(this string? text, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('|');

            return text.SurroundWith("||");
        }

        public static string Strikethrough(this string? text, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('~');

            return text.SurroundWith("~~");
        }

        public static string Underline(this string? text, bool escape = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (escape)
                text = text.Escape('_');

            return text.SurroundWith("__");
        }

        public static string UnderlineItalics(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Underline().Italics();
        }

        public static string UnderlineBold(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Underline().Bold();
        }

        public static string UnderlineBoldItalics(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Underline().Bold().Italics(false);
        }
    }
}
