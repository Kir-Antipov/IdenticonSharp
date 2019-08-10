using System;
using IdenticonSharp;
using IdenticonSharp.Identicons;
using Microsoft.Extensions.DependencyInjection;

namespace KE.IdenticonSharp.AspNetCore
{
    public static class DependcyInjector
    {
        public static IServiceCollection AddIdenticonSharp(this IServiceCollection services)
        {
            services.AddSingleton(IdenticonManager.Default);
            return services;
        }
        public static IServiceCollection AddIdenticonSharp<TProvider, UOptions>(this IServiceCollection services, Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            IdenticonManager.ConfigureDefault<TProvider, UOptions>(configurator);
            return services.AddIdenticonSharp();
        }

        public static IServiceCollection AddIdenticonSharp<TOptions>(this IServiceCollection services, string providerName, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IdenticonManager.ConfigureDefault(providerName, configurator);
            return services.AddIdenticonSharp();
        }

        public static IServiceCollection AddIdenticonSharp<TOptions>(this IServiceCollection services, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IdenticonManager.ConfigureDefault(configurator);
            return services.AddIdenticonSharp();
        }

        public static IServiceCollection AddIdenticonSharp<TProvider>(this IServiceCollection services) where TProvider : IIdenticonProvider
        {
            IdenticonManager.ConfigureDefault<TProvider>();
            return services.AddIdenticonSharp();
        }

        public static IServiceCollection AddIdenticonSharp(this IServiceCollection services, string providerName)
        {
            IdenticonManager.ConfigureDefault(providerName);
            return services.AddIdenticonSharp();
        }
    }
}
