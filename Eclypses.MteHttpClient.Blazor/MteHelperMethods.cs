
using Microsoft.JSInterop;
using MteHttpClient.Interfaces;
using MteHttpClient.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MteHttpClient
{
    public class MteHelperMethods : IMteHelperMethods
    {
        /// <summary>
        /// Property to show how many Mte engines are pooled.
        /// </summary>
        public int EstablishedMtePoolCount { get { return _mkeDecoderBag != null ? _mkeDecoderBag!.Count : -1; } }
        /// <summary>
        /// Collections of Encoders and Decoders that are reused with the paired MteStates.
        /// </summary>
        private static ConcurrentBag<IJSObjectReference> _mkeDecoderBag = new ConcurrentBag<IJSObjectReference>();
        private static ConcurrentBag<IJSObjectReference> _mkeEncoderBag = new ConcurrentBag<IJSObjectReference>();
        /// <summary>
        /// The dotNet java script interop runtime.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;
        /// <summary>
        /// The runtime MTE Relay options from appsettings.json
        /// </summary>
        private readonly MteRelayOptions _mteRelayOptions;
        /// <summary>
        /// The mte helpers java script module.
        /// </summary>
        private IJSObjectReference _mteHelpersJSModule;            
        /// <summary>
        /// This ensures that the SDR (for storing MteState) is only
        /// initialized once. 
        /// </summary>
        private static bool _sdrIsInitialized = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MteHelperMethods(IJSRuntime jsRuntime, MteRelayOptions mteRelayOptions)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _jsRuntime = jsRuntime;
            _mteRelayOptions = mteRelayOptions;
        }

        #region SetupRuntime
        /// <summary>
        /// Establishes the link to mte-helper.js and instiates the MteWasm
        /// module (the actual MTE). It then sets the license for this
        /// specific licensee and builds the initial pools of encoders and decoders.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task SetupRuntimeAsync()
        {
            try
            {
                await LoadMKEjavaScriptInterface();
                if (_mteHelpersJSModule is null)
                {
                    throw new ApplicationException("Could not initialize the mte-helper.js module.");
                }
                if (!await _mteHelpersJSModule!.InvokeAsync<bool>("instantiateMteWasm"))
                {
                    throw new ApplicationException("Could not create the MteWasm (mte.ts) module.");
                }
                await _mteHelpersJSModule.InvokeVoidAsync("initLicense", _mteRelayOptions.LicensedCompany, _mteRelayOptions.LicenseKey);

                if (_mkeDecoderBag.Count == 0)
                {
                    for (int i = 0; i < _mteRelayOptions.NumberOfCachedMtePairs; i++)
                    {
                        await AddAnEmptyDecoderAsync();
                        await AddAnEmptyEncoderAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not setup the java script interop runtime", ex);
            }
        }
        #endregion

        #region AddAnEmptyDecoderAsync
        /// <summary>
        /// Creates an empty MTE Decoder and adds it to
        /// the "bag" for reuse as needed.
        /// </summary>
        /// <returns>Completed Task</returns>
        public async Task AddAnEmptyDecoderAsync()
        {
            try
            {
                await LoadMKEjavaScriptInterface();
                //
                // Create a Decoder object and add it to the bag.
                //
                var decoder = await _mteHelpersJSModule.InvokeAsync<IJSObjectReference>("makeAnEmptyDecoder");
                _mkeDecoderBag.Add(decoder);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create and add an MTE Decoder to the internal collection.",ex);
            }
        }
        #endregion

        #region AddAnEmptyEncoderAsync
        /// <summary>
        /// Creates an empty MTE Encoder and adds it to
        /// the "bag" for reuse as needed.
        /// </summary>
        /// <returns>Completed Task</returns>
        public async Task AddAnEmptyEncoderAsync()
        {
            try
            {
                await LoadMKEjavaScriptInterface();
                //
                // Create an Encoder object and add it to the bag.
                //
                var encoder = await _mteHelpersJSModule.InvokeAsync<IJSObjectReference>("makeAnEmptyEncoder");
                _mkeEncoderBag.Add(encoder);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create and add an MTE Encoder to the internal collection.", ex);
            }
        }
        #endregion

        #region GetAnEncoderAsync
        /// <summary>
        /// Returns an MTE Encoder from the bag, if
        /// none are available, it adds one.
        /// </summary>
        /// <returns>JS Object reference to an MTE decoder.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<IJSObjectReference> GetAnEncoderAsync()
        {
            try
            {
                if (_mkeEncoderBag.IsEmpty)
                {
                    await AddAnEmptyEncoderAsync();
                }
                if (_mkeEncoderBag.TryTake(out IJSObjectReference? encoder))
                {
                    return encoder;
                }
                throw new ApplicationException("Could not retrieve an encoder from the collection.");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region GetADecoderAsync
        /// <summary>
        /// Returns an MTE Decoder from the bag, if
        /// none are available, it adds one.
        /// </summary>
        /// <returns>JS Object reference to an MTE decoder.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<IJSObjectReference> GetADecoderAsync()
        {
            try
            {
                if (_mkeDecoderBag.IsEmpty)
                {
                    await AddAnEmptyDecoderAsync();
                }
                if (_mkeDecoderBag.TryTake(out IJSObjectReference? decoder))
                {
                    return decoder;
                }
                throw new ApplicationException("Could not retrieve a decoder from the collection.");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region PutTheEncoderAsync
        /// <summary>
        /// Puts an decoder into the "bag" for later reuse.
        /// </summary>
        /// <param name="encoder">The JS Object reference to an decoder.</param>
        /// <returns>Completed Task</returns>
        public void PutTheEncoderAsync(IJSObjectReference encoder)
        {
            _mkeEncoderBag.Add(encoder);
        }
        #endregion

        #region PutTheDecoderAsync
        /// <summary>
        /// Puts a decoder into the "bag" for later reuse.
        /// </summary>
        /// <param name="decoder">The JS Object reference to a decoder.</param>
        /// <returns>Completed Task</returns>
        public void PutTheDecoderAsync(IJSObjectReference decoder)
        {
            _mkeDecoderBag.Add(decoder);
        }
        #endregion

        #region InitializeDecoderAsync
        /// <summary>
        /// Initializes a decoder with the nonce, entropy, and personalization string.
        /// </summary>
        /// <param name="mkeDecoder">The decoder to initialize.</param>
        /// <param name="magicValues">Object with the three initialization values.</param>
        /// <returns>MteStatus</returns>
        public async Task<MteStatus> InitializeDecoderAsync(IJSObjectReference mkeDecoder, MteMagicValues magicValues)
        {
            try
            {
                await LoadMKEjavaScriptInterface();
                //
                // Apply the three magic values and instantiate the decoder.
                //
                MteStatus status = await _mteHelpersJSModule.InvokeAsync<MteStatus>("initializeDecoder", mkeDecoder, magicValues.Entropy, magicValues.Nonce, magicValues.PersonalizationString);
                return status;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region InitializeEncoderAsync
        /// <summary>
        /// Initializes an decoder with the nonce, entropy, and personalization string.
        /// </summary>
        /// <param name="mkeEncoder">The decoder to initialize.</param>
        /// <param name="magicValues">Object with the three initialization values.</param>
        /// <returns>MteStatus</returns>
        public async Task<MteStatus> InitializeEncoderAsync(IJSObjectReference mkeEncoder, MteMagicValues magicValues)
        {
            try
            {
                await LoadMKEjavaScriptInterface();
                //
                // Apply the three magic values and instantiate the decoder.
                //
                MteStatus status = await _mteHelpersJSModule.InvokeAsync<MteStatus>("initializeEncoder", mkeEncoder, magicValues.Entropy, magicValues.Nonce, magicValues.PersonalizationString);
                return status;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region RestoreDecoderStateAsync
        /// <summary>
        /// Restores (sets) the state of a decoder with the B-64 representation of an MteState.
        /// </summary>
        /// <param name="mkeDecoder">The decoder to restore.</param>
        /// <param name="state">Base-64 representation of the state.</param>
        /// <returns>MteStatus</returns>
        public async Task<MteStatus> RestoreDecoderStateAsync(IJSObjectReference mkeDecoder, string state)
        {
            await LoadMKEjavaScriptInterface();
            return await _mteHelpersJSModule.InvokeAsync<MteStatus>("restoreDecoderState", mkeDecoder, state);
        }
        #endregion

        #region RestoreEncoderStateAsync
        /// <summary>
        /// Restores (sets) the state of an decoder with the B-64 representation of an MteState.
        /// </summary>
        /// <param name="mkeEncoder">The decoder to restore.</param>
        /// <param name="state">Base-64 representation of the state.</param>
        /// <returns>MteStatus</returns>
        public async Task<MteStatus> RestoreEncoderStateAsync(IJSObjectReference mkeEncoder, string state)
        {
            await LoadMKEjavaScriptInterface();
            return await _mteHelpersJSModule.InvokeAsync<MteStatus>("restoreEncoderState", mkeEncoder, state);
        }
        #endregion

        #region RetrieveDecoderStateAsync
        /// <summary>
        /// Retrieves (gets) the state of a decoder and returns it as a B-64 string.
        /// </summary>
        /// <param name="mkeDecoder">The decoder that you wish to work with.</param>
        /// <returns>Base-64 version of the current state.</returns>
        public async Task<string> RetrieveDecoderStateAsync(IJSObjectReference mkeDecoder)
        {
            await LoadMKEjavaScriptInterface();
            var theState = await _mteHelpersJSModule.InvokeAsync<string>("retrieveDecoderState", mkeDecoder);
            return theState;
        }
        #endregion

        #region RetrieveEncoderStateAsync
        /// <summary>
        /// Retrieves (gets) the state of an decoder and returns it as a B-64 string.
        /// </summary>
        /// <param name="mkeEncoder">The decoder that you wish to work with.</param>
        /// <returns>Base-64 version of the current state.</returns>
        public async Task<string> RetrieveEncoderStateAsync(IJSObjectReference mkeEncoder)
        {
            await LoadMKEjavaScriptInterface();
            var theState = await _mteHelpersJSModule.InvokeAsync<string>("retrieveEncoderState", mkeEncoder);
            return theState;
        }
        #endregion

        #region DecodeToStringAsync
        /// <summary>
        /// Decodes a B-64 string of an encoded payload.
        /// </summary>
        /// <param name="mkeDecoder">The decoder to use (must match the partner's encoder).</param>
        /// <param name="payload">Base-64 version of an encoded payload.</param>
        /// <returns>The decoded string.</returns>
        public async Task<string> DecodeToStringAsync(IJSObjectReference mkeDecoder, string payload)
        {
            await LoadMKEjavaScriptInterface();
            return await _mteHelpersJSModule.InvokeAsync<string>("decodeToString", mkeDecoder, payload);
        }
        #endregion

        #region DecodeToByteArrayAsync
        /// <summary>
        /// Decodes a byte array of an encoded payload.
        /// </summary>
        /// <param name="mkeDecoder">The decoder to use (must match the partner's encoder).</param>
        /// <param name="payload">Byte array containing an encoded payload.</param>
        /// <returns>The decoded byte array.</returns>
        public async Task<byte[]> DecodeToByteArrayAsync(IJSObjectReference mkeDecoder, byte[] payload)
        {
            await LoadMKEjavaScriptInterface();
            return await _mteHelpersJSModule.InvokeAsync<byte[]>("decodeToByteArray", mkeDecoder, payload);
        }
        #endregion

        #region EncodeToStringAsync
        /// <summary>
        /// Encodes a string returning a B-64 version of the encoded data.
        /// </summary>
        /// <param name="mkeEncoder">The encoder to use (must match the partner's decoder).</param>
        /// <param name="payload">A string to encode.</param>
        /// <returns>Base-64 version of the encoded result.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string> EncodeToStringAsync(IJSObjectReference mkeEncoder, string payload)
        {
            await LoadMKEjavaScriptInterface();
            JsonElement returnValue = await _mteHelpersJSModule.InvokeAsync<JsonElement>("encodeToString", mkeEncoder, payload);            
            var status = returnValue.GetProperty("status").GetInt32();
            if (status != (int)MteStatus.mte_status_success)
            {
                throw new ApplicationException($"Error encoding string for Mte-Relay: {status}");
            }
            string? str = returnValue.GetProperty("str").GetString();
            return str!;
        }
        #endregion

        #region EncodeAsync - byte[]
        /// <summary>
        /// Encodes a byte array returning an array of the encoded bytes.
        /// </summary>
        /// <param name="mkeEncoder">The encoder to use (must match the partner's decoder).</param>
        /// <param name="payload">A byte array to encode.</param>
        /// <returns>The encoded result.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<byte[]> EncodeAsync(IJSObjectReference mkeEncoder, byte[] payload)
        {
            await LoadMKEjavaScriptInterface();
            var returnValue = await _mteHelpersJSModule.InvokeAsync<JsonElement>("encode", mkeEncoder, payload);
            
            var status = returnValue.GetProperty("status").GetInt32();
            if (status != (int)MteStatus.mte_status_success)
            {
                throw new ApplicationException($"Error encoding byte[] for Mte-Relay: {status}");
            }
            string? str = returnValue.GetProperty("str").GetString();
            byte[] arr = Convert.FromBase64String(str!);
            return arr;
        }
        #endregion

        #region EncodeAsync - string
        /// <summary>
        /// Encodes a string returning an array of the encoded bytes.
        /// </summary>
        /// <param name="mkeEncoder">The encoder to use (must match the partner's decoder).</param>
        /// <param name="payload">A string to encode.</param>
        /// <returns>The encoded result.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<byte[]> EncodeAsync(IJSObjectReference mkeEncoder, string payload)
        {
            await LoadMKEjavaScriptInterface();
            var returnValue = await _mteHelpersJSModule.InvokeAsync<JsonElement>("encode", mkeEncoder, payload);
            var status = returnValue.GetProperty("status").GetInt32();
            if (status != (int)MteStatus.mte_status_success)
            {
                throw new ApplicationException($"Error encoding string to byte[] for Mte-Relay: {status}");
            }
            string? str = returnValue.GetProperty("str").GetString();            
            byte[] arr = Convert.FromBase64String(str!);           
            return arr;
        }
        #endregion

        #region InitializeSDRAsync
        /// <summary>
        /// Initializes the SDR for storing MTE states.
        /// </summary>
        /// <returns>Completed task.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task InitializeSDRAsync()
        {
            try
            {
                //
                // Make sure we only initialize once.
                //
                if (_sdrIsInitialized)
                {
                    return;
                }
                await LoadMKEjavaScriptInterface();
                byte[] sessionStorageEntropy = Convert.FromBase64String(await _mteHelpersJSModule.InvokeAsync<string>("getEntropy", 32));
                string sessionStorageNonce = DateTime.Now.ToString("yyMMddHHmmssffff");
                await _mteHelpersJSModule!.InvokeVoidAsync("initializeSessionSdr", Constants.DISPLAY_SESSION_ITEM_FILE, sessionStorageEntropy, sessionStorageNonce);
                _sdrIsInitialized = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error initializing the Java Script for the SDR.", ex);
            }
        }
        #endregion InitializeSDRAsync

        #region RetrieveFromSDRAsync
        /// <summary>
        /// Retrieves an item from SDR (secure session storage).
        /// </summary>
        /// <param name="name">The name (key of the data).</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>System.String.</returns>
        public async Task<string> RetrieveFromSDRAsync(string name, bool persistent = false)
        {
            try
            {
                if (_sdrIsInitialized)
                {
                    return await _mteHelpersJSModule.InvokeAsync<string>("read", name, persistent);
                }
                else
                {
                    throw new ApplicationException("The Eclypses SDR has not been initialized");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not retrieve value from the SDR", ex);
            }
        }
        #endregion

        #region StoreInSDRAsync
        /// <summary>
        /// Stores an item into SDR (secure session storage).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Completed task.</returns>
        public async Task StoreInSDRAsync(string name, string data, bool persistent = false)
        {
            try
            {
                if (_sdrIsInitialized)
                {
                    await _mteHelpersJSModule.InvokeVoidAsync("write", name, data, persistent);
                }
                else
                {
                    throw new ApplicationException("The Eclypses SDR has not been initialized");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not store value into the SDR", ex);
            }
        }
        #endregion

        #region public RemoveFromSDRAsync
        /// <summary>
        /// Removes an item from the Eclypses SDR (secure session storage).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Completed task.</returns>
        public async Task RemoveFromSDRAsync(string name, bool persistent = false)
        {
            try
            {
                if (_sdrIsInitialized)
                {
                    await _mteHelpersJSModule.InvokeVoidAsync("remove", name, persistent);
                }
                else
                {
                    throw new ApplicationException("The Eclypses SDR has not been initialized");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not remove value from the SDR", ex);
            }
        } 
        #endregion

        #region private LoadMKEjavaScriptInterface
        /// <summary>
        /// Imports the mte-helper.js script if it is not already imported.
        /// </summary>
        /// <returns></returns>
        private async Task LoadMKEjavaScriptInterface()
        {
            if (_mteHelpersJSModule is null)
            {
                string jsPath = Constants.NUGET_CONTENT_ROOT;
                if (_mteRelayOptions.IsTestingLocal)
                {
                    jsPath = Constants.LOCAL_TEST_CONTENT_ROOT;
                }
                _mteHelpersJSModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", $"{jsPath.TrimEnd('/')}/mterelay-helper.js");
            }
        }
        #endregion
    }
}
