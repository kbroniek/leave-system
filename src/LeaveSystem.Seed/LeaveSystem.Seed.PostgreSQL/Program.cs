// See https://aka.ms/new-console-template for more information
using LeaveSystem.Seed.PostgreSQL.Model;
using Microsoft.EntityFrameworkCore;

var context = new OmbContext();
var leaveTypes = await context.Leavetypes.ToListAsync();
