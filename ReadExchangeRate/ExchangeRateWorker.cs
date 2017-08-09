using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExchangeRateModel;

namespace GetExchangeRate
{
	/// <summary>
	/// Класс обрабатывающий курсы обмена
	/// </summary>
	public static class ExchangeRateWorker
	{
		/// <summary>
		/// Базовая url
		/// </summary>
		private const string BASE_URL =
			"https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/year.txt?year=";

		/// <summary>
		/// Контекст подключения к БД
		/// </summary>
		private static readonly ExchangeRateContext Context = ExchangeRateContext.Instance;

		/// <summary>
		/// Курсы валют
		/// </summary>
		private static readonly ConcurrentBag<ExchangeRate> ExchangeRates = new ConcurrentBag<ExchangeRate>();

		/// <summary>
		/// Выполняет сохранение в БД в отдельном потоке
		/// </summary>
		private static readonly BackgroundWorker Worker = new BackgroundWorker();

		/// <summary>
		/// Флаг процесса сохранения
		/// </summary>
		private static bool _isSave = true;

		/// <summary>
		/// Даты которые уже занесенные в БД
		/// </summary>
		private static ConcurrentBag<DateTime> _datesInDatabase;

		/// <summary>
		/// Получение и сохранения в базу курсов обмена
		/// </summary>
		/// <param name="args">Массив лет за которые нужно получить курсы валют</param>
		public static void Work(IEnumerable<string> args)
		{
			Console.TreatControlCAsInput = true;

			Worker.RunWorkerCompleted += WorkerCompleted;
			Worker.DoWork += (sender, e) =>
			{
				Console.Write("Сохранение ");
				Context.ExchangeRates.AddRange(ExchangeRates);
				Context.SaveChanges();
			};

			var spinner = new Spinner();
			var years = ConvertArgs(args);

			_datesInDatabase = new ConcurrentBag<DateTime>(
				Context.ExchangeRates
					.Where(x => years.Contains(x.Date.Year))
					.GroupBy(x => x.Date)
					.Select(x => x.Key)
					.ToList());

			Parallel.ForEach(years, GetExchangeRates);

			Worker.RunWorkerAsync();

			while (_isSave)
				spinner.Turn();
		}

		/// <summary>
		/// Обработка завершения сохранения курсов валют.
		/// </summary>
		private static void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_isSave = false;
			Console.SetCursorPosition(0, Console.CursorTop);
			if (e.Error != null)
				Console.WriteLine("Ошибка сохранения: " + e.Error.Message);
			else
				Console.WriteLine("Сохранение завершено");
		}

		/// <summary>
		/// Получение курса валют за год
		/// </summary>
		/// <param name="year">Год</param>
		private static void GetExchangeRates(int year)
		{
			Console.WriteLine($"Получение курса валют за {year} год");

			using (var stream = new WebClient().OpenRead(string.Concat(BASE_URL, year)))
			{
				if (stream == null)
				{
					Console.WriteLine($"Нет данных за {year}");
					return;
				}

				using (var reader = new StreamReader(stream))
				{
					string line;
					string[] titles = null;

					while (!string.IsNullOrEmpty(line = reader.ReadLine()))
					{
						var temp = line.Split('|');

						if (temp[0] == "Date")
							titles = temp;
						else
							ValuesParsing(temp, titles);
					}
				}
			}
			Console.WriteLine($"Получение курса валют за {year} год закончено");
		}

		/// <summary>
		/// Конверитирование массива лет за которые нужно получить курсы валют 
		/// </summary>
		/// <param name="args">Массив лет за которые нужно получить курсы валют</param>
		private static ICollection<int> ConvertArgs(IEnumerable<string> args)
		{
			var result = new List<int>();

			foreach (var arg in args)
			{
				int year;
				if (arg.Length == 4 && int.TryParse(arg, out year) && year >= 1991 && year <= DateTime.Now.Year)
					result.Add(year);
				else
				{
					Console.WriteLine($"Не верно указан год {arg}");
					Console.WriteLine($"Укажите год с 1991 до {DateTime.Now.Year}");
				}
			}
			return result;
		}

		/// <summary>
		/// Разбор значений
		/// </summary>
		/// <param name="values">Значения</param>
		/// <param name="titles">Наименования валют</param>
		private static void ValuesParsing(IReadOnlyList<string> values, IReadOnlyList<string> titles)
		{
			try
			{
				var date = Convert.ToDateTime(values[0].Replace(' ', '.'));
				if (_datesInDatabase.Any(x => x == date)) return;

				Parallel.For(1, values.Count, index =>
				{
					ExchangeRates.Add(new ExchangeRate
					{
						Date = date,
						Tite = titles[index],
						Value = double.Parse(values[index], CultureInfo.InvariantCulture)
					});
				});
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Ошибка!: {ex.Message}, StackTrace: {ex.StackTrace}");
			}
		}
	}
}