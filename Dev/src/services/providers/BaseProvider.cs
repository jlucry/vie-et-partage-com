using Microsoft.Extensions.Logging;

namespace Services
{
    /// <summary>
    /// The base provider.
    /// </summary>
    public abstract class BaseProvider
    {
        /// <summary>
        /// Base provider constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public BaseProvider(Services.WcmsAppContext appContext)
        {
            // Save the application context...
            AppContext = appContext;
            // Trace...
            _Log = AppContext?.LoggerFactory?.CreateLogger(this.GetType().Name);
            _Log?.LogDebug("BaseProvider: {0}", this.GetType().Name);
        }

        /// <summary>
        /// Last error message.
        /// </summary>
        public string LastError { get; set; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected ILogger _Log { get; private set; }

        /// <summary>
        /// The application context.
        /// </summary>
        protected Services.WcmsAppContext AppContext { get; private set; }
    }
}
