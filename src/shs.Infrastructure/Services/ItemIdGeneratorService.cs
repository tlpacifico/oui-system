using Microsoft.EntityFrameworkCore;
using shs.Infrastructure.Database;

namespace shs.Infrastructure.Services;

public interface IItemIdGeneratorService
{
    Task<string> GenerateNextIdAsync(long? supplierId, CancellationToken ct);
}

public class ItemIdGeneratorService : IItemIdGeneratorService
{
    private readonly ShsDbContext _db;

    public ItemIdGeneratorService(ShsDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateNextIdAsync(long? supplierId, CancellationToken ct)
    {
        var prefix = "OUI"; // Default for own-purchase

        if (supplierId.HasValue)
        {
            var supplier = await _db.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == supplierId.Value)
                .Select(s => s.Initial)
                .FirstOrDefaultAsync(ct);

            if (supplier == null)
                throw new InvalidOperationException("Supplier not found");

            prefix = supplier;
        }

        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

        // Get the last item for this prefix+yearMonth
        var lastItem = await _db.Items
            .AsNoTracking()
            .Where(i => i.IdentificationNumber.StartsWith(prefix + yearMonth))
            .OrderByDescending(i => i.IdentificationNumber)
            .Select(i => i.IdentificationNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastItem != null)
        {
            // Extract sequence from "M202602-0001"
            var parts = lastItem.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out var lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}{yearMonth}-{sequence:D4}";
    }
}
