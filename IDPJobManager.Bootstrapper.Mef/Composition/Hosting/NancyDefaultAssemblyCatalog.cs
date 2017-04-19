using Nancy;

namespace IDPJobManager.Bootstrapper.Mef.Composition.Hosting
{

    /// <summary>
    /// Special <see cref="AssemblyCatalog"/> that automatically includes Nancy itself.
    /// </summary>
    public class NancyDefaultAssemblyCatalog : NancyAssemblyCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyDefaultAssemblyCatalog()
            : base(typeof(NancyEngine).Assembly)
        {

        }

    }

}
