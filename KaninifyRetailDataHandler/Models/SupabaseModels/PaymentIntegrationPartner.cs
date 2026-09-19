using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("PaymentIntegrationPartner")]
    public class PaymentIntegrationPartner
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("ClientId")]
        public Guid ClientId { get; set; }

        [Column("ClientSecret")]
        public string ClientSecret { get; set; } = string.Empty;

        [Column("PaymentProvider")]
        public string PaymentProvider { get; set; } = string.Empty;

        [Column("Environment")]
        public string Environment { get; set; } = string.Empty;

        [Column("ApiBaseUrl")]
        public string? ApiBaseUrl { get; set; }

        [Column("OAuthBaseUrl")]
        public string? OAuthBaseUrl { get; set; }
    }
}
