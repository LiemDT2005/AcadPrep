# Use ASP.NET runtime image for the final build
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

# Use .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all csproj files and restore as distinct layers to cache dependencies
COPY ["src/AcadPrep.WebUI/AcadPrep.WebUI.csproj", "src/AcadPrep.WebUI/"]
COPY ["src/AcadPrep.Application/AcadPrep.Application.csproj", "src/AcadPrep.Application/"]
COPY ["src/AcadPrep.Domain/AcadPrep.Domain.csproj", "src/AcadPrep.Domain/"]
COPY ["src/AcadPrep.Infrastructure/AcadPrep.Infrastructure.csproj", "src/AcadPrep.Infrastructure/"]
RUN dotnet restore "src/AcadPrep.WebUI/AcadPrep.WebUI.csproj"

# Copy the remaining source code
COPY . .
WORKDIR "/src/src/AcadPrep.WebUI"
RUN dotnet build "AcadPrep.WebUI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AcadPrep.WebUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables (can be overridden by docker-compose)
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AcadPrep.WebUI.dll"]
