using System;
using System.Text;

namespace Translit.Models.Other
{
    internal static class Rc4
    {
        public static int[] C { get; set; }

        public static int[] K { get; set; }

        public static int[] Key { get; set; }

        public static int[] Content { get; set; }

        public static int SwapI { get; set; }

        public static int SwapJ { get; set; }

        public static int Max { get; set; }

        public static string Calc(string key, string content)
        {
            Max = Math.Max(content.Length, key.Length);
            C = new int[Max];
            K = new int[Max];

            Key = new int[Max];
            Content = new int[Max];

            Init(key, content);
            Generates();
            Preparation();

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < Key.Length; i++)
            {
                var symbol = Key[i] ^ Content[i];
                stringBuilder.Append((char) symbol);
            }

            return stringBuilder.ToString();
        }

        private static void Init(string key, string content)
        {
            for (var i = 0; i < Max; i++)
            {
                C[i] = i;
                K[i] = key[i % key.Length];

                if (content.Length > i) Content[i] = content[i];
            }
        }

        private static void Generates()
        {
            var j = 0;
            for (var i = 0; i < Max; i++)
            {
                j = (j + C[i] + K[i]) % Max;
                SwapI = i;
                SwapJ = j;
                C[SwapJ] = SwapI;
                C[SwapI] = SwapJ;
            }
        }

        private static void Preparation()
        {
            var j = 0;
            var m = 0;
            for (var i = 0; i < Max; i++)
            {
                m = (m + 1) % Max;
                j = (j + C[m]) % Max;
                SwapI = m;
                SwapJ = j;
                C[SwapJ] = SwapI;
                C[SwapI] = SwapJ;
                Key[i] = C[(C[m] + C[j]) % Max];
            }
        }
    }
}