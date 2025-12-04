using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nop.Plugin.Payments.Iyzipay.Models
{
    public class CallbackResponseModel
    {
        [JsonPropertyName("Token")]
        public string Token { get; set; }

        [JsonPropertyName("CallbackUrl")]
        public string CallbackUrl { get; set; }

        [JsonPropertyName("Price")]
        public string Price { get; set; }

        [JsonPropertyName("PaidPrice")]
        public string PaidPrice { get; set; }

        [JsonPropertyName("Installment")]
        public int Installment { get; set; }

        [JsonPropertyName("Currency")]
        public string Currency { get; set; }

        [JsonPropertyName("PaymentId")]
        public string PaymentId { get; set; }

        [JsonPropertyName("Signature")]
        public string Signature { get; set; }

        [JsonPropertyName("PaymentStatus")]
        public string PaymentStatus { get; set; }

        [JsonPropertyName("FraudStatus")]
        public int FraudStatus { get; set; }

        [JsonPropertyName("MerchantCommissionRate")]
        public string MerchantCommissionRate { get; set; }

        [JsonPropertyName("MerchantCommissionRateAmount")]
        public string MerchantCommissionRateAmount { get; set; }

        [JsonPropertyName("IyziCommissionRateAmount")]
        public string IyziCommissionRateAmount { get; set; }

        [JsonPropertyName("IyziCommissionFee")]
        public string IyziCommissionFee { get; set; }

        [JsonPropertyName("CardType")]
        public string CardType { get; set; }

        [JsonPropertyName("CardAssociation")]
        public string CardAssociation { get; set; }

        [JsonPropertyName("CardFamily")]
        public string CardFamily { get; set; }

        [JsonPropertyName("CardToken")]
        public string CardToken { get; set; }

        [JsonPropertyName("CardUserKey")]
        public string CardUserKey { get; set; }

        [JsonPropertyName("BinNumber")]
        public string BinNumber { get; set; }

        [JsonPropertyName("LastFourDigits")]
        public string LastFourDigits { get; set; }

        [JsonPropertyName("BasketId")]
        public string BasketId { get; set; }

        [JsonPropertyName("itemTransactions")]
        public List<ItemTransaction> ItemTransactions { get; set; }

        [JsonPropertyName("ConnectorName")]
        public string ConnectorName { get; set; }

        [JsonPropertyName("AuthCode")]
        public string AuthCode { get; set; }

        [JsonPropertyName("HostReference")]
        public string HostReference { get; set; }

        [JsonPropertyName("Phase")]
        public string Phase { get; set; }

        [JsonPropertyName("MdStatus")]
        public string MdStatus { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("StatusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("ErrorGroup")]
        public string ErrorGroup { get; set; }

        [JsonPropertyName("ConversationId")]
        public string ConversationId { get; set; }

        [JsonPropertyName("SystemTime")]
        public long SystemTime { get; set; }

        [JsonPropertyName("Locale")]
        public string Locale { get; set; }
    }

    public class ItemTransaction
    {
        [JsonPropertyName("ItemId")]
        public string ItemId { get; set; }

        [JsonPropertyName("PaymentTransactionId")]
        public string PaymentTransactionId { get; set; }

        [JsonPropertyName("TransactionStatus")]
        public int TransactionStatus { get; set; }

        [JsonPropertyName("Price")]
        public string Price { get; set; }

        [JsonPropertyName("PaidPrice")]
        public string PaidPrice { get; set; }

        [JsonPropertyName("MerchantCommissionRate")]
        public string MerchantCommissionRate { get; set; }

        [JsonPropertyName("MerchantCommissionRateAmount")]
        public string MerchantCommissionRateAmount { get; set; }

        [JsonPropertyName("IyziCommissionRateAmount")]
        public string IyziCommissionRateAmount { get; set; }

        [JsonPropertyName("IyziCommissionFee")]
        public string IyziCommissionFee { get; set; }

        [JsonPropertyName("BlockageRate")]
        public string BlockageRate { get; set; }

        [JsonPropertyName("BlockageRateAmountMerchant")]
        public string BlockageRateAmountMerchant { get; set; }

        [JsonPropertyName("BlockageRateAmountSubMerchant")]
        public string BlockageRateAmountSubMerchant { get; set; }

        [JsonPropertyName("BlockageResolvedDate")]
        public string BlockageResolvedDate { get; set; }

        [JsonPropertyName("SubMerchantKey")]
        public string SubMerchantKey { get; set; }

        [JsonPropertyName("SubMerchantPrice")]
        public string SubMerchantPrice { get; set; }

        [JsonPropertyName("SubMerchantPayoutRate")]
        public string SubMerchantPayoutRate { get; set; }

        [JsonPropertyName("SubMerchantPayoutAmount")]
        public string SubMerchantPayoutAmount { get; set; }

        [JsonPropertyName("MerchantPayoutAmount")]
        public string MerchantPayoutAmount { get; set; }

        [JsonPropertyName("ConvertedPayout")]
        public ConvertedPayout ConvertedPayout { get; set; }

        [JsonPropertyName("WithholdingTax")]
        public string WithholdingTax { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("StatusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("ErrorGroup")]
        public string ErrorGroup { get; set; }

        [JsonPropertyName("ConversationId")]
        public string ConversationId { get; set; }

        [JsonPropertyName("SystemTime")]
        public long SystemTime { get; set; }

        [JsonPropertyName("Locale")]
        public string Locale { get; set; }
    }

    public class ConvertedPayout
    {
        [JsonPropertyName("PaidPrice")]
        public string PaidPrice { get; set; }

        [JsonPropertyName("IyziCommissionRateAmount")]
        public string IyziCommissionRateAmount { get; set; }

        [JsonPropertyName("IyziCommissionFee")]
        public string IyziCommissionFee { get; set; }

        [JsonPropertyName("BlockageRateAmountMerchant")]
        public string BlockageRateAmountMerchant { get; set; }

        [JsonPropertyName("BlockageRateAmountSubMerchant")]
        public string BlockageRateAmountSubMerchant { get; set; }

        [JsonPropertyName("SubMerchantPayoutAmount")]
        public string SubMerchantPayoutAmount { get; set; }

        [JsonPropertyName("MerchantPayoutAmount")]
        public string MerchantPayoutAmount { get; set; }

        [JsonPropertyName("IyziConversionRate")]
        public string IyziConversionRate { get; set; }

        [JsonPropertyName("IyziConversionRateAmount")]
        public string IyziConversionRateAmount { get; set; }

        [JsonPropertyName("Currency")]
        public string Currency { get; set; }
    }
}
