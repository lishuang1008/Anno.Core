﻿using System;
using System.Linq;
using Autofac;
using System.Reflection;

namespace Anno.Loader
{
    public class AutofacModule : Autofac.Module
    {
        //注意以下写法
        //builder.RegisterType<GuidTransientAnnoService>().As<IGuidTransientAnnoService>();
        //builder.RegisterType<GuidScopedAnnoService>().As<IGuidScopedAnnoService>().InstancePerLifetimeScope();
        //builder.RegisterType<GuidSingletonAnnoService>().As<IGuidSingletonAnnoService>().SingleInstance();

        protected override void Load(ContainerBuilder builder)
        {
            // The generic ILogger<TCategoryName> service was added to the ServiceCollection by ASP.NET Core.
            // It was then registered with Autofac using the Populate method in ConfigureServices.

            //builder.Register(c => new ValuesService(c.Resolve<ILogger<ValuesService>>()))
            //    .As<IValuesService>()
            //    .InstancePerLifetimeScope();
            // builder.RegisterType<BaseRepository>().As<IBaseRepository>();
            Const.AppSettings.IocDll.Distinct().ToList().ForEach(d =>
            {
                RegisterAssembly(builder, Const.Assemblys.Dic[d]);
            });
            foreach (var assembly in Const.Assemblys.DependedTypes)
            {
                RegisterAssembly(builder, assembly);
            }
        }
        private void RegisterAssembly(ContainerBuilder builder, Assembly assembly)
        {
            assembly.GetTypes().Where(x => x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract && !x.GetTypeInfo().IsInterface).ToList().ForEach(
                   t =>
                   {
                       var interfaces = t.GetInterfaces();
                       if (IsAssignableFrom(t, "Anno.EngineData.BaseModule")
                       || interfaces.ToList().Exists(i => i.Name == "IFilterMetadata")
                       || interfaces.Length <= 0)
                       {
                           builder.RegisterType(t);
                       }
                       else if (!interfaces.ToList().Exists(i => i.Name == "IEntity"))
                       {
                           builder.RegisterType(t).As(t.GetInterfaces());
                       }
                   });
        }
        internal static bool IsAssignableFrom(Type type, string baseTypeFullName)
        {
            bool success = false;
            if (type == null)
            {
                success = false;
            }
            else if (type.FullName == baseTypeFullName)
            {
                success = true;
            }
            else if (type.BaseType != null)
            {
                success = IsAssignableFrom(type.BaseType, baseTypeFullName);
            }
            return success;
        }
    }
}