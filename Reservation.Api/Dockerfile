FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /reservation
COPY ["Reservation.Api/Reservation.Api.csproj", "Reservation.Api/"]
COPY ["Reservation.Shared/Reservation.Shared.csproj", "Reservation.Shared/"]
RUN dotnet restore "Reservation.Api/Reservation.Api.csproj"
COPY . .
WORKDIR Reservation.Api
RUN dotnet build "Reservation.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Reservation.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reservation.Api.dll"]
