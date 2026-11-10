FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "MyPetcare.sln"

# Piliting i-publish sa .NET 9
RUN dotnet publish "MyPetcare/MyPetcare.csproj" -c Release -o /app/publish -f net9.0

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyPetcare.dll"]