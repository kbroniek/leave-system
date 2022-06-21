﻿using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Domains;
public class Department : IHaveId<Guid>
{
    public Guid DepartmentId { get; set; }
    public string? Title { get; set; }
    public UserDepartment? Users { get; set; }

    public Guid Id => DepartmentId;

    public class UserDepartment
    {
        public string[]? UserIds { get; set; }
    }
}
