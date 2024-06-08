FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5280

ENV ASPNETCORE_URLS=http://+:5280

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DeliveryDrivers.csproj", "./"]
RUN dotnet restore "DeliveryDrivers.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DeliveryDrivers.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DeliveryDrivers.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeliveryDrivers.dll"]
