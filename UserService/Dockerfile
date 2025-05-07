# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserService/UserService.csproj", "UserService/"]
RUN dotnet restore "UserService/UserService.csproj"
COPY . .
WORKDIR "/src/UserService"
RUN dotnet build "UserService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserService.dll"] 