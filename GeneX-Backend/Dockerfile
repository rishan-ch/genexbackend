# STAGE 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
# Note: If your .csproj is inside a subfolder, ensure the path is correct
COPY ["GeneX-Backend.csproj", "./"]
RUN dotnet restore "GeneX-Backend.csproj"

# Copy the rest of the files and build
COPY . .
RUN dotnet publish "GeneX-Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# STAGE 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render uses port 10000 by default; you can set it here
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "GeneX-Backend.dll"]
