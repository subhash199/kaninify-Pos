using System;
using System.Text.Json.Serialization;

namespace EposRetail.Models
{
    public sealed class TeyaPartnerCredentials
    {
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
    }

    public sealed class TeyaDeviceAuthorizationResponse
    {
        [JsonPropertyName("user_code")]
        public string UserCode { get; set; } = string.Empty;

        [JsonPropertyName("verification_url")]
        public string VerificationUrl { get; set; } = string.Empty;

        [JsonPropertyName("verification_url_complete")]
        public string VerificationUrlComplete { get; set; } = string.Empty;

        [JsonPropertyName("device_code")]
        public string DeviceCode { get; set; } = string.Empty;

        [JsonPropertyName("qr_code")]
        public string QrCode { get; set; } = string.Empty;

        [JsonPropertyName("interval")]
        public int IntervalSeconds { get; set; } = 5;

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }
    }

    public sealed class TeyaTokenResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope {get; set;} = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }
    }

    public sealed class TeyaOAuthErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }

    public sealed class TeyaStoresResponse
    {
        [JsonPropertyName("stores")]
        public List<TeyaStoreSummary> Stores { get; set; } = new();
    }

    public sealed class TeyaStoreSummary
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public TeyaStoreAddress? Address { get; set; }
    }

    public sealed class TeyaStoreAddress
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("street_address_line_1")]
        public string? StreetAddressLine1 { get; set; }

        [JsonPropertyName("street_address_line_2")]
        public string? StreetAddressLine2 { get; set; }

        [JsonPropertyName("zipcode")]
        public string? ZipCode { get; set; }
    }

    public sealed class TeyaTerminalsResponse
    {
        [JsonPropertyName("terminals")]
        public List<TeyaTerminalSummary> Terminals { get; set; } = new();
    }

    public sealed class TeyaTerminalSummary
    {
        [JsonPropertyName("terminal_id")]
        public string TerminalId { get; set; } = string.Empty;

        [JsonPropertyName("terminal_name")]
        public string? TerminalName { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }
    }

    public sealed class TeyaStoreConfigResponse
    {
        [JsonPropertyName("config_key")]
        public string ConfigKey { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class TeyaTerminalConfigsResponse
    {
        [JsonPropertyName("configs")]
        public List<TeyaStoreConfigResponse> Configs { get; set; } = new();
    }

    public sealed class TeyaRequestedAmount
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("tip")]
        public int Tip { get; set; }
    }

    public sealed class TeyaPaymentRequestResponse
    {
        [JsonPropertyName("payment_request_id")]
        public string PaymentRequestId { get; set; } = string.Empty;

        [JsonPropertyName("requested_amount")]
        public TeyaRequestedAmount? RequestedAmount { get; set; }

         [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("transaction_type")]
        public string? TransactionType { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

         [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonPropertyName("merchant_reference")]
        public string? MerchantReference { get; set; }

         [JsonPropertyName("status_reason")]
        public string? StatusReason { get; set; }

        [JsonPropertyName("epos_instance_id")]
        public string? EposInstanceId { get; set; }

        [JsonPropertyName("gateway_payment_id")]
        public string? GatewayPaymentId { get; set; }

        [JsonPropertyName("store_id")]
        public string? StoreId { get; set; }

        [JsonPropertyName("terminal_id")]
        public string? TerminalId { get; set; }

        [JsonPropertyName("progress_status")]
        public string? ProgressStatus { get; set; }

        [JsonPropertyName("metadata")]
        public string? MetaData { get; set; }

        [JsonPropertyName("transaction_timestamp")]
        public DateTime? TransactionTimeStamp { get; set; }

        [JsonPropertyName("tab_id")]
        public string? TabId { get; set; }

         [JsonPropertyName("payment_type")]
        public string? PaymentType { get; set; }

         [JsonPropertyName("payment_method")]
        public string? PaymentMethod { get; set; }

    }

    public sealed class TeyaPaymentRequestsResponse
    {
        [JsonPropertyName("payment_requests")]
        public List<TeyaPaymentRequestResponse> PaymentRequests { get; set; } = new();
    }

    public sealed class TeyaPaymentStatusUpdateRequest
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "CANCELLING";
    }

    public sealed class TeyaPaymentProcessingResult
    {
        public bool IsConfigured { get; init; }
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? PaymentRequestId { get; init; }
        public string? GatewayPaymentId { get; init; }
        public string? FinalStatus { get; init; }
        public string? StatusReason { get; init; }
    }
}
