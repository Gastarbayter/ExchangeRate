using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExchangeRateModel;

namespace ShowExchangeRate
{
	internal static class Show
	{
		/// <summary>
		/// Контекст подключения к БД
		/// </summary>
		private static readonly ExchangeRateContext Context = ExchangeRateContext.Instance;

		private static void Main()
		{
			while (true)
			{
				Console.WriteLine("Введите дату");
				var input = Console.ReadLine();

				DateTime inputdate;
				if (input != null && input.Equals("q", StringComparison.CurrentCultureIgnoreCase)) break;

				if (DateTime.TryParse(input, out inputdate))
				{
					DateTime outputDate;
					var result = GetExchangeRate(inputdate, out outputDate);

					Console.WriteLine(inputdate == outputDate
						? $"\nКурс валют на {inputdate.ToShortDateString()}"
						: $"\nБлижайший прошедший рабочий день - {outputDate.ToShortDateString()}");

					foreach (var item in result)
						Console.WriteLine($"{item.Tite} = {item.Value}");
					Console.WriteLine();
				}
				else
				{
					Console.WriteLine("Не правильный формат даты!");
					Console.WriteLine("Правильный формат - " + CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
				}

			}
		}

		/// <summary>
		/// Получение курсов обмена
		/// </summary>
		/// <param name="date">День за который нужно получить курс обмена</param>
		/// <param name="outDate">Ближайший рабочий день</param>
		/// <returns></returns>
		private static IEnumerable<ExchangeRate> GetExchangeRate(DateTime date, out DateTime outDate)
		{
			switch (date.DayOfWeek)
			{
				case DayOfWeek.Sunday:
					date = date.AddDays(-2);
					break;
				case DayOfWeek.Saturday:
					date = date.AddDays(-1);
					break;
			}

			IQueryable<ExchangeRate> result;
			while (true)
			{
				result = Context.ExchangeRates.Where(x => x.Date == date);

				if (result.Any()) break;
				date = date.AddDays(-1);
			}
			outDate = date;
			return result.ToList();
		}
	}
}