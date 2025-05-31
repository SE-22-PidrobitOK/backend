# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JobService/JobService.csproj", "JobService/"]
RUN dotnet restore "JobService/JobService.csproj"
COPY . .
WORKDIR "/src/JobService"
RUN dotnet build "JobService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "JobService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobService.dll"] 