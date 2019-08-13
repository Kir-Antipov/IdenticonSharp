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
        public static IIdenticonProvider Default { get; private set; }

        private static readonly string[] ProviderNamingParts = {
            "provider",
            "identicon",
            "avatar",
            "options"
        };

        private static readonly Dictionary<Type, Func<IIdenticonProvider>> ProvidersByType;
        private static readonly Dictionary<string, Func<IIdenticonProvider>> ProvidersByName;

        private static readonly Dictionary<string, bool> ConfiguredInstances;
        private static readonly Dictionary<Type, IIdenticonProvider> InstancesByType;
        private static readonly Dictionary<string, IIdenticonProvider> InstancesByName;
        #endregion

        #region Init
        static IdenticonManager()
        {
            Default = new GitHubIdenticonProvider();
            InstancesByType = new Dictionary<Type, IIdenticonProvider>();
            InstancesByName = new Dictionary<string, IIdenticonProvider>();
            ConfiguredInstances = new Dictionary<string, bool>();
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
                string name = ClearProviderName(providerInfo.Provider.Name);
                Func<IIdenticonProvider> creator = Expression.Lambda<Func<IIdenticonProvider>>(Expression.New(providerInfo.Constructor)).Compile();
                ProvidersByName[name] = ProvidersByType[providerInfo.Provider] = creator;
                InstancesByName[name] = InstancesByType[providerInfo.Provider] = providerInfo.Provider == typeof(GitHubIdenticonProvider) ? Default : creator();
            }
        }
        #endregion

        #region Create
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
        #endregion

        #region Get
        public static TProvider Get<TProvider>() where TProvider : IIdenticonProvider => (TProvider)Get(typeof(TProvider));

        public static IIdenticonProvider Get(Type providerType)
        {
            if (InstancesByType.TryGetValue(providerType, out var provider))
                return provider;
            return null;
        }

        public static IIdenticonProvider Get(string providerName)
        {
            if (InstancesByName.TryGetValue(providerName, out var provider))
                return provider;
            return null;
        }
        #endregion

        #region Configure
        public static void Configure<TOptions>(string providerName, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            IIdenticonProvider<TOptions> provider = Create(providerName) as IIdenticonProvider<TOptions>;
            Configure(provider, providerName, configurator);
        }

        public static void Configure<TOptions>(Action<TOptions> configurator) where TOptions : IIdenticonOptions =>
            Configure(typeof(TOptions).Name, configurator);

        public static void Configure<TProvider, UOptions>(Action<UOptions> configurator) where TProvider : IIdenticonProvider<UOptions> where UOptions : IIdenticonOptions
        {
            IIdenticonProvider<UOptions> provider = (IIdenticonProvider<UOptions>)Get<TProvider>();
            Configure(provider, typeof(TProvider).Name, configurator);
        }

        private static void Configure<TOptions>(IIdenticonProvider<TOptions> provider, string name, Action<TOptions> configurator) where TOptions : IIdenticonOptions
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            if (configurator is null)
                throw new ArgumentNullException(nameof(configurator));

            if (ConfiguredInstances.TryGetValue(name, out bool configured) ? configured : false)
                throw new InvalidOperationException("This provider has already been configured");

            configurator(provider.Options);

            ConfiguredInstances[name] = true;
        }
        #endregion

        #region ConfigureDefault
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
            if (ConfiguredInstances.TryGetValue(name, out bool configured) ? configured : false)
                throw new InvalidOperationException("Default provider has been already configured");

            Default = provider ?? throw new TypeLoadException(name);

            ConfiguredInstances[name] = true;
        }
        #endregion

        #region Other
        private static string ClearProviderName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            name = name.ToLower();
            foreach (string part in ProviderNamingParts)
                name = name.Replace(part, string.Empty);
            return name;
        }
        #endregion
    }
}
