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
                product.ExpiryDate,
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
            .ToDictionary(group => group.Key, group => group
                .GroupBy(item => item.Date)
                .ToDictionary(day => day.Key, day => day.Sum(item => item.Quantity)));

        var allDays = Enumerable.Range(0, options.HistoryDays)
            .Select(offset => since.AddDays(offset))
            .ToArray();

        var candidateProducts = products
            .Select(product =>
            {
                var dailySales = allDays
                    .Select(day => new DailyDemand(day, salesByProduct.GetValueOrDefault(product.Id)?.GetValueOrDefault(day) ?? 0))
                    .ToArray();

                var sales7 = dailySales.TakeLast(7).Sum(item => item.Quantity);
                var sales30 = dailySales.Sum(item => item.Quantity);
                var avg7 = sales7 == 0 ? 0d : sales7 / 7d;
                var avg30 = sales30 == 0 ? 0d : sales30 / dailySales.Length;
                var trendPercent = avg30 == 0 ? 0d : ((avg7 - avg30) / avg30) * 100d;
                var reorderPoint = Math.Max(10, (int)Math.Ceiling(avg7 * 1.5d));
                var daysOfCover = product.CurrentStock <= 0 ? 0d : product.CurrentStock / Math.Max(avg7, 1d);
                var daysUntilExpiry = product.ExpiryDate.HasValue
                    ? (int)Math.Floor((product.ExpiryDate.Value.Date - now.Date).TotalDays)
                    : int.MaxValue;
                var expiryUrgency = daysUntilExpiry <= 0 ? 50 : daysUntilExpiry <= 7 ? 35 : daysUntilExpiry <= 14 ? 20 : 0;
                var urgencyScore = ((product.CurrentStock <= reorderPoint ? 50 : 0)
                    + (trendPercent > 0 ? Math.Min(trendPercent * 2.5, 40) : 0)
                    + (product.CurrentStock <= 20 ? 20 : 0)
                    + expiryUrgency
                    + (avg7 > avg30 ? 15 : 0));

                return new
                {
                    Product = product,
                    DaysUntilExpiry = daysUntilExpiry,
                    DailySales = dailySales.TakeLast(14).ToArray(),
                    AvgDailySales7 = (int)Math.Ceiling(avg7),
                    AvgDailySales30 = (int)Math.Ceiling(avg30),
                    TrendPercent = Math.Round(trendPercent, 2),
                    ReorderPoint = reorderPoint,
                    DaysOfCover = Math.Round(daysOfCover, 2),
                    UrgencyScore = Math.Round(urgencyScore, 2)
                };
            })
            .Where(item =>
                item.Product.CurrentStock <= Math.Max(20, item.ReorderPoint + 10)
                || item.TrendPercent >= 15
                || item.DaysUntilExpiry <= 14
                || item.AvgDailySales7 >= Math.Max(2, item.AvgDailySales30 * 1.2))
            .OrderByDescending(item => item.UrgencyScore)
            .Take(10)
            .ToArray();

        return new ForecastInput(
            now,
            since,
            now,
            options.ForecastHorizonDays,
            10,
            candidateProducts.Select(item => new ProductDemandInput(
                item.Product.Id,
                item.Product.Name,
                item.Product.ExpiryDate,
                item.DaysUntilExpiry,
                item.Product.CurrentStock,
                item.Product.ReservedStock,
                item.AvgDailySales7,
                item.AvgDailySales30,
                item.TrendPercent,
                item.ReorderPoint,
                item.DaysOfCover,
                item.UrgencyScore,
                item.DailySales)).ToArray());
    }
}
