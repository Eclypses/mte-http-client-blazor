namespace MteHttpClient.Interfaces
{
    /// <summary>
    /// Used to create an ECDH Helper at runtime.
    /// </summary>
    public interface IEcdhFactory
    {
        /// <summary>
        /// Creates the ECDHHelper using an
        /// interface so that you can use
        /// different implementations for
        /// Blazor and dotNet.
        /// </summary>
        /// <returns>Implementation of the IECDHHelperMethods class.</returns>
        IEcdhHelperMethods Create();
    }
}
