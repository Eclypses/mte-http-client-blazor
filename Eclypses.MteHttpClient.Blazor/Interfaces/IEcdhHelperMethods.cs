namespace MteHttpClient.Interfaces
{
    /// <summary>
    /// The contract for the ECDH Helper.  
    /// </summary>
    public interface IEcdhHelperMethods
    {
        /// <summary>
        /// The size of the public key for ECDH (64)
        /// </summary>
        int SzPublicKey { get; }
        /// <summary>
        /// The size of the private key for ECDH (32)
        /// </summary>
        int SzPrivateKey { get; }

        /// <summary>
        /// Computes a shared secret from the peer's public key
        /// and returns it in the secret byte array.
        /// </summary>
        /// <param name="peerPublicKey">The peer's (server's) ECDH public key for a specified pair.</param>
        /// <returns>Return code indicating success (0) and the byte[] of the actual secret.</returns>
        Task<(int rc, byte[] secret)> ComputeSharedSecretAsync(byte[] peerPublicKey);

        /// <summary>
        /// Instances up an ECDH object and offers up the public key so
        /// you can do an exchange with a peer.
        /// </summary>
        /// <returns>Return code indicating success (0) and the byte[] of the public key.</returns>
        Task<(int rc, byte[] publicKey)> GetPublicKeyFromECDHAsync();
    }
}
