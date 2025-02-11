using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Leavetypegroup
{
    public int Leavetypegroupid { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Leavetype> Leavetypes { get; set; } = new List<Leavetype>();
}
