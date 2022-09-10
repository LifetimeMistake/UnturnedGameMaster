﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Autofac
{
    public class PluginAutoFacRegistrar : IAutoFacRegistrar
    {
        private IDatabaseProvider<GameData> databaseProvider;

        public PluginAutoFacRegistrar(IDatabaseProvider<GameData> databaseProvider)
        {
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        public void RegisterComponents(ContainerBuilder builder)
        {
            AutowirePropertySelector autowirePropertySelector = new AutowirePropertySelector();
            IEnumerable<Type> managers = Assembly.GetCallingAssembly()
                .GetTypes().Where(x => typeof(IManager).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);

            foreach (Type managerType in managers)
            {
                builder.RegisterType(managerType).InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            }

            builder.RegisterType<LoadoutIdProvider>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<TeamIdProvider>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<ArenaIdProvider>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);

            builder.RegisterInstance(databaseProvider).As<IDatabaseProvider<GameData>>().ExternallyOwned();
        }
    }
}
