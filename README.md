# ?? TSWeb � Dashboard de Temperatura e Umidade via ThingSpeak

Este projeto � um **dashboard em .NET 8** que consome dados da API do **ThingSpeak** (canal fornecido pela turma) e exibe em tempo real os campos:

- ??? **Temperatura** (field2)  
- ?? **Umidade** (field1)  

Os dados s�o consultados periodicamente da API e armazenados em cache, para exibi��o no frontend (gr�fico interativo em HTML/JS).  

---

## ?? Configura��o do Projeto

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
- **ReadApiKey** ? Chave de leitura (se o canal for privado; se for p�blico pode deixar vazio)  
- **PollIntervalSeconds** ? Intervalo em segundos para buscar novos dados  

---

## ?? Como executar

No terminal (dentro da pasta do projeto):

```bash
dotnet restore
dotnet run
```

O servidor ir� rodar em:

- [http://localhost:5000](http://localhost:5000)  
- [https://localhost:7000](https://localhost:7000)  

---

## ?? Endpoints dispon�veis

- `GET /api/feeds` ? Retorna o JSON dos dados de **temperatura e umidade** buscados do ThingSpeak  
- `/` ? Abre o dashboard (HTML/JS) que consome esses dados e exibe no navegador  

---

## ?? Arquitetura do Projeto

- **Program.cs** ? Configura��o principal do ASP.NET Core (rotas, servi�os, cache, background service)  
- **ThingSpeakPoller** ? Servi�o em background que busca dados periodicamente no ThingSpeak  
- **IMemoryCache** ? Mant�m os dados em mem�ria para evitar excesso de chamadas na API  
- **wwwroot/index.html** ? Dashboard frontend que consome `/api/feeds` e plota os gr�ficos  

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
- Cache em mem�ria (`IMemoryCache`)  
- Frontend simples em HTML + JavaScript  