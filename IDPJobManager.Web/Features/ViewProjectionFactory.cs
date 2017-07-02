using IDPJobManager.Core;
using Nancy.TinyIoc;

namespace IDPJobManager.Web.Features
{
    public class ViewProjectionFactory : IViewProjectionFactory
    {
        private readonly TinyIoCContainer _container;

        public ViewProjectionFactory(TinyIoCContainer container)
        {
            _container = container;
        }

        public TOut Get<TIn, TOut>(TIn input)
        {
            var viewProjection = _container.Resolve<IViewProjection<TIn, TOut>>();
            return viewProjection.Project(input);
        }
    }
}
