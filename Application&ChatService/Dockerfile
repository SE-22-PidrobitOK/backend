# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Application&ChatService/Application&ChatService.csproj", "Application&ChatService/"]
RUN dotnet restore "Application&ChatService/Application&ChatService.csproj"
COPY . .
WORKDIR "/src/Application&ChatService"
RUN dotnet build "Application&ChatService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Application&ChatService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Application&ChatService.dll"] 