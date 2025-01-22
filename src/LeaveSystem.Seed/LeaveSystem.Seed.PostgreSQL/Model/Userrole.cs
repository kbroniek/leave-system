using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Userrole
{
    public int Userroleid { get; set; }

    public string Role { get; set; } = null!;

    public int UserUserid { get; set; }

    public virtual User UserUser { get; set; } = null!;
}
