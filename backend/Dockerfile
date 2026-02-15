# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY . .

RUN dotnet restore "src/Api/Api.csproj"
WORKDIR "/src/."
COPY . .
RUN dotnet build "src/Api/Api.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "src/Api/Api.csproj" -c Release -o /app/publish

FROM base AS final
# Set timezone
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=Europe/Madrid

WORKDIR /app
RUN mkdir -p /files

RUN apk add --no-cache tzdata icu-libs

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]