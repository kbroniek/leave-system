using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.UnitTests.Providers;
using Marten;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class HandleCreateWorkingHoursTest
{
    private IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> repository;
    private WorkingHoursFactory factory;

    private HandleCreateWorkingHours GetSut() => new(repository, factory);

    [Fact]
    public async Task WhenValidationExceptionWasThrown_ThenNotAddAndNotSaveChanges()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        var validator = new CreateWorkingHoursValidator(Substitute.For<IDocumentSession>());
        factory = Substitute.For<WorkingHoursFactory>(validator);
        factory.Create(command).Throws<ValidationException>();
        repository = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        var sut = GetSut();
        //When
        var act = async () => { await sut.Handle(command, CancellationToken.None); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
        factory.Received(1).Create(ArgExtensions.IsEquivalentTo<CreateWorkingHours>(command));
        await repository.DidNotReceiveWithAnyArgs().Add(default);
        await repository.DidNotReceiveWithAnyArgs().SaveChanges();
    }

    [Fact]
    public async Task WhenAddThrowException_ThenNotSaveChanges()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        var validator = new CreateWorkingHoursValidator(Substitute.For<IDocumentSession>());
        factory = Substitute.For<WorkingHoursFactory>(validator);
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration));
        factory.Create(command).Returns(
            fakeWorkingHours
        );
        repository = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        repository.Add(fakeWorkingHours, Arg.Any<CancellationToken>()).Throws<Exception>();
        var sut = GetSut();
        //When
        var act = async () => { await sut.Handle(command, CancellationToken.None); };
        //Then
        await act.Should().ThrowAsync<Exception>();
        factory.Received(1).Create(ArgExtensions.IsEquivalentTo<CreateWorkingHours>(command));
        await repository.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await repository.DidNotReceiveWithAnyArgs().SaveChanges();
    }

    [Fact]
    public async Task WhenHandlingSuccessful_PassAllMethodsAndReturnUnitValue()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        var validator = new CreateWorkingHoursValidator(Substitute.For<IDocumentSession>());
        factory = Substitute.For<WorkingHoursFactory>(validator);
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration));
        factory.Create(command).Returns(
            fakeWorkingHours
        );
        repository = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        var sut = GetSut();
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        factory.Received(1).Create(ArgExtensions.IsEquivalentTo<CreateWorkingHours>(command));
        await repository.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await repository.Received(1).SaveChanges();
        result.Should().BeEquivalentTo(Unit.Value);
    }
}