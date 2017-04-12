using IDPJobManager.Core;
using IDPJobManager.Web.Features;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Web.Extensions
{
    public static class TinyIoCContainerExtensions
    {
        public static void RegisterIViewProjections(this TinyIoCContainer container)
        {
            var viewProjectionTypes = Assembly.GetAssembly(typeof(IViewProjection<,>))
                                              .DefinedTypes
                                              .Select(t => new
                                              {
                                                  Type = t.AsType(),
                                                  Interface = t.ImplementedInterfaces.FirstOrDefault(
                                                      i => i.IsGenericType() && i.GetGenericTypeDefinition() == typeof(IViewProjection<,>))
                                              })
                                              .Where(t => t.Interface != null);
            foreach (var viewProjectionType in viewProjectionTypes)
            {
                container.Register(viewProjectionType.Interface, viewProjectionType.Type);
            }
            container.Register(typeof(IViewProjectionFactory), (cContainer, overloads) => new ViewProjectionFactory(cContainer));
        }

        public static void RegisterICommandInvoker(this TinyIoCContainer container)
        {
            var commandInvokerTypes = Assembly.GetAssembly(typeof(ICommandInvoker<,>))
                                              .DefinedTypes
                                              .Select(t => new
                                              {
                                                  Type = t.AsType(),
                                                  Interface = t.ImplementedInterfaces.FirstOrDefault(
                                                      i => i.IsGenericType() && i.GetGenericTypeDefinition() == typeof(ICommandInvoker<,>))
                                              })
                                              .Where(t => t.Interface != null);
            foreach (var commandInvokerType in commandInvokerTypes)
            {
                container.Register(commandInvokerType.Interface, commandInvokerType.Type);
            }
            container.Register(typeof(ICommandInvokerFactory), (cContainer, overloads) => new CommandInvokerFactory(cContainer));
        }
    }
}
