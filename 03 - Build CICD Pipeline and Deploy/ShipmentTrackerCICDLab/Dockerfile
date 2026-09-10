FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ShipmentTracker.sln .
COPY src/ShipmentTracker.Application/ShipmentTracker.Application.csproj src/ShipmentTracker.Application/
COPY src/ShipmentTracker.Api/ShipmentTracker.Api.csproj src/ShipmentTracker.Api/
COPY tests/ShipmentTracker.Tests/ShipmentTracker.Tests.csproj tests/ShipmentTracker.Tests/

RUN dotnet restore ShipmentTracker.sln

COPY . .
RUN dotnet publish src/ShipmentTracker.Api/ShipmentTracker.Api.csproj     --configuration Release     --no-restore     --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "ShipmentTracker.Api.dll"]
