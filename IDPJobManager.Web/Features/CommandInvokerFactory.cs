using IDPJobManager.Core;
using Nancy.TinyIoc;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace IDPJobManager.Web.Features
{
    [Export(typeof(ICommandInvokerFactory))]
    public class CommandInvokerFactory : ICommandInvokerFactory
    {
        private readonly CompositionContainer _container;

        [ImportingConstructor]
        public CommandInvokerFactory(CompositionContainer container)
        {
            _container = container;
        }

        public TOut Handle<TIn, TOut>(TIn input)
        {
            try
            {
                var commandInvoker = _container.GetExportedValue<ICommandInvoker<TIn, TOut>>();
                return commandInvoker.Execute(input);
            }
            catch (System.Exception e)
            {

                throw;
            }
        }
    }
}
