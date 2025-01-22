using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Leavetype
{
    public int Leavetypeid { get; set; }

    public string Abbreviation { get; set; } = null!;

    public bool Attachmentrequired { get; set; }

    public int? Defaultlimit { get; set; }

    public string Description { get; set; } = null!;

    public bool Includefreeday { get; set; }

    public int? LeavetypegroupLeavetypegroupid { get; set; }

    public virtual ICollection<Leaverequest> Leaverequests { get; set; } = new List<Leaverequest>();

    public virtual Leavetypegroup? LeavetypegroupLeavetypegroup { get; set; }

    public virtual ICollection<Userleavelimit> Userleavelimits { get; set; } = new List<Userleavelimit>();

    public virtual ICollection<Leavetype> LeavelimitLeavelimits { get; set; } = new List<Leavetype>();

    public virtual ICollection<Leavetype> LeavetypeLeavetypes { get; set; } = new List<Leavetype>();
}
