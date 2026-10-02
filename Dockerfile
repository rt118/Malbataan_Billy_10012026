# Use .NET 10 SDK to build, ASP.NET runtime for smaller final image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj(s) and restore as a separate step for caching
COPY ["Malbataan_Billy_10012026.csproj", "./"]
RUN dotnet restore "./Malbataan_Billy_10012026.csproj"

# copy everything and build/publish
COPY . .
RUN dotnet publish "Malbataan_Billy_10012026.csproj" -c Release -o /app/publish /p:UseAppHost=false

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# optional: set URL to listen on port 80 (container)
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish ./

# Expose HTTP port
EXPOSE 80

ENTRYPOINT ["dotnet", "Malbataan_Billy_10012026.dll"]