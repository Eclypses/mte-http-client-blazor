namespace MteHttpClient.Models
{
    /// <summary>
    /// This is the exchange model with the MteRelay
    /// proxy server when pairing is required.
    /// </summary>
    internal class MtePairRequestModel
    {
        /// <summary>
        /// The pair id that identifies this specific MTE pairing.
        /// </summary>
        public string? pairId { get; set; }
        /// <summary>
        /// The public key for the MteEncoder.
        /// </summary>
        public string? encoderPublicKey { get; set; }
        /// <summary>
        /// The personalization string for the MteEncoder.
        /// </summary>
        public string? encoderPersonalizationStr { get; set; }    
        /// <summary>
        /// The public key for the MteDecoder.
        /// </summary>
        public string? decoderPublicKey { get; set; }
        /// <summary>
        /// The personalization string for the MteDecoder.
        /// </summary>
        public string? decoderPersonalizationStr { get; set; }

        /// <summary>
        /// Sets all of the string properties to null.
        /// </summary>
        public void Clear()
        {
            pairId = null;
            encoderPublicKey = null;
            encoderPersonalizationStr = null;
            decoderPublicKey = null;
            decoderPersonalizationStr = null;
        }
    }
}
