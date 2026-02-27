using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using shs.Import.Services;
using shs.Infrastructure;
using shs.Infrastructure.Database;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddInfrastructure(context.Configuration);
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddScoped<ExcelEstoqueReader>();
        services.AddScoped<ExcelConsignadosReader>();
        services.AddScoped<ImportService>();
    })
    .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
// Diretório base: subir de bin/Debug/net9.0 para a raiz do repositório (5 níveis)
var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var defaultEstoque = Path.Combine(repoRoot, "docs", "dados-reais", "Personal_items_to_import.xlsx");
var defaultConsignados = Path.Combine(repoRoot, "docs", "dados-reais", "Itens Consignados_to_import.xlsx");

var estoquePath = config["Import:PersonalItemsPath"] ?? defaultEstoque;
var consignadosPath = config["Import:ConsignadosPath"] ?? defaultConsignados;

// Resolver caminhos relativos em relação ao repo root
if (!Path.IsPathRooted(estoquePath))
    estoquePath = Path.Combine(repoRoot, estoquePath);
if (!Path.IsPathRooted(consignadosPath))
    consignadosPath = Path.Combine(repoRoot, consignadosPath);

estoquePath = Path.GetFullPath(estoquePath);
consignadosPath = Path.GetFullPath(consignadosPath);

Console.WriteLine("=== OUI System - Importação de Dados ===");
Console.WriteLine();
Console.WriteLine("Arquivos:");
Console.WriteLine("  Itens Pessoais: {0}", estoquePath);
Console.WriteLine("  Consignados:    {0}", consignadosPath);
Console.WriteLine();

if (!File.Exists(estoquePath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERRO: Arquivo de itens pessoais não encontrado: {0}", estoquePath);
    Console.ResetColor();
    return 1;
}

if (!File.Exists(consignadosPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERRO: Arquivo de consignados não encontrado.");
    Console.ResetColor();
    return 1;
}

var forceImport = args.Contains("--yes") || args.Contains("-y");
if (!forceImport)
{
    Console.WriteLine("AVISO: Faça backup da base de dados antes de continuar.");
    Console.Write("Deseja prosseguir com a importação? (s/N): ");
    try
    {
        var key = Console.ReadKey(true);
        Console.WriteLine();
        if (key.Key != ConsoleKey.S && key.KeyChar != 's' && key.KeyChar != 'S')
        {
            Console.WriteLine("Importação cancelada.");
            return 0;
        }
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine();
        Console.WriteLine("Use --yes ou -y para executar sem confirmação interativa.");
        return 1;
    }
}

Console.WriteLine();
Console.WriteLine("Iniciando importação...");
Console.WriteLine();

using var scope = host.Services.CreateScope();
var importService = scope.ServiceProvider.GetRequiredService<ImportService>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    var result = await importService.ImportAsync(estoquePath, consignadosPath);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("=== Importação concluída com sucesso ===");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Resumo:");
    Console.WriteLine("  Linhas lidas (pessoais):    {0}", result.EstoqueRowsRead);
    Console.WriteLine("  Linhas lidas (consignados): {0}", result.ConsignadoRowsRead);
    Console.WriteLine("  Marcas criadas:             {0}", result.BrandsCreated);
    Console.WriteLine("  Fornecedores criados:       {0}", result.SuppliersCreated);
    Console.WriteLine("  Itens importados (pessoais):{0}", result.ItemsFromEstoque);
    Console.WriteLine("  Itens importados (consign.):{0}", result.ItemsFromConsignados);
    Console.WriteLine("  Erros:                       {0}", result.Errors);
    Console.WriteLine();

    return result.Errors > 0 ? 1 : 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Falha na importação");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERRO: {0}", ex.Message);
    Console.ResetColor();
    return 1;
}
