using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoMaster.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    static ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> GenerateDailyPdfAsync(DateTime date)
    {
        var from = date.Date;
        var to = from.AddDays(1);
        var orders = await _db.Orders.Include(o => o.Customer).Include(o => o.Vehicle)
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to).ToListAsync();
        return BuildReport($"Дневной отчёт — {from:dd.MM.yyyy}", orders);
    }

    public async Task<byte[]> GenerateWeeklyPdfAsync(DateTime weekEnd)
    {
        var to = weekEnd.Date.AddDays(1);
        var from = to.AddDays(-7);
        var orders = await _db.Orders.Include(o => o.Customer).Include(o => o.Vehicle)
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to).ToListAsync();
        return BuildReport($"Недельный отчёт — {from:dd.MM.yyyy}–{to.AddDays(-1):dd.MM.yyyy}", orders);
    }

    private static byte[] BuildReport(string title, List<Domain.Entities.Order> orders)
    {
        var totalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalCost);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(title).FontSize(18).Bold();

                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Заказов: {orders.Count}    Выручка: {totalRevenue:N0} TJS");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("№").Bold();
                            header.Cell().Text("Клиент").Bold();
                            header.Cell().Text("Авто").Bold();
                            header.Cell().Text("Статус").Bold();
                            header.Cell().Text("Сумма").Bold();
                        });

                        foreach (var o in orders)
                        {
                            table.Cell().Text(o.OrderNumber);
                            table.Cell().Text(o.Customer?.FullName ?? "—");
                            table.Cell().Text(o.Vehicle == null ? "—" : $"{o.Vehicle.Make} {o.Vehicle.Model}");
                            table.Cell().Text(o.Status.ToString());
                            table.Cell().Text($"{o.TotalCost:N0} TJS");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("AutoMaster CRM · ");
                    t.Span(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }
}
