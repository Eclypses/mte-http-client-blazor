namespace Eclypses.MteHttpClient.Blazor.Models
{
    /// <summary>
    /// Return model from pairing with the Kyber relay server.
    /// </summary>
    public class MtePairResponseModel
    {
        /// <summary>
        /// The pair id that identifies this specific MTE pairing.
        /// </summary>
        public string? pairId { get; set; }
        /// <summary>
        /// The encrypted secret for the MteEncoder.
        /// </summary>
        public string? encoderSecret { get; set; }   
        /// <summary>
        /// The nonce value for the MteEncoder.
        /// </summary>
        public string? encoderNonce { get; set; }
        /// <summary>
        /// The encrypted secret for the MteDecoder.
        /// </summary>
        public string? decoderSecret { get; set; }    
        /// <summary>
        /// The nonce value for the MteDecoder.
        /// </summary>
        public string? decoderNonce { get; set; }
        /// <summary>
        /// Sets all of the string properties to null.
        /// </summary>
        public void Clear()
        {
            pairId = null;
            encoderNonce = null;
            encoderSecret = null;
            decoderNonce = null;
            decoderSecret = null;
        }
    }
}
