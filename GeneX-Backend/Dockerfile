FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# This copies everything from your LOCAL publish folder to the IMAGE's /app folder
COPY ./publish/ .

# Force ASP.NET to listen on port 10000 (Render's favorite)

EXPOSE 10000

# Ensure the casing matches your .csproj exactly
ENTRYPOINT ["dotnet", "GeneX-Backend.dll"]