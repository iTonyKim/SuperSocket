using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSuperSocket<TReceivePackage, TPipelineFilter>(this IHostBuilder hostBuilder)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return hostBuilder.UseSuperSocketWithFilterFactory<TReceivePackage, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
        }

        public static IHostBuilder UseSuperSocket<TReceivePackage>(this IHostBuilder hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.TryAdd(ServiceDescriptor.Singleton<IChannelCreatorFactory, TcpChannelCreatorFactory>());
                    services.AddSingleton<IPipelineFilterFactory<TReceivePackage>>(new DelegatePipelineFilterFactory<TReceivePackage>(filterFactory));
                    services.AddSingleton<IHostedService, SuperSocketService<TReceivePackage>>();
                }
            );

            return hostBuilder;
        }


        public static IHostBuilder UseSuperSocketWithFilterFactory<TReceivePackage, TPipelineFilterFactory>(this IHostBuilder hostBuilder)
            where TReceivePackage : class
            where TPipelineFilterFactory : IPipelineFilterFactory<TReceivePackage>, new()
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.TryAdd(ServiceDescriptor.Singleton<IChannelCreatorFactory, TcpChannelCreatorFactory>());
                    services.AddSingleton<IHostedService, SuperSocketService<TReceivePackage, TPipelineFilterFactory>>();
                }
            );

            return hostBuilder;
        }

        public static IHostBuilder UseSession<TSession>(this IHostBuilder hostBuilder)
            where TSession : AppSession, new()
        {
            return hostBuilder.UseSessionFactory<GenericSessionFactory<TSession>>();
        }

        public static IHostBuilder UseSessionFactory<TSessionFactory>(this IHostBuilder hostBuilder)
            where TSessionFactory : class, ISessionFactory
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<ISessionFactory, TSessionFactory>();
                }
            );

            return hostBuilder;
        }

        public static IHostBuilder ConfigurePackageHandler<TReceivePackage>(this IHostBuilder hostBuilder, Func<IAppSession, TReceivePackage, Task> packageHandler)
            where TReceivePackage : class
        {
            if (packageHandler == null)
            {
                return hostBuilder;
            }

            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, TReceivePackage, Task>>(packageHandler);
                }
            );
        }

        public static IHostBuilder ConfigureSuperSocket(this IHostBuilder hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            );
        }

        public static IHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this IHostBuilder<TReceivePackage> hostBuilder, Action<ServerOptions> configurator)
            where TReceivePackage : class
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            ) as IHostBuilder<TReceivePackage>;
        }

        public static IServer BuildAsServer(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.Services.GetService<IEnumerable<IHostedService>>().OfType<IServer>().FirstOrDefault();
        }

        public static IHostBuilder<TReceivePackage> ConfigurePackageHandler<TReceivePackage>(this IHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, Task> packageHandler)
            where TReceivePackage : class
        {
            if (packageHandler == null)
            {
                return hostBuilder;
            }

            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, TReceivePackage, Task>>(packageHandler);
                }
            ) as IHostBuilder<TReceivePackage>;
        }
    }
}
