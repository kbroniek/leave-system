namespace GoldenEye.Backend.Core.Marten.Registration.Cusom;
using System;
using global::Marten;
using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.DDD.Events;
using GoldenEye.Backend.Core.DDD.Queries;
using GoldenEye.Backend.Core.Marten.Events.Storage.Custom;
using GoldenEye.Backend.Core.Marten.Repositories.Custom;
using GoldenEye.Backend.Core.Repositories;
using GoldenEye.Shared.Core.Extensions.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class Registration
{

    public static void AddMartenEventSourcedRepository<TEntity>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TEntity : class, IEventSource, new()
    {
        services.Add((IServiceProvider sp) => new MartenEventSourcedRepository<TEntity>(sp.GetService<IDocumentSession>(), sp.GetService<MartenEventStore>()), serviceLifetime);
        services.Add((Func<IServiceProvider, IRepository<TEntity>>)((IServiceProvider sp) => sp.GetService<MartenEventSourcedRepository<TEntity>>()), serviceLifetime);
        services.Add((Func<IServiceProvider, IReadonlyRepository<TEntity>>)((IServiceProvider sp) => sp.GetService<MartenEventSourcedRepository<TEntity>>()), serviceLifetime);
    }

    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services, ServiceLifetime withLifetime = ServiceLifetime.Transient)
        where TCommand : ICommand
        where TCommandHandler : class, ICommandHandler<TCommand> =>
            services.Add<TCommandHandler>(withLifetime)
                .Add<IRequestHandler<TCommand, Unit>>(sp => sp.GetService<TCommandHandler>()!, withLifetime)
                .Add<ICommandHandler<TCommand>>(sp => sp.GetService<TCommandHandler>()!, withLifetime);

    public static IServiceCollection AddQueryHandler<TQuery, TResponse, TQueryHandler>(
        this IServiceCollection services, ServiceLifetime withLifetime = ServiceLifetime.Transient)
        where TQuery : IQuery<TResponse>
        where TQueryHandler : class, IQueryHandler<TQuery, TResponse> =>
            services.Add<TQueryHandler>(withLifetime)
                .Add<IRequestHandler<TQuery, TResponse>>(sp => sp.GetService<TQueryHandler>()!, withLifetime)
                .Add<IQueryHandler<TQuery, TResponse>>(sp => sp.GetService<TQueryHandler>()!, withLifetime);
}
