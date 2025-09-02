# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
 
COPY . .

# Restaura dependencias
RUN dotnet restore

RUN dotnet publish -c Release -o out
 
# Etapa 2: Test
FROM build AS test
RUN dotnet test tests --no-build --logger "trx;LogFileName=TestingResults.trx"
 
# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "InventoryApi.ML.dll"]