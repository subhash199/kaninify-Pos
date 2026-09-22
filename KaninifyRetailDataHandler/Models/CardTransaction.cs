using EntityFrameworkDatabaseLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataHandlerLibrary.Models
{
    public class CardTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Payments are journaled before checkout assigns the sale's database ID.
        public int? SalesTransaction_Id { get; set; }
        public virtual SalesTransaction? SalesTransaction { get; set; }

        [Required, MaxLength(40)]
        public string Transaction_Reference { get; set; } = string.Empty;

        [Required, MaxLength(40)]
        public string Provider { get; set; } = "Teya";

        [Required, MaxLength(20)]
        public string Environment { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Idempotency_Key { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Payment_Request_Id { get; set; }

        [MaxLength(200)]
        public string? Gateway_Payment_Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Preserve Teya's exact minor-unit values separately from the display amount.
        public int? Requested_Amount_Minor_Units { get; set; }
        public int? Requested_Tip_Minor_Units { get; set; }

        [Required, MaxLength(3)]
        public string Currency_Code { get; set; } = "GBP";

        [Required, MaxLength(20)]
        public string Transaction_Type { get; set; } = "SALE";

        [Required, MaxLength(40)]
        public string Status { get; set; } = "INITIATED";
        public string? Status_Reason { get; set; }
        public string? Progress_Status { get; set; }

        [MaxLength(100)]
        public string? Store_Id { get; set; }

        [MaxLength(100)]
        public string? Terminal_Id { get; set; }

        [MaxLength(200)]
        public string? Epos_Instance_Id { get; set; }

        [MaxLength(200)]
        public string? Merchant_Reference { get; set; }

        [Column(TypeName = "jsonb")]
        public CardTransactionMetadata? Metadata { get; set; }

        public DateTime? Provider_Created_At { get; set; }
        public DateTime? Provider_Updated_At { get; set; }
        public DateTime? Transaction_Timestamp { get; set; }
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
    }

    public class CardTransactionMetadata
    {
        [JsonPropertyName("payment_method_details")]
        public CardPaymentMethodDetails? PaymentMethodDetails { get; set; }

        [JsonPropertyName("dcc")]
        public JsonElement? Dcc { get; set; }

        [JsonPropertyName("receipt_text")]
        public string? ReceiptText { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    public class CardPaymentMethodDetails
    {
        [JsonPropertyName("card")]
        public CardPaymentCardDetails? Card { get; set; }

        [JsonPropertyName("entry_mode")]
        public string? EntryMode { get; set; }

        [JsonPropertyName("verification_method")]
        public string? VerificationMethod { get; set; }

        [JsonPropertyName("application_name")]
        public string? ApplicationName { get; set; }

        [JsonPropertyName("application_id")]
        public string? ApplicationId { get; set; }

        [JsonPropertyName("mid")]
        public string? Mid { get; set; }

        [JsonPropertyName("response_code")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("authorisation_code")]
        public string? AuthorisationCode { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    public class CardPaymentCardDetails
    {
        [JsonPropertyName("last4")]
        public string? Last4 { get; set; }

        [JsonPropertyName("issuing_country")]
        public string? IssuingCountry { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("bin")]
        public string? Bin { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }
}
