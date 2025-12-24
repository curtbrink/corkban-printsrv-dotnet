FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Corkban.Printsrv/Corkban.Printsrv.csproj", "Corkban.Printsrv/"]
RUN dotnet restore "Corkban.Printsrv/Corkban.Printsrv.csproj"
COPY ./src .
WORKDIR "/src/Corkban.Printsrv"
RUN dotnet build "./Corkban.Printsrv.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Corkban.Printsrv.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Corkban.Printsrv.dll"]
