FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 3000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TryInventories.csproj", "."]
RUN dotnet restore "./TryInventories.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TryInventories.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TryInventories.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TryInventories.dll"]