using System;
using System.Collections.Generic;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class Passwordresettoken
{
    public int Passwordresettokenid { get; set; }

    public DateTime? Expirydate { get; set; }

    public string? Token { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
