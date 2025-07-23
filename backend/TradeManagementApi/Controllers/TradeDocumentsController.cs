using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeManagementApi.Data;
using TradeManagementApi.Models;

namespace TradeManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradeDocumentsController : ControllerBase
{
    private readonly TradeDbContext _context;

    public TradeDocumentsController(TradeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TradeDocument>>> GetDocuments()
    {
        Console.WriteLine("📄 API调用: GET /api/tradedocuments - 从数据库查询");
        
        var documents = await _context.TradeDocuments
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
            
        Console.WriteLine($"✅ 返回 {documents.Count} 个文档");
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TradeDocument>> GetDocument(int id)
    {
        Console.WriteLine($"📄 API调用: GET /api/tradedocuments/{id} - 从数据库查询");
        
        var document = await _context.TradeDocuments
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            Console.WriteLine($"❌ 文档 {id} 未找到");
            return NotFound();
        }

        Console.WriteLine($"✅ 返回文档: {document.DocumentType}");
        return Ok(document);
    }

    [HttpPost]
    public async Task<ActionResult<TradeDocument>> CreateDocument(TradeDocument document)
    {
        Console.WriteLine("📝 API调用: POST /api/tradedocuments - 保存到数据库");
        
        document.CreatedDate = DateTime.UtcNow;
        
        _context.TradeDocuments.Add(document);
        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ 文档创建成功: ID {document.Id}");
        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocument(int id, TradeDocument document)
    {
        if (id != document.Id)
        {
            return BadRequest();
        }

        _context.Entry(document).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ 文档 {id} 更新成功");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await DocumentExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _context.TradeDocuments.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        _context.TradeDocuments.Remove(document);
        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ 文档 {id} 删除成功");
        return NoContent();
    }

    private async Task<bool> DocumentExists(int id)
    {
        return await _context.TradeDocuments.AnyAsync(e => e.Id == id);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ComplianceAlertsController : ControllerBase
{
    private readonly TradeDbContext _context;

    public ComplianceAlertsController(TradeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComplianceAlert>>> GetAlerts()
    {
        Console.WriteLine("🚨 API调用: GET /api/compliancealerts - 从数据库查询");
        
        var alerts = await _context.ComplianceAlerts
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
            
        Console.WriteLine($"✅ 返回 {alerts.Count} 个警报");
        return Ok(alerts);
    }
}