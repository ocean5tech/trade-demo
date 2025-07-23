using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradeManagementApi.Models;

namespace TradeManagementApi.Data;

// 继承IdentityDbContext以支持用户认证
public class TradeDbContext : IdentityDbContext<ApplicationUser>
{
    public TradeDbContext(DbContextOptions<TradeDbContext> options) : base(options)
    {
    }

    public DbSet<TradeDocument> TradeDocuments { get; set; }
    public DbSet<ComplianceAlert> ComplianceAlerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // 重要：调用基类方法

        // 配置TradeDocument
        modelBuilder.Entity<TradeDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // 索引优化
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.CreatedDate);
        });

        // 配置ComplianceAlert
        modelBuilder.Entity<ComplianceAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // 外键关系
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.RelatedAlerts)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 种子数据
        SeedData(modelBuilder);
    }
private static void SeedData(ModelBuilder modelBuilder)
{
    // 使用固定的静态日期而不是DateTime.Now
    var baseDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    // 插入示例贸易文档 - 使用静态日期
    modelBuilder.Entity<TradeDocument>().HasData(
        new TradeDocument
        {
            Id = 1,
            DocumentType = "Export License",
            Country = "China",
            Status = "Approved",
            CreatedDate = baseDate.AddDays(-5), // 固定日期：2025-01-10
            CompanyName = "Global Electronics Ltd",
            Value = 125000m,
            RiskLevel = "Low"
        },
        new TradeDocument
        {
            Id = 2,
            DocumentType = "Import Permit",
            Country = "Germany", 
            Status = "Pending",
            CreatedDate = baseDate.AddDays(-3), // 固定日期：2025-01-12
            CompanyName = "Euro Manufacturing GmbH",
            Value = 89000m,
            RiskLevel = "Medium"
        },
        new TradeDocument
        {
            Id = 3,
            DocumentType = "Certificate of Origin",
            Country = "USA",
            Status = "Draft",
            CreatedDate = baseDate.AddDays(-1), // 固定日期：2025-01-14
            CompanyName = "American Trade Corp",
            Value = 156000m,
            RiskLevel = "High"
        },
        new TradeDocument
        {
            Id = 4,
            DocumentType = "Customs Declaration",
            Country = "Japan",
            Status = "Approved",
            CreatedDate = baseDate, // 固定日期：2025-01-15
            CompanyName = "Pacific Trading Co",
            Value = 67000m,
            RiskLevel = "Low"
        }
    );

    // 插入示例合规警报 - 使用静态日期
    modelBuilder.Entity<ComplianceAlert>().HasData(
        new ComplianceAlert
        {
            Id = 1,
            Type = "warning",
            Message = "文档 #2 需要额外的合规审查",
            DocumentId = 2,
            CreatedDate = baseDate.AddHours(-2) // 固定时间
        },
        new ComplianceAlert
        {
            Id = 2,
            Type = "error", 
            Message = "检测到文档 #3 的高风险交易",
            DocumentId = 3,
            CreatedDate = baseDate.AddHours(-1) // 固定时间
        },
        new ComplianceAlert
        {
            Id = 3,
            Type = "info",
            Message = "下个月起生效新的贸易法规",
            DocumentId = null,
            CreatedDate = baseDate // 固定时间
        }
    );
}
}