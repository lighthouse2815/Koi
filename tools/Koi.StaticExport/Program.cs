using Koi.StaticExport;

try
{
    await new SiteExporter(args).RunAsync();
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Xuất FE không thành công: {exception.Message}");
    return 1;
}
