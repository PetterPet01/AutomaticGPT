//using System.Diagnostics;

namespace PetterPet.FreeGPT.Helper
{
    public static class MiscHelper
    {
        static Random random = new Random();
        static char[] lookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        public static string HexStr(byte[] data)
        {
            int i = 0, p = 0, l = data.Length;
            char[] c = new char[l * 2];
            byte d;
            while (i < l)
            {
                d = data[i++];
                c[p++] = lookup[d / 0x10];
                c[p++] = lookup[d % 0x10];
            }
            return new string(c, 0, c.Length);
        }
        public static string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            return HexStr(buffer);
        }

        static int[] seperation = new int[] { 7, 14, 19, 24 };
        public static string GetRandomId()
        {
            string result = GetRandomHexNumber(32);
            foreach (int i in seperation)
                result = result.Insert(i, "-");
            return result;
        }

        //public static IFrame? GetFrame(this ChromiumWebBrowser browser, string frameName)
        //{
        //    IFrame? frame = null;

        //    var identifiers = browser.GetBrowser().GetFrameIdentifiers();

        //    foreach (var i in identifiers)
        //    {
        //        frame = browser.GetBrowser().GetFrame(i);
        //        //Debug.WriteLine(frame.Name);
        //        if (frame.Name == frameName)
        //            return frame;
        //    }

        //    return null;
        //}

        //public static Dictionary<string, string> GetHeaders(Dictionary<string, string> source, Dictionary<string, string> changes)
        //{
        //    NameValueCollection headers = new NameValueCollection();
        //    foreach (var header in source)
        //    {
        //        if (header.Value == "" || changes.ContainsKey(header.Key))
        //        {
        //            if (changes.TryGetValue(header.Key, out var value))
        //                headers.Add(header.Key, value);
        //        }
        //        else
        //            headers.Add(header.Key, header.Value);
        //    }

        //    return headers;
        //}

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }
}
