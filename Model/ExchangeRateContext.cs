using System.Data.Entity;
using System.Threading;

namespace ExchangeRateModel
{
	/// <summary>
	/// Контекст базы данных ExchangeRate
	/// </summary>
	public sealed class ExchangeRateContext : DbContext
	{

		private static readonly object Mutex = new object();

		private static ExchangeRateContext _instance;

		/// <summary>
		/// конструктор по умолчанию
		/// </summary>
		private ExchangeRateContext() : base("name=ExchangeRate") { }

		/// <summary>
		/// Экземпляр контекста БД
		/// </summary>
		public static ExchangeRateContext Instance
		{
			get
			{
				if (_instance != null) return _instance;
				Monitor.Enter(Mutex);
				var temp = new ExchangeRateContext();
				Interlocked.Exchange(ref _instance, temp);
				Monitor.Exit(Mutex);
				return _instance;
			}
		}

		static ExchangeRateContext()
		{
			Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ExchangeRateContext>());
		}

		public DbSet<ExchangeRate> ExchangeRates { get; set; }
	}
}