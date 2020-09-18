FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
WORKDIR /src/retropos-warehouse-api
RUN dotnet build "RetroPOS.Warehouse.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RetroPOS.Warehouse.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RetroPOS.Warehouse.Api.dll"]