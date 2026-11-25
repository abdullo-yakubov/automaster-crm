namespace AutoMaster.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateDailyPdfAsync(DateTime date);
    Task<byte[]> GenerateWeeklyPdfAsync(DateTime weekEnd);
}
