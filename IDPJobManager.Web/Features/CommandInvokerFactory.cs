using IDPJobManager.Core;
using Nancy.TinyIoc;

namespace IDPJobManager.Web.Features
{
    public class CommandInvokerFactory : ICommandInvokerFactory
    {
        private readonly TinyIoCContainer _container;

        public CommandInvokerFactory(TinyIoCContainer container)
        {
            _container = container;
        }

        public TOut Handle<TIn, TOut>(TIn input)
        {
            var commandInvoker = _container.Resolve<ICommandInvoker<TIn, TOut>>();
            return commandInvoker.Execute(input);
        }
    }
}
