using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.InventoryForecasting;

namespace SMA.API.Services.ServiceImplementation;

public sealed class ForecastDataSource(AppDbContext context) : IForecastDataSource
{
    public async Task<ForecastInput> GetInputAsync(DateTime now, ForecastOptions options, CancellationToken cancellationToken)
    {
        var since = now.Date.AddDays(-options.HistoryDays + 1);
        var products = await context.Products
            .Where(product => product.IsActive)
            .Select(product => new
            {
                product.Id,
                product.Name,
                CurrentStock = product.Inventory == null ? 0 : product.Inventory.QuantityAvailable,
                ReservedStock = product.Inventory == null ? 0 : product.Inventory.QuantityReserved
            })
            .ToListAsync(cancellationToken);

        var sales = await context.OrderItems
            .Where(item => item.Order != null &&
                (item.Order.Status == "Paid" || item.Order.Status == "Placed") &&
                item.Order.CreatedAt >= since && item.Order.CreatedAt <= now)
            .Select(item => new { item.ProductId, Date = item.Order!.CreatedAt.Date, item.Quantity })
            .ToListAsync(cancellationToken);

        var salesByProduct = sales
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.GroupBy(item => item.Date)
                .ToDictionary(day => day.Key, day => day.Sum(item => item.Quantity)));
        var days = Enumerable.Range(0, options.HistoryDays).Select(offset => since.AddDays(offset)).ToArray();

        return new ForecastInput(
            now, since, now, options.ForecastHorizonDays,
            products.Select(product => new ProductDemandInput(
                product.Id,
                product.Name,
                product.CurrentStock,
                product.ReservedStock,
                days.Select(day => new DailyDemand(day, salesByProduct.GetValueOrDefault(product.Id)?.GetValueOrDefault(day) ?? 0)).ToArray()))
                .ToArray());
    }
}
