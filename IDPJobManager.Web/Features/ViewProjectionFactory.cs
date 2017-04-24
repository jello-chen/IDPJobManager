using IDPJobManager.Core;
using Nancy.TinyIoc;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace IDPJobManager.Web.Features
{
    [Export(typeof(IViewProjectionFactory))]
    public class ViewProjectionFactory : IViewProjectionFactory
    {
        private readonly CompositionContainer _container;

        [ImportingConstructor]
        public ViewProjectionFactory(CompositionContainer container)
        {
            _container = container;
        }

        public TOut Get<TIn, TOut>(TIn input)
        {
            var viewProjection = _container.GetExportedValue<IViewProjection<TIn, TOut>>();
            return viewProjection.Project(input);
        }
    }
}
