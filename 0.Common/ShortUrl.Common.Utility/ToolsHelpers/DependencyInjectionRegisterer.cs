using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ShortUrl.Common.Utility.ToolsHelpers
{
    // In DependencyInjection static class:
    // new DependencyInjectionRegisterer().ResolveAllTypes(services, ServiceLifetime.Scoped, typeof(DependencyInjection), "Service");
    // new DependencyInjectionRegisterer().ResolveAllTypes(services, ServiceLifetime.Scoped, typeof(DependencyInjection), "Provider");
    // new DependencyInjectionRegisterer().ResolveAllTypes(services, ServiceLifetime.Scoped, typeof(DependencyInjection), "Repository");
    public class DependencyInjectionRegisterer
    {
        public void ResolveAllTypes(IServiceCollection services, ServiceLifetime serviceLifetime, Type refType, string suffix)
        {
            var assembly = refType.GetTypeInfo().Assembly;

            var allServices = assembly.GetTypes().Where(t =>
                t.GetTypeInfo().IsClass &&
                !t.GetTypeInfo().IsAbstract &&
                !t.GetType().IsInterface &&
                //(!t.Name.StartsWith("I") ||) &&
                t.Name.EndsWith(suffix)
            );


            foreach (var type in allServices)
            {
                var allInterfaces = type.GetInterfaces();
                var mainInterfaces = allInterfaces.Except
                    (allInterfaces.SelectMany(t => t.GetInterfaces()));
                foreach (var itype in mainInterfaces)
                {
                    if (allServices.Any(x => !x.Equals(type) && itype.IsAssignableFrom(x)))
                    {
                        throw new Exception("The " + itype.Name +
                                            " type has more than one implementations, please change your filter");
                    }
                    services.Add(new ServiceDescriptor(itype, type, serviceLifetime));
                }
            }
        }

    }
}
