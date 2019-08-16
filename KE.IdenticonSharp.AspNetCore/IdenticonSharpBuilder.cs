using System;
using IdenticonSharp;
using IdenticonSharp.Identicons;
using Microsoft.Extensions.DependencyInjection;

namespace KE.IdenticonSharp.AspNetCore
{
    public class IdenticonSharpBuilder
    {
        #region Var
        private readonly IServiceCollection Services;
        #endregion

        #region Init
        internal IdenticonSharpBuilder(IServiceCollection services) => Services = services;
        #endregion

        #region Configure
        public IdenticonSharpBuilder Configure<TOptions>(string providerName, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IdenticonManager.Configure(providerName, configurator);
            IIdenticonProvider provider = IdenticonManager.Get(providerName);
            Services.AddSingleton(provider.GetType(), provider);
            return this;
        }

        public IdenticonSharpBuilder Configure<TOptions>(Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IdenticonManager.Configure(configurator);
            IIdenticonProvider provider = IdenticonManager.Get(typeof(TOptions).Name);
            Services.AddSingleton(provider.GetType(), provider);
            return this;
        }

        public IdenticonSharpBuilder Configure<TProvider, UOptions>(Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            IdenticonManager.Configure<TProvider, UOptions>(configurator);
            TProvider provider = IdenticonManager.Get<TProvider>();
            Services.AddSingleton(typeof(TProvider), provider);
            return this;
        }
        #endregion

        #region ConfigureDefault
        public IdenticonSharpBuilder ConfigureDefault<TProvider>() where TProvider : IIdenticonProvider
        {
            IdenticonManager.ConfigureDefault<TProvider>();
            Services.AddSingleton(IdenticonManager.Default);
            return this;
        }

        public IdenticonSharpBuilder ConfigureDefault(string providerName)
        {
            IdenticonManager.ConfigureDefault(providerName);
            Services.AddSingleton(IdenticonManager.Default);
            return this;
        }

        public IdenticonSharpBuilder ConfigureDefault<TOptions>(string providerName, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IdenticonManager.ConfigureDefault(providerName, configurator);
            Services.AddSingleton(IdenticonManager.Default);
            return this;
        }

        public IdenticonSharpBuilder ConfigureDefault<TOptions>(Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IdenticonManager.ConfigureDefault(configurator);
            Services.AddSingleton(IdenticonManager.Default);
            return this;
        }

        public IdenticonSharpBuilder ConfigureDefault<TProvider, UOptions>(Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            IdenticonManager.ConfigureDefault<TProvider, UOptions>(configurator);
            Services.AddSingleton(IdenticonManager.Default);
            return this;
        }
        #endregion

        #region Other
        public IServiceCollection Build() => Services;
        #endregion
    }
}
