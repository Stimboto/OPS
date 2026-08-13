FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OPS.API/OPS.API.csproj", "OPS.API/"]
COPY ["OPS.Application/OPS.Application.csproj", "OPS.Application/"]
COPY ["OPS.Domain/OPS.Domain.csproj", "OPS.Domain/"]
COPY ["OPS.Infrastructure/OPS.Infrastructure.csproj", "OPS.Infrastructure/"]

RUN dotnet restore "OPS.API/OPS.API.csproj"

COPY . .
WORKDIR "/src/OPS.API"
RUN dotnet build "OPS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OPS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OPS.API.dll"]
