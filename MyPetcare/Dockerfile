FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MyPetcare/MyPetcare.csproj", "MyPetcare/"]
RUN dotnet restore "MyPetcare/MyPetcare.csproj"
COPY . .
WORKDIR "/src/MyPetcare"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyPetcare.dll"]