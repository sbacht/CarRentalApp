FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/CarRental.WebUI/CarRental.WebUI.csproj
RUN dotnet publish src/CarRental.WebUI/CarRental.WebUI.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/CarRentalApp.db"

RUN mkdir -p /app/data && chmod 777 /app/data

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CarRental.WebUI.dll"]
