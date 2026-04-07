FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef

COPY ["src/ElohimShop.API/ElohimShop.API.csproj", "src/ElohimShop.API/"]
COPY ["src/ElohimShop.Application/ElohimShop.Application.csproj", "src/ElohimShop.Application/"]
COPY ["src/ElohimShop.Infrastructure/ElohimShop.Infrastructure.csproj", "src/ElohimShop.Infrastructure/"]
COPY ["src/ElohimShop.Domain/ElohimShop.Domain.csproj", "src/ElohimShop.Domain/"]

RUN dotnet restore "src/ElohimShop.API/ElohimShop.API.csproj"
COPY . .

RUN dotnet publish "src/ElohimShop.API/ElohimShop.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
COPY --from=build /src/. /src/
COPY --from=build /app/publish/. .
COPY entrypoint.sh /entrypoint.sh

ENV PATH="/root/.dotnet/tools:${PATH}"
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]