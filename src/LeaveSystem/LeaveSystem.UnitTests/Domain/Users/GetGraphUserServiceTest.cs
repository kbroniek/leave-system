using GoldenEye.Backend.Core.Exceptions;
using LeaveSystem.Domain.Users;
using LeaveSystem.GraphApi;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.TestDataGenerators;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Moq;

namespace LeaveSystem.UnitTests.Domain.Employees;

public class GetGraphUserServiceTest
{
    [Fact]
    public async Task WhenGetCollection_ThenReturnEmployees()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);
        var resolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);

        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        mockRequestAdapter.SetupUserCollectionResponse()
            .ReturnsAsync(new UserCollectionResponse
            {
                Value = new List<User>
                {
                    new() {
                        Id = "6d60db6d-c772-4bfc-b8dd-23b80338c990",
                        Mail = "fake@test.com",
                        DisplayName = "Test",
                        AdditionalData = {
                            { TestData.FakeRoleAttributeName, TestData.FakeRolesJson }
                        }
                    },
                    new() {
                        Id = "6d60db6d-c772-4bfc-b8dd-23b80338c991"
                    }
                }
            });

        var sut = new GetGraphUserService(graphClientFactoryMock.Object, resolver);
        //When
        var result = await sut.Get(new[] { "6d60db6d-c772-4bfc-b8dd-23b80338c990" }, CancellationToken.None);
        var resultAll = await sut.Get(CancellationToken.None);
        //Then
        var expected = new[] {
            new
            {
                Id = "6d60db6d-c772-4bfc-b8dd-23b80338c990",
                Email = "fake@test.com",
                Name = "Test",
                Roles = new string[]
                {
                    "nulla",
                    "aliquip",
                    "amet",
                    "aliqua",
                    "magna",
                    "cillum",
                    "excepteur"
                }
            },
            new
            {
                Id = "6d60db6d-c772-4bfc-b8dd-23b80338c991",
                Email = (string?)null,
                Name = (string?)null,
                Roles = Array.Empty<string>()
            }
        };
        result.Should().BeEquivalentTo(expected);
        resultAll.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task WhenGet_CannotFind_ThenThrowError()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.SetupUserCollectionResponse()
            .ReturnsAsync((UserCollectionResponse?)null);
        mockRequestAdapter.SetupUserResponse()
            .ReturnsAsync((User?)null);
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new GetGraphUserService(graphClientFactoryMock.Object, rolesAttributeNameResolver);
        //When
        var act = async () => await sut.Get(new[] { "32d8b5fc-234c-42c1-a75a-3c62efbe768b" }, CancellationToken.None);
        var actAll = async () => await sut.Get(CancellationToken.None);
        var actOne = async () => await sut.Get("32d8b5fc-234c-42c1-a75a-3c62efbe768c", CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("UserCollectionResponse with id: Can't find users in graph api was not found.");
        await actAll.Should().ThrowAsync<NotFoundException>().WithMessage("UserCollectionResponse with id: Can't find users in graph api was not found.");
        await actOne.Should().ThrowAsync<NotFoundException>().WithMessage("UserCollectionResponse with id: 32d8b5fc-234c-42c1-a75a-3c62efbe768c was not found.");
    }

    [Fact]
    public async Task WhenGet_ThenGetGraphUser()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.SetupUserResponse()
            .ReturnsAsync(new User
            {
                Id = "5a864035-7c79-458b-aa15-45ff94a94660",
                Mail = "fake@test.com",
                DisplayName = "Test",
                AdditionalData = {
                    { TestData.FakeRoleAttributeName, TestData.FakeRolesJson }
                }
            });
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new GetGraphUserService(graphClientFactoryMock.Object, rolesAttributeNameResolver);
        //When
        var result = await sut.Get("5a864035-7c79-458b-aa15-45ff94a94660", CancellationToken.None);
        //Then
        var expected = new
        {
            Id = "5a864035-7c79-458b-aa15-45ff94a94660",
            Email = "fake@test.com",
            Name = "Test",
            Roles = new string[]
            {
                "nulla",
                "aliquip",
                "amet",
                "aliqua",
                "magna",
                "cillum",
                "excepteur"
            }
        };
        result.Should().BeEquivalentTo(expected);
    }
}
