FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
WORKDIR /src
RUN dotnet build "RetroPOS.Publisher.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RetroPOS.Publisher.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RetroPOS.Publisher.Api.dll"]