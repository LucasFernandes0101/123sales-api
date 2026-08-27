FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/123vendas.Api/123vendas.Api.csproj", "src/123vendas.Api/"]
COPY ["src/123vendas.Application/123vendas.Application.csproj", "src/123vendas.Application/"]
COPY ["src/123vendas.Domain/123vendas.Domain.csproj", "src/123vendas.Domain/"]
COPY ["src/123vendas.Infrastructure/123vendas.Infrastructure.csproj", "src/123vendas.Infrastructure/"]

RUN dotnet restore src/123vendas.Api/123vendas.Api.csproj

COPY . .

WORKDIR /src/src/123vendas.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "123vendas.Api.dll"]
