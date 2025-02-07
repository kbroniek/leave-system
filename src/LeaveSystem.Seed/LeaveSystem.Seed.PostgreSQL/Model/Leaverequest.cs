using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Leaverequest
{
    public int Leaverequestid { get; set; }

    public string? Decisiondescription { get; set; }

    public string? Description { get; set; }

    public DateOnly Endday { get; set; }

    public int Hours { get; set; }

    public DateTime? Modificationdate { get; set; }

    public DateOnly Startday { get; set; }

    public DateOnly? Submissiondate { get; set; }

    public int Year { get; set; }

    public int? DecideuserUserid { get; set; }

    public int LeavetypeLeavetypeid { get; set; }

    public int StatusLeaverequeststatusid { get; set; }

    public int UserUserid { get; set; }

    public virtual User? DecideuserUser { get; set; }

    public virtual ICollection<Leaverequesthistory> Leaverequesthistories { get; set; } = new List<Leaverequesthistory>();

    public virtual Leavetype LeavetypeLeavetype { get; set; } = null!;

    public virtual Leaverequeststatus StatusLeaverequeststatus { get; set; } = null!;

    public virtual User UserUser { get; set; } = null!;
}
