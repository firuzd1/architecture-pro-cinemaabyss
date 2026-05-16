FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ProxyService.csproj", "."]
RUN dotnet restore "ProxyService.csproj"
COPY . .
RUN dotnet build "ProxyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProxyService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProxyService.dll"]