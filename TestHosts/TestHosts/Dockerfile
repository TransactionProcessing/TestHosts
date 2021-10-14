FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TestHosts/TestHosts.csproj", "TestHosts/"]
COPY ["TestHosts/NuGet.Config", "TestHosts/"]
RUN dotnet restore "TestHosts/TestHosts.csproj"
COPY . .
WORKDIR "/src/TestHosts"
RUN dotnet build "TestHosts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestHosts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestHosts.dll"]
