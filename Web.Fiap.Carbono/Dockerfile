FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Web.Fiap.Carbono/Web.Fiap.Carbono.csproj", "Web.Fiap.Carbono/"]
RUN dotnet restore "Web.Fiap.Carbono/Web.Fiap.Carbono.csproj"
COPY . .
WORKDIR "/src/Web.Fiap.Carbono"
RUN dotnet build "./Web.Fiap.Carbono.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web.Fiap.Carbono.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.Fiap.Carbono.dll"]
