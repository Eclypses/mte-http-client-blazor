using Microsoft.JSInterop;
using MteHttpClient.Interfaces;
using MteHttpClient.Models;

namespace MteHttpClient
{
    /// <summary>
    /// Concrete implementation of the methods
    /// required to interact with the 
    /// Elliptical Curve Diffie-Hellman java script
    /// modules.
    /// </summary>
    internal class EcdhHelper : IEcdhHelperMethods
    {
        /// <summary>
        /// The size of the public key for ECDH (64)
        /// </summary>
        public int SzPublicKey { get; } = 64;
        /// <summary>
        /// The size of the private key for ECDH (32)
        /// </summary>
        public int SzPrivateKey { get; } = 32;
        /// <summary>
        /// The dotNet java script runtime bridge.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;
        /// <summary>
        /// The runtime options for the Mte-Relay Proxy server.
        /// </summary>
        private readonly MteRelayOptions _mteRelayOptions;
        /// <summary>
        /// A property to hold the ECDH derived keys
        /// in isolation for each paired MTE.
        /// </summary>
        private EcdhKeys? _ecdhKeys { get; set; }
        /// <summary>
        /// Instances up a EcdhHelper object.
        /// </summary>
        /// <param name="jSRuntime"></param>
        public EcdhHelper(IJSRuntime jSRuntime, MteRelayOptions mteRelayOptions)
        {
            _jsRuntime = jSRuntime;
            _mteRelayOptions = mteRelayOptions;
        }
        #region GetPublicKeyFromECDH
        /// <summary>
        /// Creates the key pair, stores it in a private property
        /// and returns the public key to be sent to the server
        /// for generating entropy.
        /// </summary>
        /// <returns>rc=0 for success and the specific public key as a tuple.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<(int rc, byte[] publicKey)> GetPublicKeyFromECDHAsync()
        {
            string jsPath = Constants.NUGET_CONTENT_ROOT;
            if (_mteRelayOptions.IsTestingLocal)
            {
                jsPath = Constants.LOCAL_TEST_CONTENT_ROOT;
            }
            await using (IJSObjectReference ecdhJS = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", $"{jsPath.TrimEnd('/')}/mterelay-helper.js"))
            {
                try
                {
                    //
                    // Call the 'makeKeyPair' method from the ecdh-helper.js module
                    // which returns a base 64 string of the private and public keys.
                    //
                    _ecdhKeys = await ecdhJS.InvokeAsync<EcdhKeys>("makeKeyPair");
                    return (0, Convert.FromBase64String(_ecdhKeys.publicKey));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Exception calling java script 'makeKeyPair' to create a key pair.", ex);
                }
            }
        }
        #endregion

        #region ComputeSharedSecretBlazor
        /// <summary>
        /// Uses this instance's private key, and the peer's public key
        /// to generate a secret for MTE to use as entropy.
        /// </summary>
        /// <param name="peerPublicKey">The public key of the instance we wish to pair with.</param>
        /// <returns>rc=0 for success and the specific secret as a tuple.</returns>
        public async Task<(int rc, byte[] secret)> ComputeSharedSecretAsync(byte[] peerPublicKey)
        {
            string jsPath = Constants.NUGET_CONTENT_ROOT;
            if (_mteRelayOptions.IsTestingLocal)
            {
                jsPath = Constants.LOCAL_TEST_CONTENT_ROOT;
            }
            await using (IJSObjectReference ecdhJS = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", $"{jsPath.TrimEnd('/')}/mterelay-helper.js"))
            {
                try
                {
                    //
                    // Call the 'makeSecret' method from the ecdh-helper.js module
                    // which returns a base 64 string of the computed secret.
                    //
                    string secret = await ecdhJS.InvokeAsync<string>("makeSecret", peerPublicKey, _ecdhKeys!.privateKey);
                    return (0, Convert.FromBase64String(secret));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Exception calling java script 'makeSecret' to create a shared secret.", ex);
                }
            }
        }
        #endregion
    }
}
