using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Leaverequeststatus
{
    public int Leaverequeststatusid { get; set; }

    public int State { get; set; }

    public string Status { get; set; } = null!;

    public bool Visibility { get; set; }

    public virtual ICollection<Leaverequesthistory> Leaverequesthistories { get; set; } = new List<Leaverequesthistory>();

    public virtual ICollection<Leaverequest> Leaverequests { get; set; } = new List<Leaverequest>();
}
