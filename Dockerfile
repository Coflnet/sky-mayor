# Container we use for final publish
FROM mcr.microsoft.com/dotnet/core/aspnet:8.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build container
FROM mcr.microsoft.com/dotnet/core/sdk:8.0-buster AS build

# Copy the code into the container
WORKDIR /src
COPY ["src/Coflnet.Sky.Mayor/Coflnet.Sky.Mayor.csproj", "Coflnet.Sky.Mayor/"]

# NuGet restore
RUN dotnet restore "Coflnet.Sky.Mayor/Coflnet.Sky.Mayor.csproj"
COPY ["src/Coflnet.Sky.Mayor/", "Coflnet.Sky.Mayor/"]

# Build the API
WORKDIR "Coflnet.Sky.Mayor"
RUN dotnet build "Coflnet.Sky.Mayor.csproj" -c Release -o /app/build

# Publish it
FROM build AS publish
RUN dotnet publish "Coflnet.Sky.Mayor.csproj" -c Release -o /app/publish

# Make the final image for publishing
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Coflnet.Sky.Mayor.dll"]
