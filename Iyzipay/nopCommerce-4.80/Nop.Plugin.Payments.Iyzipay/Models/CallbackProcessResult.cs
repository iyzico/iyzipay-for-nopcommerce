namespace Nop.Plugin.Payments.Iyzipay.Models
{
    public class CallbackProcessResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int OrderId { get; set; }
        public string TransactionId { get; set; }
    }
}
