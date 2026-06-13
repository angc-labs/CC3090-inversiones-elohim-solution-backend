FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ElohimShop.API/ElohimShop.API.csproj", "src/ElohimShop.API/"]
COPY ["src/ElohimShop.Application/ElohimShop.Application.csproj", "src/ElohimShop.Application/"]
COPY ["src/ElohimShop.Infrastructure/ElohimShop.Infrastructure.csproj", "src/ElohimShop.Infrastructure/"]
COPY ["src/ElohimShop.Domain/ElohimShop.Domain.csproj", "src/ElohimShop.Domain/"]

RUN dotnet restore "src/ElohimShop.API/ElohimShop.API.csproj"
COPY . .
# Fuerza inclusión de entidades de dominio en capa no cacheada incorrectamente.
COPY src/ElohimShop.Domain/Entities/ src/ElohimShop.Domain/Entities/
# Fuerza inclusión de archivos de pagos Stripe en Infrastructure.
COPY src/ElohimShop.Infrastructure/Pagos/ src/ElohimShop.Infrastructure/Pagos/

RUN dotnet publish "src/ElohimShop.API/ElohimShop.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish/. .
COPY entrypoint.sh /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]