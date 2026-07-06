# Planning Poker

Aplicacao de Planning Poker em ASP.NET Core (.NET 8) com Blazor Server.

## Rodar localmente

1. Restaurar dependencias:

```bash
dotnet restore Poker/Poker.csproj
```

2. Executar:

```bash
dotnet run --project Poker/Poker.csproj
```

3. Abrir no navegador:
- http://localhost:5188
- ou a URL exibida no terminal

## Publicar no Render (gratis)

1. Suba este repositorio para o GitHub.
2. No Render, clique em **New +** -> **Web Service**.
3. Conecte o repositorio `pedro381/planning-poker`.
4. Escolha deploy com Docker (Render detecta o `Dockerfile`).
5. Em **Environment Variables**, configure:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `PathBase=` (deixe vazio, a menos que queira subpath)
6. Clique em **Create Web Service**.

## Observacoes

- O estado da aplicacao e mantido em memoria (singleton). Em reinicio da instancia, salas e votos ativos podem ser perdidos.
- Plano gratis pode hibernar a aplicacao apos inatividade.
