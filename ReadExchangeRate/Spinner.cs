using System;
using System.Threading;

namespace GetExchangeRate
{
	/// <summary>
	/// Класс для отрисовки спиннера
	/// </summary>
	internal class Spinner
	{
		private int _counter;

		/// <summary>
		/// конструктор по умолчанию
		/// </summary>
		public Spinner() { _counter = 0; }

		/// <summary>
		/// Запуск спиннера
		/// </summary>
		public void Turn()
		{
			_counter++;

			switch (_counter % 4)
			{
				case 0: Console.Write("/"); break;
				case 1: Console.Write("-"); break;
				case 2: Console.Write(@"\"); break;
				case 3: Console.Write("|"); break;
			}
			Thread.Sleep(100);
			if (Console.CursorLeft != 0)
				Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
		}
	}
}
