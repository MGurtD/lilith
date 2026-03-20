# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy only project manifests first — restore layer is cached until deps change
COPY global.json NuGet.config ./
COPY src/Api/Api.csproj src/Api/
COPY src/Application/Application.csproj src/Application/
COPY src/Application.Contracts/Application.Contracts.csproj src/Application.Contracts/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Verifactu/Verifactu.csproj src/Verifactu/

RUN dotnet restore "src/Api/Api.csproj"

# Copy full source and build (restore is already done)
COPY src/ src/
RUN dotnet build "src/Api/Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "src/Api/Api.csproj" -c Release -o /app/publish --no-build

FROM base AS final
# Set timezone
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=Europe/Madrid

WORKDIR /app
RUN mkdir -p /files

RUN apk add --no-cache tzdata icu-libs

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]