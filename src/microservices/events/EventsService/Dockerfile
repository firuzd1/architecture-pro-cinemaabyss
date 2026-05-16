FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["EventsService.csproj", "."]
RUN dotnet restore "EventsService.csproj"
COPY . .
RUN dotnet build "EventsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsService.dll"]