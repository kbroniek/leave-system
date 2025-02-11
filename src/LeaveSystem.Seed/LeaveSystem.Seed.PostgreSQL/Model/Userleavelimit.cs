using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Userleavelimit
{
    public int Userleavelimitid { get; set; }

    public string? Description { get; set; }

    public double? UserLimit { get; set; }

    public double Overduelimit { get; set; }

    public DateOnly? Validsince { get; set; }

    public DateOnly? Validuntil { get; set; }

    public int LeavetypeLeavetypeid { get; set; }

    public int UserUserid { get; set; }

    public virtual Leavetype LeavetypeLeavetype { get; set; } = null!;

    public virtual User UserUser { get; set; } = null!;
}
