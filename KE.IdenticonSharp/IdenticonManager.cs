using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using IdenticonSharp.Identicons;
using System.Collections.Generic;
using IdenticonSharp.Identicons.Defaults.GitHub;

namespace IdenticonSharp
{
    public static class IdenticonManager
    {
        #region Var
        private static readonly Dictionary<Type, Func<IIdenticonProvider>> ProvidersByType;
        private static readonly Dictionary<string, Func<IIdenticonProvider>> ProvidersByName;

        private static readonly string[] ProviderNamingParts = {
            "provider",
            "identicon",
            "gravatar",
            "avatar",
            "options"
        }; 

        public static IIdenticonProvider Default { get; private set; }

        private static bool Configured = false;
        private static readonly object _sync = new object();
        #endregion

        #region Init
        static IdenticonManager()
        {
            Default = new GitHubIdenticonProvider();
            ProvidersByType = new Dictionary<Type, Func<IIdenticonProvider>>();
            ProvidersByName = new Dictionary<string, Func<IIdenticonProvider>>();

            string[] defaultAssemblies = { "System", "Microsoft", "netstandart", "mscorlib", "WindowsBase", "SixLabors" };
            var providersInfo = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !defaultAssemblies.Any(x.GetName().Name.StartsWith))
                .SelectMany(x => x.GetExportedTypes())
                .Where(typeof(IIdenticonProvider).IsAssignableFrom)
                .Where(type => !type.IsAbstract)
                .Select(x => new { Provider = x, Constructor = x.GetConstructor(Type.EmptyTypes) })
                .Where(x => x.Constructor != null);

            foreach (var providerInfo in providersInfo)
            {
                Func<IIdenticonProvider> creator = Expression.Lambda<Func<IIdenticonProvider>>(Expression.New(providerInfo.Constructor)).Compile();
                ProvidersByType[providerInfo.Provider] = creator;
                ProvidersByName[ClearProviderName(providerInfo.Provider.Name)] = creator;
            }
        }
        #endregion

        #region Functions
        private static string ClearProviderName(string name)
        {
            name = name.ToLower();
            foreach (string part in ProviderNamingParts)
                name = name.Replace(part, string.Empty);
            return name;
        }

        public static IIdenticonProvider Create(string name)
        {
            if (ProvidersByName.TryGetValue(ClearProviderName(name), out var creator))
                return creator();

            return default;
        }

        public static IIdenticonProvider Create(Type providerType) 
        {
            if (ProvidersByType.TryGetValue(providerType, out var creator))
                return creator();

            if (typeof(IIdenticonProvider).IsAssignableFrom(providerType))
            {
                ConstructorInfo constructor = providerType.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                    return (IIdenticonProvider)constructor.Invoke(new object[0]);
            }

            return default;
        }

        public static TProvider Create<TProvider>() where TProvider : IIdenticonProvider => (TProvider)Create(typeof(TProvider));

        public static TProvider Create<TProvider, UOptions>(Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            TProvider provider = Create<TProvider>();
            configurator(provider.Options);
            return provider;
        }

        public static IIdenticonProvider Create<TOptions>(Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            if (Create(typeof(TOptions).Name) is IIdenticonProvider<TOptions> provider)
            {
                configurator(provider.Options);
                return provider;
            }
            return default;
        }

        public static void ConfigureDefault<TProvider>() where TProvider : IIdenticonProvider =>
            ConfigureDefault(Create<TProvider>(), typeof(TProvider).Name);

        public static void ConfigureDefault(string providerName) =>
            ConfigureDefault(Create(providerName), providerName);

        public static void ConfigureDefault<TOptions>(string providerName, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IIdenticonProvider<TOptions> provider = Create(providerName) as IIdenticonProvider<TOptions>;
            ConfigureDefault(provider, providerName);
            configurator?.Invoke(provider.Options);
        }
        public static void ConfigureDefault<TOptions>(Action<TOptions> configurator) where TOptions : IIdenticonOptions =>
            ConfigureDefault(typeof(TOptions).Name, configurator);

        public static void ConfigureDefault<TProvider, UOptions>(Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            IIdenticonProvider<UOptions> provider = Create<TProvider>();
            ConfigureDefault(provider, typeof(TProvider).Name);
            configurator?.Invoke(provider.Options);
        }

        private static void ConfigureDefault(IIdenticonProvider provider, string name)
        {
            lock (_sync)
            {
                if (Configured)
                    throw new InvalidOperationException("The default provider has already been configured");

                Default = provider ?? throw new TypeLoadException(name);

                Configured = true;
            }
        }
        #endregion
    }
}
