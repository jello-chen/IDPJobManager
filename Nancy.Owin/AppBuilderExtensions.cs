using Owin;
using System;
using System.Threading;

namespace Nancy.Owin
{
    /// <summary>
	/// OWIN extensions for Nancy
	/// </summary>
	public static class AppBuilderExtensions
    {
        private const string AppDisposingKey = "host.OnAppDisposing";

        /// <summary>
        /// Adds Nancy to the OWIN pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="options">The Nancy options.</param>
        /// <returns>IAppBuilder.</returns>
        public static IAppBuilder UseNancy(this IAppBuilder builder, NancyOptions options = null)
        {
            NancyOptions nancyOptions = options ?? new NancyOptions();
            AppBuilderExtensions.HookDisposal(builder, nancyOptions);
            return builder.Use(NancyMiddleware.UseNancy(nancyOptions), new object[0]);
        }

        /// <summary>
        /// Adds Nancy to the OWIN pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configuration">A configuration builder action.</param>
        /// <returns>IAppBuilder.</returns>
        public static IAppBuilder UseNancy(this IAppBuilder builder, Action<NancyOptions> configuration)
        {
            NancyOptions nancyOptions = new NancyOptions();
            configuration(nancyOptions);
            return builder.UseNancy(nancyOptions);
        }

        private static void HookDisposal(IAppBuilder builder, NancyOptions nancyOptions)
        {
            if (!builder.Properties.ContainsKey("host.OnAppDisposing"))
            {
                return;
            }
            CancellationToken? cancellationToken = builder.Properties["host.OnAppDisposing"] as CancellationToken?;
            if (cancellationToken.HasValue)
            {
                cancellationToken.Value.Register(new Action(nancyOptions.Bootstrapper.Dispose));
            }
        }
    }
}
