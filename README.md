# ?? TSWeb – Dashboard de Temperatura e Umidade via ThingSpeak

Este projeto é um **dashboard em .NET 8** que consome dados da API do **ThingSpeak** (canal fornecido pela turma) e exibe em tempo real os campos:

- ??? **Temperatura** (field2)  
- ?? **Umidade** (field1)  

Os dados são consultados periodicamente da API e armazenados em cache, para exibição no frontend (gráfico interativo em HTML/JS).  

---

## ?? Configuração do Projeto

1. Clone ou extraia o projeto.  
   ```
   TSWeb/
    ?? Program.cs
    ?? TSWeb.csproj
    ?? appsettings.json
    ?? wwwroot/
        ?? index.html
   ```

2. Crie o arquivo `appsettings.json` com os dados do seu canal no ThingSpeak:  

```json
{
  "ThingSpeak": {
    "ChannelId": "00000",
    "ReadApiKey": "AAA0A00A0",
    "PollIntervalSeconds": 30
  }
}
```

- **ChannelId** ? ID do canal do ThingSpeak  
- **ReadApiKey** ? Chave de leitura (se o canal for privado; se for público pode deixar vazio)  
- **PollIntervalSeconds** ? Intervalo em segundos para buscar novos dados  

---

## ?? Como executar

No terminal (dentro da pasta do projeto):

```bash
dotnet restore
dotnet run
```

O servidor irá rodar em:

- [http://localhost:5000](http://localhost:5000)  
- [https://localhost:7000](https://localhost:7000)  

---

## ?? Endpoints disponíveis

- `GET /api/feeds` ? Retorna o JSON dos dados de **temperatura e umidade** buscados do ThingSpeak  
- `/` ? Abre o dashboard (HTML/JS) que consome esses dados e exibe no navegador  

---

## ?? Arquitetura do Projeto

- **Program.cs** ? Configuração principal do ASP.NET Core (rotas, serviços, cache, background service)  
- **ThingSpeakPoller** ? Serviço em background que busca dados periodicamente no ThingSpeak  
- **IMemoryCache** ? Mantém os dados em memória para evitar excesso de chamadas na API  
- **wwwroot/index.html** ? Dashboard frontend que consome `/api/feeds` e plota os gráficos  

---

## ?? Exemplo de retorno da API

Chamada: `GET http://localhost:5000/api/feeds`  

```json
{
  "channel": {
    "id": 2943258,
    "name": "Canal IoT"
  },
  "feeds": [
    {
      "entry_id": 1,
      "created_at": "2025-09-15T12:30:00Z",
      "field1": "65",
      "field2": "28"
    }
  ]
}
```

---

## ?? Tecnologias usadas

- .NET 8 (Minimal API + Hosted Services)  
- ThingSpeak API (HTTP/JSON)  
- Cache em memória (`IMemoryCache`)  
- Frontend simples em HTML + JavaScript  