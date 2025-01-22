using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Seed.PostgreSQL.Model;

public partial class OmbContext : DbContext
{
    public OmbContext()
    {
    }

    public OmbContext(DbContextOptions<OmbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Leaverequest> Leaverequests { get; set; }

    public virtual DbSet<Leaverequesthistory> Leaverequesthistories { get; set; }

    public virtual DbSet<Leaverequeststatus> Leaverequeststatuses { get; set; }

    public virtual DbSet<Leavetype> Leavetypes { get; set; }

    public virtual DbSet<Leavetypegroup> Leavetypegroups { get; set; }

    public virtual DbSet<Passwordresettoken> Passwordresettokens { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userleavelimit> Userleavelimits { get; set; }

    public virtual DbSet<Userrole> Userroles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'omb'; PASSWORD = '123456'; USER ID = 'postgres'");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Leaverequest>(entity =>
        {
            entity.HasKey(e => e.Leaverequestid).HasName("leaverequest_pkey");

            entity.ToTable("leaverequest");

            entity.Property(e => e.Leaverequestid).HasColumnName("leaverequestid");
            entity.Property(e => e.DecideuserUserid).HasColumnName("decideuser_userid");
            entity.Property(e => e.Decisiondescription)
                .HasMaxLength(255)
                .HasColumnName("decisiondescription");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Endday).HasColumnName("endday");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.LeavetypeLeavetypeid).HasColumnName("leavetype_leavetypeid");
            entity.Property(e => e.Modificationdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modificationdate");
            entity.Property(e => e.Startday).HasColumnName("startday");
            entity.Property(e => e.StatusLeaverequeststatusid).HasColumnName("status_leaverequeststatusid");
            entity.Property(e => e.Submissiondate).HasColumnName("submissiondate");
            entity.Property(e => e.UserUserid).HasColumnName("user_userid");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.DecideuserUser).WithMany(p => p.LeaverequestDecideuserUsers)
                .HasForeignKey(d => d.DecideuserUserid)
                .HasConstraintName("fk14psnwfse2jwf45s5gm7lnjw");

            entity.HasOne(d => d.LeavetypeLeavetype).WithMany(p => p.Leaverequests)
                .HasForeignKey(d => d.LeavetypeLeavetypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk60qwtq0y0536s7phwmia1en2l");

            entity.HasOne(d => d.StatusLeaverequeststatus).WithMany(p => p.Leaverequests)
                .HasForeignKey(d => d.StatusLeaverequeststatusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk592pqebq8rmr9hnwb8c710b4q");

            entity.HasOne(d => d.UserUser).WithMany(p => p.LeaverequestUserUsers)
                .HasForeignKey(d => d.UserUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk8oub93g7ltiwwt2cfriuc49cn");
        });

        modelBuilder.Entity<Leaverequesthistory>(entity =>
        {
            entity.HasKey(e => e.Leaverequesthistoryid).HasName("leaverequesthistory_pkey");

            entity.ToTable("leaverequesthistory");

            entity.Property(e => e.Leaverequesthistoryid).HasColumnName("leaverequesthistoryid");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Decisiondescription)
                .HasMaxLength(255)
                .HasColumnName("decisiondescription");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.LeaverequestLeaverequestid).HasColumnName("leaverequest_leaverequestid");
            entity.Property(e => e.StatusLeaverequeststatusid).HasColumnName("status_leaverequeststatusid");
            entity.Property(e => e.UserUserid).HasColumnName("user_userid");

            entity.HasOne(d => d.LeaverequestLeaverequest).WithMany(p => p.Leaverequesthistories)
                .HasForeignKey(d => d.LeaverequestLeaverequestid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkqqyyjeafbrfulx8al7edp5w92");

            entity.HasOne(d => d.StatusLeaverequeststatus).WithMany(p => p.Leaverequesthistories)
                .HasForeignKey(d => d.StatusLeaverequeststatusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk1xt8u0vbingglm26ov24m8y5b");

            entity.HasOne(d => d.UserUser).WithMany(p => p.Leaverequesthistories)
                .HasForeignKey(d => d.UserUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk9sglv5ep2901vgxju6l5bkb58");
        });

        modelBuilder.Entity<Leaverequeststatus>(entity =>
        {
            entity.HasKey(e => e.Leaverequeststatusid).HasName("leaverequeststatus_pkey");

            entity.ToTable("leaverequeststatus");

            entity.HasIndex(e => e.Status, "uk_4j0qmnlyidfk22xhayvg55t5f").IsUnique();

            entity.HasIndex(e => e.State, "uk_9elx39emftdxtjtrfjs3jn5o8").IsUnique();

            entity.Property(e => e.Leaverequeststatusid).HasColumnName("leaverequeststatusid");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Visibility)
                .HasDefaultValue(true)
                .HasColumnName("visibility");
        });

        modelBuilder.Entity<Leavetype>(entity =>
        {
            entity.HasKey(e => e.Leavetypeid).HasName("leavetype_pkey");

            entity.ToTable("leavetype");

            entity.HasIndex(e => e.Abbreviation, "uk_fr16mfshytuaoxjnqfencnoqq").IsUnique();

            entity.Property(e => e.Leavetypeid).HasColumnName("leavetypeid");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(255)
                .HasColumnName("abbreviation");
            entity.Property(e => e.Attachmentrequired)
                .HasDefaultValue(false)
                .HasColumnName("attachmentrequired");
            entity.Property(e => e.Defaultlimit).HasColumnName("defaultlimit");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Includefreeday).HasColumnName("includefreeday");
            entity.Property(e => e.LeavetypegroupLeavetypegroupid).HasColumnName("leavetypegroup_leavetypegroupid");

            entity.HasOne(d => d.LeavetypegroupLeavetypegroup).WithMany(p => p.Leavetypes)
                .HasForeignKey(d => d.LeavetypegroupLeavetypegroupid)
                .HasConstraintName("fkv3gu7jsgs12txcwdh1frgsu2");

            entity.HasMany(d => d.LeavelimitLeavelimits).WithMany(p => p.LeavetypeLeavetypes)
                .UsingEntity<Dictionary<string, object>>(
                    "Leavetypelimitconstraint",
                    r => r.HasOne<Leavetype>().WithMany()
                        .HasForeignKey("LeavelimitLeavelimitid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk2rf6cvptg8bf2290nm88o89wy"),
                    l => l.HasOne<Leavetype>().WithMany()
                        .HasForeignKey("LeavetypeLeavetypeid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk81iy1sd1435swnsu9grpyc2vb"),
                    j =>
                    {
                        j.HasKey("LeavelimitLeavelimitid", "LeavetypeLeavetypeid").HasName("leavetypelimitconstraints_pkey");
                        j.ToTable("leavetypelimitconstraints");
                        j.IndexerProperty<int>("LeavelimitLeavelimitid").HasColumnName("leavelimit_leavelimitid");
                        j.IndexerProperty<int>("LeavetypeLeavetypeid").HasColumnName("leavetype_leavetypeid");
                    });

            entity.HasMany(d => d.LeavetypeLeavetypes).WithMany(p => p.LeavelimitLeavelimits)
                .UsingEntity<Dictionary<string, object>>(
                    "Leavetypelimitconstraint",
                    r => r.HasOne<Leavetype>().WithMany()
                        .HasForeignKey("LeavetypeLeavetypeid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk81iy1sd1435swnsu9grpyc2vb"),
                    l => l.HasOne<Leavetype>().WithMany()
                        .HasForeignKey("LeavelimitLeavelimitid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk2rf6cvptg8bf2290nm88o89wy"),
                    j =>
                    {
                        j.HasKey("LeavelimitLeavelimitid", "LeavetypeLeavetypeid").HasName("leavetypelimitconstraints_pkey");
                        j.ToTable("leavetypelimitconstraints");
                        j.IndexerProperty<int>("LeavelimitLeavelimitid").HasColumnName("leavelimit_leavelimitid");
                        j.IndexerProperty<int>("LeavetypeLeavetypeid").HasColumnName("leavetype_leavetypeid");
                    });
        });

        modelBuilder.Entity<Leavetypegroup>(entity =>
        {
            entity.HasKey(e => e.Leavetypegroupid).HasName("leavetypegroup_pkey");

            entity.ToTable("leavetypegroup");

            entity.Property(e => e.Leavetypegroupid).HasColumnName("leavetypegroupid");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Passwordresettoken>(entity =>
        {
            entity.HasKey(e => e.Passwordresettokenid).HasName("passwordresettoken_pkey");

            entity.ToTable("passwordresettoken");

            entity.Property(e => e.Passwordresettokenid).HasColumnName("passwordresettokenid");
            entity.Property(e => e.Expirydate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expirydate");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Passwordresettokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkg3fbfo1tc9louotgq5r940avr");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Positionid).HasName("position_pkey");

            entity.ToTable("position");

            entity.Property(e => e.Positionid).HasColumnName("positionid");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Lastname)
                .HasMaxLength(255)
                .HasColumnName("lastname");
            entity.Property(e => e.Login)
                .HasMaxLength(255)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PositionPositionid).HasColumnName("position_positionid");
            entity.Property(e => e.Secret)
                .HasMaxLength(255)
                .HasColumnName("secret");

            entity.HasOne(d => d.PositionPosition).WithMany(p => p.Users)
                .HasForeignKey(d => d.PositionPositionid)
                .HasConstraintName("fk4acnot4a33fldhgtqqedryby0");
        });

        modelBuilder.Entity<Userleavelimit>(entity =>
        {
            entity.HasKey(e => e.Userleavelimitid).HasName("userleavelimit_pkey");

            entity.ToTable("userleavelimit");

            entity.Property(e => e.Userleavelimitid).HasColumnName("userleavelimitid");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.LeavetypeLeavetypeid).HasColumnName("leavetype_leavetypeid");
            entity.Property(e => e.Overduelimit).HasColumnName("overduelimit");
            entity.Property(e => e.UserLimit).HasColumnName("user_limit");
            entity.Property(e => e.UserUserid).HasColumnName("user_userid");
            entity.Property(e => e.Validsince).HasColumnName("validsince");
            entity.Property(e => e.Validuntil).HasColumnName("validuntil");

            entity.HasOne(d => d.LeavetypeLeavetype).WithMany(p => p.Userleavelimits)
                .HasForeignKey(d => d.LeavetypeLeavetypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkt13s5ikhorxcv9ik1rrwuwtxa");

            entity.HasOne(d => d.UserUser).WithMany(p => p.Userleavelimits)
                .HasForeignKey(d => d.UserUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkjejvltu1pfgmatp169chxiq4f");
        });

        modelBuilder.Entity<Userrole>(entity =>
        {
            entity.HasKey(e => e.Userroleid).HasName("userrole_pkey");

            entity.ToTable("userrole");

            entity.Property(e => e.Userroleid).HasColumnName("userroleid");
            entity.Property(e => e.Role)
                .HasMaxLength(255)
                .HasColumnName("role");
            entity.Property(e => e.UserUserid).HasColumnName("user_userid");

            entity.HasOne(d => d.UserUser).WithMany(p => p.Userroles)
                .HasForeignKey(d => d.UserUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkawjkkakkec29sipjb8s5khrf2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
