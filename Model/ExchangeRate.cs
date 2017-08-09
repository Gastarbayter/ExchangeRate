using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExchangeRateModel
{
	/// <summary>
	/// Обменный курс
	/// </summary>
	[Table("ExchangeRates")]
	public class ExchangeRate
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid ExchangeRateId { get; set; }

		/// <summary>
		/// Дата
		/// </summary>
		[Required]
		public DateTime Date { get; set; }

		/// <summary>
		/// Наименование валюты
		/// </summary>
		[Required, MaxLength(8)]
		public string Tite { get; set; }

		/// <summary>
		/// Значение курса
		/// </summary>
		[Required]
		public double Value { get; set; }
	}
}