namespace Eclypses.MteHttpClient.Blazor.Models
{
    /// <summary>
    /// The Kyber generated shared secrets.
    /// </summary>
    public class KyberSecrets
    {
        /// <summary>
        /// The shared secret for the encoder.
        /// </summary>
        public string? encoderSecret { get; set; }
        /// <summary>
        /// The shared secret for the decoder.
        /// </summary>
        public string? decoderSecret { get; set; }
        /// <summary>
        /// The shared secret return code for the encoder.
        /// </summary>
        public int eRC { get; set; }
        /// <summary>
        /// The shared secret return code for the decoder.
        /// </summary>
        public int dRC { get; set; }
        /// <summary>
        /// Resets this object.
        /// </summary>
        public void Clear()
        {
            encoderSecret = null;
            decoderSecret = null;
            eRC = 0;
            dRC = 0;
        }
    }
}
