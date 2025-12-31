using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class User
{
    public int Userid { get; set; }

    public string? Email { get; set; }

    public string? Lastname { get; set; }

    public string Login { get; set; } = null!;

    public string? Name { get; set; }

    public string Password { get; set; } = null!;

    public string? Secret { get; set; }

    public int? PositionPositionid { get; set; }

    public virtual ICollection<Leaverequest> LeaverequestDecideuserUsers { get; set; } = new List<Leaverequest>();

    public virtual ICollection<Leaverequest> LeaverequestUserUsers { get; set; } = new List<Leaverequest>();

    public virtual ICollection<Leaverequesthistory> Leaverequesthistories { get; set; } = new List<Leaverequesthistory>();

    public virtual ICollection<Passwordresettoken> Passwordresettokens { get; set; } = new List<Passwordresettoken>();

    public virtual Position? PositionPosition { get; set; }

    public virtual ICollection<Userleavelimit> Userleavelimits { get; set; } = new List<Userleavelimit>();

    public virtual ICollection<Userrole> Userroles { get; set; } = new List<Userrole>();
}
