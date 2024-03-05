namespace MteHttpClient.Models
{
    public enum RelayHeaderDisposition
    {
        /// <summary>
        /// Do not encode any headers on the request.
        /// </summary>
        EncodeNoHeaders,
        /// <summary>
        /// Encode all of the headers on the request.
        /// </summary>
        EncodeAllHeaders,
        /// <summary>
        /// Encode only a list of specific headers on the request.
        /// </summary>
        EncodeListOfHeaders
    }

    /// <summary>
    /// Options to affect behavior of the Mte-Relay client.
    /// </summary>
    public class MteRelayOptions
    {
        /// <summary>
        /// If you have configured multiple endpoints that your app communicates with
        /// they are kept in this list.  There will always be at least one.
        /// </summary>
        public List<MteRelayEndpoint>? Endpoints { get; set; } = new List<MteRelayEndpoint>();
        /// <summary>
        /// When this is false, the mterelay-helper.js is loaded from NuGet,
        /// otherwise it is a project reference - used when we need to test the 
        /// MteHttpClient (NuGet does not give this the chance).
        /// </summary>
        public bool IsTestingLocal { get; set; } = false;
        /// <summary>
        /// The company that your MTE is licensed to.
        /// </summary>
        public string? LicensedCompany { get; set; }
        /// <summary>
        /// The MTE license key assigned to you.
        /// </summary>
        public string? LicenseKey { get; set; }
        /// <summary>
        /// The number of MTE Pairs that you wish to work with.
        /// More pairs increases throughput and aids with
        /// asynchronous requests. This is generally higher
        /// than the number of Mte Instances created. It's
        /// default is set in the Constants object.
        /// </summary>
        public int NumberOfConcurrentMteStates { get; set; }
        /// <summary>
        /// The number of MTE pairs to create at initial pairing time.
        /// These are re-hydrated with the "pairId" states before
        /// use. They are pooled and available for concurrent requests.
        /// If they are all "Busy", then a new one is added to the pool.
        /// </summary>
        public int NumberOfCachedMtePairs { get; set; }
        /// <summary>
        /// If true, the destinaiton Url (Route) is encoded.
        /// The default is false.
        /// </summary>
        public bool ShouldEncodeUrl { get; set; } = false;
        /// <summary>
        /// If true, the body (content) is encoded.
        /// The default is true.
        /// </summary>
        public bool ShouldEncodeBody { get; set; } = true;
        /// <summary>
        /// Based on this enum, specific headers will be encoded
        /// into the x-mte-relay-eh header (Note Content-type is
        /// always included).
        /// </summary>
        public RelayHeaderDisposition HeaderDisposition { get; set; }
        /// <summary>
        /// If HeaderDisposition is EncodeListOfHeaders, then this is the list that will be encoded.
        /// NOTE: the Content-Type header is always encoded.
        /// </summary>
        public List<string> HeadersToEncode { get; set; } = new List<string>();
    }
}
