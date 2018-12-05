using System;
using System.Text;

namespace Translit.Entity
{
	static class Rc4
	{
		private static int[] _c;
		private static int[] _k;
		private static int[] _key;
		private static int[] _content;
		private static int _swapI;
		private static int _swapJ;
		private static int _max;

		public static string Calc(string key, string content)
		{
			_max = Math.Max(content.Length, key.Length);
			_c = new int[_max];
			_k = new int[_max];

			_key = new int[_max];
			_content = new int[_max];

			Init(key, content);
			Generates();
			Preparation();

			var stringBuilder = new StringBuilder();

			for (var i = 0; i < _key.Length; i++)
			{
				var symbol = _key[i] ^ _content[i];
				stringBuilder.Append((char) symbol);
			}

			return stringBuilder.ToString();
		}

		private static void Init(string key, string content)
		{
			for (var i = 0; i < _max; i++)
			{
				_c[i] = i;
				_k[i] = key[i % key.Length];

				if (content.Length > i) _content[i] = content[i];
			}
		}

		private static void Generates()
		{
			var j = 0;
			for (var i = 0; i < _max; i++)
			{
				j = (j + _c[i] + _k[i]) % _max;
				_swapI = i;
				_swapJ = j;
				_c[_swapJ] = _swapI;
				_c[_swapI] = _swapJ;
			}
		}

		private static void Preparation()
		{
			var j = 0;
			var m = 0;
			for (var i = 0; i < _max; i++)
			{
				m = (m + 1) % _max;
				j = (j + _c[m]) % _max;
				_swapI = m;
				_swapJ = j;
				_c[_swapJ] = _swapI;
				_c[_swapI] = _swapJ;
				_key[i] = _c[(_c[m] + _c[j]) % _max];
			}
		}
	}
}
