// Usings principais
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;   
using Microsoft.Extensions.Options;        
using Microsoft.Extensions.Hosting;    

// Cria o builder que prepara DI, configura��o, logging e web host
var builder = WebApplication.CreateBuilder(args);

// Registra um cache em mem�ria (usado para armazenar os feeds lidos do ThingSpeak)
builder.Services.AddMemoryCache();

// Registra um HttpClient nomeado "ts" com BaseAddress apontando para a API do ThingSpeak
builder.Services.AddHttpClient("ts", client => client.BaseAddress = new Uri("https://api.thingspeak.com/"));

// Mapeia a se��o "ThingSpeak" do appsettings.json para o POCO ThingSpeakOptions
builder.Services.Configure<ThingSpeakOptions>(builder.Configuration.GetSection("ThingSpeak"));

// Registra o servi�o em background que far� polling peri�dico no ThingSpeak
builder.Services.AddHostedService<ThingSpeakPoller>();

// Constr�i o app (finaliza DI e pipeline)
var app = builder.Build();

// Habilita servir arquivos est�ticos da pasta wwwroot.
// UseDefaultFiles permite que index.html seja servido sem especificar o nome.
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint simples que retorna os dados em cache (preenchidos pelo ThingSpeakPoller).
app.MapGet("/api/feeds", (IMemoryCache cache) =>
{
    if (cache.TryGetValue<ThingSpeakResponse>("feeds", out var data))
        return Results.Ok(data);
    return Results.NotFound(new { message = "Nenhum dado em cache ainda." }); // Se n�o houver nada em cache retorna 404 com mensagem
});

// Inicia o servidor web
app.Run();


// Classes/POCOs / Options

// Configura��es lidas de appsettings.json: ChannelId, ReadApiKey, PollIntervalSeconds
public record ThingSpeakOptions
{
    public string ChannelId { get; init; } = "";
    public string ReadApiKey { get; init; } = "";
    public int PollIntervalSeconds { get; init; } = 30;
}

// Estrutura esperada do JSON retornado pelo ThingSpeak (simplificada)
public class ThingSpeakResponse
{
    public Channel? channel { get; set; }
    public Feed[]? feeds { get; set; }
}
public class Channel { public int id { get; set; } public string? name { get; set; } }

// Cada feed representa uma linha (registro) retornado pela API do ThingSpeak.
// Observa��o: field1/field2 chegam como strings no JSON � aqui deixei string para evitar erros de parsing direto.
public class Feed
{
    [JsonPropertyName("entry_id")] public int entry_id { get; set; }
    [JsonPropertyName("created_at")] public DateTime created_at { get; set; }
    [JsonPropertyName("field1")] public string? field1 { get; set; } // Umidade (field1)
    [JsonPropertyName("field2")] public string? field2 { get; set; } // Temperatura (field2)
}


// BackgroundService que faz polling no ThingSpeak

public class ThingSpeakPoller : BackgroundService
{
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly IOptions<ThingSpeakOptions> _opts;

    // Depend�ncias injetadas via construtor (DI)
    public ThingSpeakPoller(IHttpClientFactory http, IMemoryCache cache, IOptions<ThingSpeakOptions> opts)
    {
        _http = http;
        _cache = cache;
        _opts = opts;
    }

    // M�todo principal executado quando o host inicia
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Obt�m o HttpClient nomeado "ts"
        var client = _http.CreateClient("ts");

        // Loop de polling at� o CancellationToken ser sinalizado (shutdown)
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Monta a URL para buscar os �ltimos 200 registros do canal configurado
                var url = $"channels/{_opts.Value.ChannelId}/feeds.json?results=200";

                // Se houver ReadApiKey, adiciona ao querystring (necess�rio para canais privados)
                if (!string.IsNullOrEmpty(_opts.Value.ReadApiKey))
                    url += $"&api_key={_opts.Value.ReadApiKey}";

                // Faz a requisi��o (string JSON)
                var txt = await client.GetStringAsync(url, stoppingToken);

                // Desserializa com op��o case-insensitive para corresponder propriedades
                var ts = JsonSerializer.Deserialize<ThingSpeakResponse>(txt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Se vieram feeds, armazena no cache para o endpoint GET consumir
                if (ts?.feeds != null)
                {
                    // Define um tempo de expira��o razo�vel (p. ex. 3x o intervalo de polling ou no m�nimo 60s)
                    _cache.Set("feeds", ts, TimeSpan.FromSeconds(Math.Max(60, _opts.Value.PollIntervalSeconds * 3)));
                }
            }
            catch (Exception ex)
            {
                // Em produ��o prefira ILogger em vez de Console.WriteLine
                Console.WriteLine($"Polling error: {ex.Message}");
            }

            // Espera o intervalo configurado antes de repetir (respeitando cancelamento)
            await Task.Delay(TimeSpan.FromSeconds(_opts.Value.PollIntervalSeconds), stoppingToken);
        }
    }
}
