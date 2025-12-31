using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Leaverequesthistory
{
    public int Leaverequesthistoryid { get; set; }

    public DateTime Date { get; set; }

    public string? Decisiondescription { get; set; }

    public string? Description { get; set; }

    public int LeaverequestLeaverequestid { get; set; }

    public int StatusLeaverequeststatusid { get; set; }

    public int UserUserid { get; set; }

    public virtual Leaverequest LeaverequestLeaverequest { get; set; } = null!;

    public virtual Leaverequeststatus StatusLeaverequeststatus { get; set; } = null!;

    public virtual User UserUser { get; set; } = null!;
}
