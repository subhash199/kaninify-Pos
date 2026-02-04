using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models.PaymentsModels.Card
{
    public class MakePaymentStateChange
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("tip")]
        public int Tip { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("gatewayPaymentId")]
        public string GatewayPaymentId { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentState State { get; set; }

        [JsonPropertyName("isFinal")]
        public bool IsFinal { get; set; }

        [JsonPropertyName("reason")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStateReason? Reason { get; set; }

        [JsonPropertyName("metadata")]
        public MetadataInfo Metadata { get; set; }

        public enum PaymentState
        {
            PENDING,
            NEW,
            IN_PROGRESS,
            CANCELLING,
            CANCELED,
            SUCCESSFUL,
            PROCESSING_FAILED,
            COMMUNICATION_FAILED
        }

        public enum PaymentStateReason
        {
            PROCESSING_FAILED_DECLINED_ONLINE,
            PROCESSING_FAILED_DECLINED_OFFLINE,
            PROCESSING_FAILED_TIMEOUT,
            PROCESSING_FAILED_CONNECTION_ERROR,
            PROCESSING_FAILED_COMM_TIMEOUT,
            PROCESSING_FAILED_CARD_PROCESSING_ERROR,
            UNKNOWN,
            CANCELED_BY_EPOS,
            CANCELED_BY_USER,
            EXPIRED,
            COMMUNICATION_FAILED_NETWORK,
            COMMUNICATION_FAILED_UNEXPECTEDLY,
            COMMUNICATION_FAILED_AUTH_REQUIRED
        }

        public class MetadataInfo
        {
            [JsonPropertyName("card")]
            public Card CardInfo { get; set; }

            [JsonPropertyName("entryMode")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public EntryModeInfo? EntryMode { get; set; }

            [JsonPropertyName("verificationMethod")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public VerificationMethodInfo? VerificationMethod { get; set; }

            [JsonPropertyName("applicationId")]
            public string ApplicationId { get; set; }

            [JsonPropertyName("merchantAcquiringId")]
            public string MerchantAcquiringId { get; set; }

            [JsonPropertyName("responseCode")]
            public string ResponseCode { get; set; }

            [JsonPropertyName("authorisationCode")]
            public string AuthorisationCode { get; set; }

            public class Card
            {
                [JsonPropertyName("last4")]
                public string Last4 { get; set; }

                [JsonPropertyName("issuingCountry")]
                public string IssuingCountry { get; set; }

                [JsonPropertyName("brand")]
                [JsonConverter(typeof(JsonStringEnumConverter))]
                public CardBrand Brand { get; set; }

                [JsonPropertyName("type")]
                [JsonConverter(typeof(JsonStringEnumConverter))]
                public CardType? Type { get; set; }
            }

            public enum EntryModeInfo
            {
                ECOM,
                EMV_PIN,
                CONTACT_ICC,
                EMV_CONTACTLESS,
                KEYED,
                MAGSTRIPE_SWIPED,
                ON_FILE,
                MAGSTRIPE_FALLBACK
            }

            public enum VerificationMethodInfo
            {
                NONE,
                ELECTRONIC_SIGNATURE,
                ON_DEVICE,
                MANUAL,
                SIGNATURE,
                OFFLINE_PIN,
                ONLINE_PIN,
                OFFLINE_PIN_PLUS_SIGNATURE,
                SECURED_ELECTRONIC_COMMERCE
            }

            public enum CardBrand
            {
                VISA,
                MAESTRO,
                MASTERCARD,
                UNION_PAY,
                JCB,
                DINERS,
                AMEX,
                OTHER
            }

            public enum CardType
            {
                DEBIT,
                CREDIT,
                PREPAID,
                CHARGE,
                UNKNOWN
            }
        }

        public override string ToString()
        {
            var metadataString = Metadata == null ? "null" :
                $"{{ CardInfo: {(Metadata.CardInfo == null ? "null" :
                    $"{{ Last4: {Metadata.CardInfo.Last4}, IssuingCountry: {Metadata.CardInfo.IssuingCountry}, " +
                    $"Brand: {Metadata.CardInfo.Brand}, Type: {Metadata.CardInfo.Type} }}")}, " +
                $"EntryMode: {Metadata.EntryMode}, VerificationMethod: {Metadata.VerificationMethod}, " +
                $"ApplicationId: {Metadata.ApplicationId}, MerchantAcquiringId: {Metadata.MerchantAcquiringId}, " +
                $"ResponseCode: {Metadata.ResponseCode}, AuthorisationCode: {Metadata.AuthorisationCode} }}";

            return $"MakePaymentStateChange {{ " +
                   $"TransactionId: {TransactionId}, " +
                   $"GatewayPaymentId: {GatewayPaymentId}, " +
                   $"State: {State}, " +
                   $"Amount: {Amount}, " +
                   $"Tip: {Tip}, " +
                   $"Currency: {Currency}, " +
                   $"IsFinal: {IsFinal}, " +
                   $"Reason: {Reason}, " +
                   $"Metadata: {metadataString} }}";
        }
    }
}
