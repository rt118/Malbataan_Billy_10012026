FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Malbataan_Billy_10012026/Malbataan_Billy_10012026.csproj", "Malbataan_Billy_10012026/"]
RUN dotnet restore "./Malbataan_Billy_10012026/Malbataan_Billy_10012026.csproj"
COPY . .
WORKDIR "/src/Malbataan_Billy_10012026"
RUN dotnet build "./Malbataan_Billy_10012026.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Malbataan_Billy_10012026.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Malbataan_Billy_10012026.dll"]