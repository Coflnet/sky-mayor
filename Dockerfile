FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /build/sky
COPY Coflnet.Sky.Mayor.csproj Coflnet.Sky.Mayor.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8000

RUN useradd --uid $(shuf -i 2000-65000 -n 1) app-usr
USER app-usr

ENTRYPOINT ["dotnet", "Coflnet.Sky.Mayor.dll", "--hostBuilder:reloadConfigOnChange=false"]
