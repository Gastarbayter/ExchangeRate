using System;

namespace GetExchangeRate
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length == 0)
				args = new[] { DateTime.Now.Year.ToString() };

			ExchangeRateWorker.Work(args);

			Console.WriteLine("Нажмите любую клавищу для завершения");
			Console.ReadKey();
		}
	}
}