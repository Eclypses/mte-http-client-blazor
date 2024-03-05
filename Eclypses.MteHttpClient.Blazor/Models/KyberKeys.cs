namespace Eclypses.MteHttpClient.Blazor.Models
{
    /// <summary>
    /// Public keys returned from java script Kkyber.
    /// </summary>
    public class KyberKeys
    {
        /// <summary>
        /// The kyber encoder public key.
        /// </summary>
        public string? encoderPublicKey { get; set; }
        /// <summary>
        /// The kyber decoder public key.
        /// </summary>
        public string? decoderPublicKey { get; set; }
        /// <summary>
        /// The kyber encoder return code.
        /// </summary>
        public int eRC { get; set; }
        /// <summary>
        /// The kyber decoder return code.
        /// </summary>
        public int dRC { get; set; }
    }
}
