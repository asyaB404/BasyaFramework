namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static string Format(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return format;

            if (args == null || args.Length == 0)
                return format;

            return string.Format(format, args);
        }

        public static bool TryParse(string str, out int result)
        {
            return int.TryParse(str, out result);
        }
    }
}