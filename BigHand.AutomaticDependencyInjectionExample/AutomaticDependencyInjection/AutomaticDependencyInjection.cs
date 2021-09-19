using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AutomaticDependencyInjectionExample.AutomaticDependencyInjection
{
    public static class AutomaticDependencyInjection
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            var assemblies = GetAssemblies();

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<ITransient>())
                .AsSelfWithInterfaces()
                .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo<IScoped>())
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ISingleton>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();
            var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.FullName != null && assembly.FullName.StartsWith("BigHand")).ToList();

            foreach (var assembly in appDomainAssemblies) assembliesToCheck.Enqueue(assembly);

            while (assembliesToCheck.Any())
            {
                var assemblyToCheck = assembliesToCheck.Dequeue();

                foreach (var reference in assemblyToCheck.GetReferencedAssemblies()
                    .Where(assembly => assembly.FullName.StartsWith("BigHand")))
                    if (!loadedAssemblies.Contains(reference.FullName))
                    {
                        var assembly = Assembly.Load(reference);
                        assembliesToCheck.Enqueue(assembly);
                        loadedAssemblies.Add(reference.FullName);
                        assemblies.Add(assembly);
                    }
            }

            assemblies.AddRange(appDomainAssemblies);

            return assemblies;
        }
    }
}