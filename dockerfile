FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# On Local
# EXPOSE 8081
# ENV ASPNETCORE_URLS=http://+:8081

# On Render cloud
EXPOSE 80
EXPOSE 443

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DotnetStockAPI.csproj", "./"]
RUN dotnet restore "DotnetStockAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DotnetStockAPI.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DotnetStockAPI.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotnetStockAPI.dll"]