using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.Mappers;
using LeaveSystem.Services.LeaveType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace LeaveSystem.UnitTests.Services.LeaveType
{
    [TestClass]
    public class LeaveTypeServiceTests
    {
        private static Lazy<ValueTask<LeaveSystemDbContext>> dbContext = new Lazy<ValueTask<LeaveSystemDbContext>>(DbContextFactory.CreateDbContext);
        private Guid leaveTypeId = Guid.Parse("4eac08bd-c504-4644-bd74-cd0bacb71899");
        private string description = "fake description";
        private string properties = "fake properties";
        private LeaveTypeService sut;

        [TestInitialize]
        public async Task Initialize()
        {
            sut = new LeaveTypeService(await dbContext.Value, new LeaveTypeMapper());
        }

        [TestMethod]
        public async Task o01_ShouldCreateLeaveType_Successfully()
        {
            await sut.Create(new CreateLeaveType(leaveTypeId, description, properties));
        }

        [TestMethod]
        public async Task o02_ShouldGetLeaveType_Successfully()
        {
            var leaveType = await sut.Get(leaveTypeId);

            leaveType.Should().BeEquivalentTo(new
            {
                Description = description,
                LeaveTypeId = leaveTypeId,
                Properties = properties,
            });
        }

        [TestMethod]
        public async Task o03_ShouldGetAllLeaveTypes_Successfully()
        {
            var leaveTypes = await sut.GetAll(null, null);

            leaveTypes.Should().BeEquivalentTo(new[] {
                new
                {
                    Description = description,
                    LeaveTypeId = leaveTypeId,
                    Properties = properties,
                }
            });
        }

        [TestMethod]
        public async Task o04_ShouldUpdateLeaveType_Successfully()
        {
            string newDescription = "new fake description";
            string newProperties = "new fake properties";
            var leaveTypeUpdated = await sut.Update(new UpdateLeaveType(leaveTypeId, newDescription, newProperties));
            var leaveTypeGet = await sut.Get(leaveTypeId);

            leaveTypeUpdated.Should().BeEquivalentTo(leaveTypeGet);
        }

        [TestMethod]
        public async Task o05_ShouldRemoveLeaveType_Successfully()
        {
            await sut.Remove(leaveTypeId);
            var leaveTypes = await sut.GetAll(null, null);

            leaveTypes.Should().BeEmpty();
        }
    }
}