using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradeManagementApi.Models;

/// <summary>
/// 贸易文档实体 - 改进的 Null Safety 版本
/// </summary>
public class TradeDocument
{
    /// <summary>
    /// 文档唯一标识
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 文档类型 (如: Export License, Import Permit 等)
    /// </summary>
    [Required(ErrorMessage = "文档类型不能为空")]
    [MaxLength(100, ErrorMessage = "文档类型长度不能超过100字符")]
    public string DocumentType { get; set; } = string.Empty;
    
    /// <summary>
    /// 贸易国家
    /// </summary>
    [Required(ErrorMessage = "国家不能为空")]
    [MaxLength(50, ErrorMessage = "国家名称长度不能超过50字符")]
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// 文档状态
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // 默认为草稿状态
    
    /// <summary>
    /// 创建时间 (UTC)
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }
    
    /// <summary>
    /// 公司名称
    /// </summary>
    [Required(ErrorMessage = "公司名称不能为空")]
    [MaxLength(200, ErrorMessage = "公司名称长度不能超过200字符")]
    public string CompanyName { get; set; } = string.Empty;
    
    /// <summary>
    /// 贸易价值 (美元)
    /// </summary>
    [Column(TypeName = "decimal(15,2)")]
    [Range(0.01, 999999999999.99, ErrorMessage = "贸易价值必须大于0")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal Value { get; set; }
    
    /// <summary>
    /// 风险等级
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string RiskLevel { get; set; } = "Low"; // 默认为低风险
    
    /// <summary>
    /// 备注信息
    /// </summary>
    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000字符")]
    public string? Notes { get; set; } // 可为空
    
    /// <summary>
    /// 创建者 (用户邮箱)
    /// </summary>
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// 文档过期时间
    /// </summary>
    public DateTime? ExpiryDate { get; set; } // 可为空
    
    /// <summary>
    /// 是否已删除 (软删除标记)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedDate { get; set; } // 可为空
    
    /// <summary>
    /// 关联的合规警报 (导航属性)
    /// </summary>
    public virtual ICollection<ComplianceAlert> RelatedAlerts { get; set; } = new List<ComplianceAlert>();
}

/// <summary>
/// 合规警报实体 - 改进的 Null Safety 版本
/// </summary>
public class ComplianceAlert
{
    /// <summary>
    /// 警报唯一标识
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 警报类型 (warning, error, info)
    /// </summary>
    [Required(ErrorMessage = "警报类型不能为空")]
    [MaxLength(20)]
    public string Type { get; set; } = "info"; // 默认为信息类型
    
    /// <summary>
    /// 警报消息内容
    /// </summary>
    [Required(ErrorMessage = "警报消息不能为空")]
    [MaxLength(500, ErrorMessage = "警报消息长度不能超过500字符")]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 关联的文档ID (可选)
    /// </summary>
    public int? DocumentId { get; set; } // 可为空，表示系统级警报
    
    /// <summary>
    /// 警报创建时间
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 警报是否已读
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// 警报严重级别 (1-5, 1最低, 5最高)
    /// </summary>
    [Range(1, 5, ErrorMessage = "严重级别必须在1-5之间")]
    public int Severity { get; set; } = 1; // 默认为最低级别
    
    /// <summary>
    /// 警报来源
    /// </summary>
    [MaxLength(50)]
    public string Source { get; set; } = "System"; // 默认为系统生成
    
    /// <summary>
    /// 是否已删除 (软删除标记)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// 关联的文档 (导航属性)
    /// </summary>
    public virtual TradeDocument? Document { get; set; } // 可为空
}

/// <summary>
/// 审计实体接口 - 用于统一处理审计字段
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }
    DateTime? LastModifiedDate { get; set; }
    string CreatedBy { get; set; }
    string? ModifiedBy { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DeletedDate { get; set; }
}

/// <summary>
/// 文档状态枚举 - 类型安全的状态管理
/// </summary>
public enum DocumentStatus
{
    /// <summary>草稿</summary>
    Draft = 0,
    
    /// <summary>待审批</summary>
    Pending = 1,
    
    /// <summary>已批准</summary>
    Approved = 2,
    
    /// <summary>已拒绝</summary>
    Rejected = 3,
    
    /// <summary>已过期</summary>
    Expired = 4,
    
    /// <summary>已归档</summary>
    Archived = 5
}

/// <summary>
/// 风险等级枚举 - 类型安全的风险管理
/// </summary>
public enum RiskLevel
{
    /// <summary>低风险</summary>
    Low = 0,
    
    /// <summary>中等风险</summary>
    Medium = 1,
    
    /// <summary>高风险</summary>
    High = 2,
    
    /// <summary>极高风险</summary>
    Critical = 3
}

/// <summary>
/// 警报类型枚举
/// </summary>
public enum AlertType
{
    /// <summary>信息</summary>
    Info = 0,
    
    /// <summary>警告</summary>
    Warning = 1,
    
    /// <summary>错误</summary>
    Error = 2,
    
    /// <summary>严重错误</summary>
    Critical = 3
}

/// <summary>
/// 贸易文档 DTO - 用于 API 响应
/// </summary>
public class TradeDocumentDto
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string FormattedValue => Value.ToString("C"); // 格式化货币显示
    public string RiskLevel { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public List<ComplianceAlertDto> RelatedAlerts { get; set; } = new();
}

/// <summary>
/// 合规警报 DTO - 用于 API 响应
/// </summary>
public class ComplianceAlertDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? DocumentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }
    public int Severity { get; set; }
    public string Source { get; set; } = string.Empty;
    public string SeverityText => Severity switch
    {
        1 => "很低",
        2 => "低",
        3 => "中等", 
        4 => "高",
        5 => "很高",
        _ => "未知"
    };
}

/// <summary>
/// 创建贸易文档 DTO - 用于 API 请求
/// </summary>
public class CreateTradeDocumentDto
{
    [Required(ErrorMessage = "文档类型不能为空")]
    [MaxLength(100, ErrorMessage = "文档类型长度不能超过100字符")]
    public string DocumentType { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "国家不能为空")]
    [MaxLength(50, ErrorMessage = "国家名称长度不能超过50字符")]
    public string Country { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "公司名称不能为空")]
    [MaxLength(200, ErrorMessage = "公司名称长度不能超过200字符")]
    public string CompanyName { get; set; } = string.Empty;
    
    [Range(0.01, 999999999999.99, ErrorMessage = "贸易价值必须大于0")]
    public decimal Value { get; set; }
    
    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000字符")]
    public string? Notes { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// 更新贸易文档 DTO - 用于 API 请求
/// </summary>
public class UpdateTradeDocumentDto
{
    [Required(ErrorMessage = "文档类型不能为空")]
    [MaxLength(100, ErrorMessage = "文档类型长度不能超过100字符")]
    public string DocumentType { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "国家不能为空")]
    [MaxLength(50, ErrorMessage = "国家名称长度不能超过50字符")]
    public string Country { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "公司名称不能为空")]
    [MaxLength(200, ErrorMessage = "公司名称长度不能超过200字符")]
    public string CompanyName { get; set; } = string.Empty;
    
    [Range(0.01, 999999999999.99, ErrorMessage = "贸易价值必须大于0")]
    public decimal Value { get; set; }
    
    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000字符")]
    public string? Notes { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// 分页结果 DTO - 通用分页响应
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
    public int StartIndex => (PageNumber - 1) * PageSize + 1;
    public int EndIndex => Math.Min(PageNumber * PageSize, TotalCount);
}

/// <summary>
/// API 标准响应格式
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 查询过滤器 - 用于搜索和过滤
/// </summary>
public class DocumentQueryFilter
{
    public string? DocumentType { get; set; }
    public string? Country { get; set; }
    public string? Status { get; set; }
    public string? RiskLevel { get; set; }
    public string? CompanyName { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public decimal? ValueFrom { get; set; }
    public decimal? ValueTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedDate";
    public bool SortDescending { get; set; } = true;
}