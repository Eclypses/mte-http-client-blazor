namespace MteHttpClient.Models
{
    /// <summary>
    /// Contains the keys for an ECDH handshake.
    /// </summary>
    internal class EcdhKeys
    {
        /// <summary>
        /// Base-64 representation of the public key.
        /// </summary>
        public string publicKey { get; set; } = string.Empty;
        /// <summary>
        /// Base-64 representation of the private key.
        /// </summary>
        public string privateKey { get; set; } = string.Empty;
        /// <summary>
        /// Return code from the actual java script calls into ECDH.
        /// </summary>
        public int rc { get; set; } = 0;
    }
}
