FROM stuartferguson/txnprocbase:latest AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TestHosts/TestHosts.csproj", "TestHosts/"]
COPY ["TestHosts/NuGet.Config", "TestHosts/"]
COPY ["Directory.Packages.props", "."]
COPY . .
RUN dotnet restore "TestHosts/TestHosts.csproj"
COPY . .
WORKDIR "/src/TestHosts"
RUN dotnet build "TestHosts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestHosts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=10001:10001 /app/publish .
USER 10001:10001
ENTRYPOINT ["dotnet", "TestHosts.dll"]
