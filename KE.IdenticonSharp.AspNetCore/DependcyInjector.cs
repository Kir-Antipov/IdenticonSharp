using System;
using IdenticonSharp.Identicons;
using Microsoft.Extensions.DependencyInjection;

namespace KE.IdenticonSharp.AspNetCore
{
    public static class DependcyInjector
    {
        public static IdenticonSharpBuilder AddIdenticonSharp(this IServiceCollection services) => new IdenticonSharpBuilder(services);

        public static IdenticonSharpBuilder AddIdenticonSharp<TProvider, UOptions>(this IServiceCollection services, Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            return new IdenticonSharpBuilder(services).ConfigureDefault<TProvider, UOptions>(configurator);
        }

        public static IdenticonSharpBuilder AddIdenticonSharp<TOptions>(this IServiceCollection services, string providerName, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            return new IdenticonSharpBuilder(services).ConfigureDefault(providerName, configurator);
        }

        public static IdenticonSharpBuilder AddIdenticonSharp<TOptions>(this IServiceCollection services, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            return new IdenticonSharpBuilder(services).ConfigureDefault(configurator);
        }

        public static IdenticonSharpBuilder AddIdenticonSharp<TProvider>(this IServiceCollection services) where TProvider : IIdenticonProvider
        {
            return new IdenticonSharpBuilder(services).ConfigureDefault<TProvider>();
        }

        public static IdenticonSharpBuilder AddIdenticonSharp(this IServiceCollection services, string providerName)
        {
            return new IdenticonSharpBuilder(services).ConfigureDefault(providerName);
        }
    }
}
