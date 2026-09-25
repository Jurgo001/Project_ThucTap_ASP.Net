FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ProductCrud.Gateway/ProductCrud.Gateway.csproj", "ProductCrud.Gateway/"]

RUN dotnet restore "./ProductCrud.Gateway/ProductCrud.Gateway.csproj"

COPY . .

WORKDIR "/src/ProductCrud.Gateway"

RUN dotnet build "./ProductCrud.Gateway.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./ProductCrud.Gateway.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ProductCrud.Gateway.dll"]