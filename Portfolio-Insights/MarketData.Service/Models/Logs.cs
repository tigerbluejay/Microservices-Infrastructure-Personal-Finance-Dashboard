using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MarketData.Service.Models
{
    /// <summary>
    /// Represents a single simulated market update log entry.
    /// Stored in SQLite via MarketDataContext.
    /// </summary>
    public class Logs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }


        /// <summary>
        /// Asset symbol, e.g. "AAPL".
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string Asset { get; set; } = default!;


        /// <summary>
        /// Price at the time of the simulated update.
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal Price { get; set; }


        /// <summary>
        /// When this update occurred (UTC recommended).
        /// </summary>
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Optional additional metadata (for example: source, event id, notes).
        /// </summary>
        [MaxLength(256)]
        public string? Metadata { get; set; }
    }
}