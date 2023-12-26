
using Microsoft.JSInterop;
using MteHttpClient.Interfaces;
using MteHttpClient.Models;

namespace MteHttpClient
{
    /// <summary>
    /// Creates a EcdhHelper for managing
    /// the shared secret with the Mte-Relay.
    /// </summary>
    public class EcdhFactory : IEcdhFactory
    {
        /// <summary>
        /// The java script runtime interop object.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;
        /// <summary>
        /// The runtime options for this Mte-Relay.
        /// </summary>
        private readonly MteRelayOptions _mteRelayOptions;
        /// <summary>
        /// Constructs a new instance of the EcdhFactory
        /// </summary>
        /// <param name="jSRuntime"></param>
        public EcdhFactory(IJSRuntime jSRuntime, MteRelayOptions mteRelayOptions)
        {
            _jsRuntime = jSRuntime;
            _mteRelayOptions = mteRelayOptions;
        }
        /// <summary>
        /// Constructs a new blazor specific ECDH helper
        /// and returns it to the blazor app using the global
        /// java script runtime module.
        /// </summary>
        /// <returns>An instakce of the ECDHHelperMethodsBlazor.</returns>
        public IEcdhHelperMethods Create()
        {
            return new EcdhHelper(_jsRuntime, _mteRelayOptions);
        }
    }
}
