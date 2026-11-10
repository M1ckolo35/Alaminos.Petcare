FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopyahin ang buong repository kasama ang solution at mga project folder
COPY . .

# I-restore gamit ang solution file sa root
RUN dotnet restore "MyPetcare.sln"

# I-publish ang MyPetcare project
RUN dotnet publish "MyPetcare/MyPetcare.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyPetcare.dll"]