using System;
using System.Collections.Generic;
using System.Linq;

namespace IDPJobManager.Core.Utils
{
    /// <summary>
    /// AppDomain Loader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AppDomainLoader<T>
    {
        private static readonly Dictionary<string, AppDomain> appdomainCache = new Dictionary<string, AppDomain>();

        /// <summary>
        /// Load the type and return it by using new <see cref="System.AppDomain"/>
        /// </summary>
        /// <param name="dllPath"></param>
        /// <param name="classPath"></param>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static T Load(string dllPath, string classPath, out AppDomain appDomain)
        {
            var appDomainCacheKey = dllPath.ToUpper();
            lock (appdomainCache)
            {
                if (appdomainCache.ContainsKey(appDomainCacheKey))
                {
                    appDomain = appdomainCache[appDomainCacheKey];
                }
                else
                {
                    AppDomainSetup setup = new AppDomainSetup();
                    if (System.IO.File.Exists($"{dllPath}.config"))
                        setup.ConfigurationFile = $"{dllPath}.config";
                    setup.ShadowCopyFiles = "true";
                    setup.ApplicationBase = System.IO.Path.GetDirectoryName(dllPath);
                    appDomain = AppDomain.CreateDomain(System.IO.Path.GetFileName(dllPath), null, setup);
                    AppDomain.MonitoringIsEnabled = true;
                    appdomainCache.Add(appDomainCacheKey, appDomain);
                }
                return (T)appDomain.CreateInstanceFromAndUnwrap(dllPath, classPath);
            }
        }

        /// <summary>
        /// Unload the <see cref="System.AppDomain"/>
        /// </summary>
        /// <param name="appDomain"></param>
        public static void UnLoad(AppDomain appDomain)
        {
            lock (appdomainCache)
            {
                if (appdomainCache.ContainsValue(appDomain))
                {
                    var keys = appdomainCache.Where(d => d.Value == appDomain).Select(d => d.Key).ToList();
                    foreach (var key in keys)
                    {
                        appdomainCache.Remove(key);
                    }
                } 
            }
            AppDomain.Unload(appDomain);
            appDomain = null;
        }
    }
}
