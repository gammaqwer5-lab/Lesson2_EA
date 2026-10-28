using ExporterStrategies;
var failed = new List<string>();
void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    if (!condition) failed.Add(name);
}
var path = Path.Combine(Path.GetTempPath(), $"report-test-{Guid.NewGuid():N}.txt");
try
{
    IReportExporter fileExporter = new TextFileReportExporter(path);
    var service = new ReportService(fileExporter);
    service.PublishDailySummary(10, 1);
    var contents = File.ReadAllText(path);
    Check("файловый экспорт создаёт файл", File.Exists(path));
    Check("файл содержит заголовок", contents.Contains("Сводка за день"));
    Check("файл содержит значения отчёта", contents.Contains("Всего задач: 10") && contents.Contains("Ошибок: 1"));
}
finally { if (File.Exists(path)) File.Delete(path); }

// TODO 7: Добавьте проверку LegacyReportAdapter через Console.Out.
var originalOut = Console.Out;
var buffer = new StringWriter();
Console.SetOut(buffer);
new ReportService(new LegacyReportAdapter(new LegacyReportPrinter())).PublishDailySummary(10, 1);
Console.SetOut(originalOut);
Check("LegacyReportAdapter через Console.Out", buffer.ToString().Contains("Сводка за день"));

// TODO 8: Докажите, что ReportService не меняется при переключении экспортёра.
var p = Path.GetTempFileName();
originalOut = Console.Out;
buffer = new StringWriter();
Console.SetOut(buffer);
try
{
    new ReportService(new LegacyReportAdapter(new LegacyReportPrinter())).PublishDailySummary(5, 2);
    new ReportService(new TextFileReportExporter(p)).PublishDailySummary(5, 2);
    Check("ReportService работает с обеими реализациями", File.ReadAllText(p).Contains("5"));
}
finally
{
    Console.SetOut(originalOut);
    File.Delete(p);
}
// TODO 9: Проверьте контракт пустого документа.
var emptyPath = Path.GetTempFileName();
new TextFileReportExporter(emptyPath).Export(new ReportDocument("", ""));
Check("пустой документ", File.ReadAllText(emptyPath) == Environment.NewLine);
File.Delete(emptyPath);

// TODO 10: Проверьте ошибочный путь и отсутствие подавления ошибки записи.
var emptyPathErr = false;
try 
{ 
    new TextFileReportExporter(""); 
}
catch (ArgumentException) { emptyPathErr = true; }
Check("пустой путь", emptyPathErr);

Console.WriteLine($"Провалено базовых проверок: {failed.Count} из 3.");
