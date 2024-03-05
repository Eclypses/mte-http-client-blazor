using System.Text;

namespace Eclypses.MteHttpClient.Blazor.Models
{
    /// <summary>
    /// Determines if we are using MTE or MKE.
    /// </summary>
    public enum EncodeType
    {
        MTE = 0,
        MKE = 1
    }

    /// <summary>
    /// The x-mte-relay header is a comma separated list
    /// of relay parameters - this class manages it.
    /// </summary>
    public class X_MteRelay_Header
    {
        /// <summary>
        /// The clientId is returned from the Mte-Pair route.
        /// </summary>
        public string? clientId { get; set; }
        /// <summary>
        /// Unique identifier for the Encoder / Decoder pair
        /// that is pooled for performance.
        /// </summary>
        public string? pairId { get; set; }
        /// <summary>
        /// Determines the Eclypses encoding type.
        /// </summary>
        public EncodeType encodeType { get; set; } = EncodeType.MKE;
        /// <summary>
        /// If true, the eventual endpoint url is encoded.
        /// </summary>
        public bool urlIsEncoded { get; set; }
        /// <summary>
        /// If true, there is an encoded header to send to the relay.
        /// </summary>
        public bool headersAreEncoded { get; set; }
        /// <summary>
        /// If true, the body of the request is coded.  This must be
        /// false for a GET request.
        /// </summary>
        public bool bodyIsEncoded { get; set; }
        public X_MteRelay_Header() { }
        /// <summary>
        /// Deconstructs a x-mte-relay header into this class.
        /// </summary>
        /// <param name="mteRelayHeader">The x-mte-relay header value.</param>
        public X_MteRelay_Header(string mteRelayHeader)
        {
            string[] s = mteRelayHeader.Split(',');
            if (s.Length > 0) { clientId = s[0]; }
            if (s.Length > 1) { pairId = s[1]; }
            if (s.Length > 2) { encodeType = (EncodeType)int.Parse(s[2]); }
            if (s.Length > 3) { if (int.Parse(s[3]) == 1) urlIsEncoded = true; else urlIsEncoded = false; }
            if (s.Length > 4) { if (int.Parse(s[4]) == 1) headersAreEncoded = true; else headersAreEncoded = false; }
            if (s.Length > 5) { if (int.Parse(s[5]) == 1) bodyIsEncoded = true; else bodyIsEncoded = false; }
        }
        /// <summary>
        /// Returns a string of the x-mte-relay header to transmit to the Mte-Relay.
        /// </summary>
        /// <returns>Value for the x-mte-relay header.</returns>
        public override string ToString()
        {
            string value = $"{clientId},{pairId},{(int)encodeType},{urlIsEncoded: 1 ? 0},{headersAreEncoded: 1 ? 0},{bodyIsEncoded: 1 ? 0}";
            StringBuilder sb = new StringBuilder($"{clientId},{pairId},{(int)encodeType},");
            if (urlIsEncoded) sb.Append("1,"); else sb.Append("0,");
            if (headersAreEncoded) sb.Append("1,"); else sb.Append("0,");
            if (bodyIsEncoded) sb.Append("1"); else sb.Append("0");
            return sb.ToString();
        }
    }
}
