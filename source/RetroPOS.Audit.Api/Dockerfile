FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 6000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
WORKDIR /src/retropos-audit-api
RUN dotnet build "RetroPOS.Audit.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RetroPOS.Audit.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RetroPOS.Audit.Api.dll"]