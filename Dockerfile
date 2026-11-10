FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

# Tamang restore command para sa csproj nang walang -f flag
RUN dotnet restore "MyPetcare/MyPetcare.csproj"

# Publish gamit ang net9.0 target
RUN dotnet publish "MyPetcare/MyPetcare.csproj" -c Release -o /app/publish -f net9.0

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyPetcare.dll"]