FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Poker.sln ./
COPY Poker/Poker.csproj Poker/
RUN dotnet restore Poker/Poker.csproj

COPY . .
RUN dotnet publish Poker/Poker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render provides PORT at runtime; default to 8080 for local container runs.
ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "Poker.dll"]
