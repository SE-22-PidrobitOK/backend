# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ModeratorService/ModeratorService.csproj", "ModeratorService/"]
RUN dotnet restore "ModeratorService/ModeratorService.csproj"
COPY . .
WORKDIR "/src/ModeratorService"
RUN dotnet build "ModeratorService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ModeratorService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ModeratorService.dll"] 