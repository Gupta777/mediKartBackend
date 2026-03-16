FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["MediKartX.API/MediKartX.API.csproj", "MediKartX.API/"]
COPY ["MediKartX.Application/MediKartX.Application.csproj", "MediKartX.Application/"]
COPY ["MediKartX.Infrastructure/MediKartX.Infrastructure.csproj", "MediKartX.Infrastructure/"]
COPY ["MediKartX.Core/MediKartX.Core.csproj", "MediKartX.Core/"]

RUN dotnet restore "MediKartX.API/MediKartX.API.csproj"

# copy everything else and publish
COPY . .
WORKDIR /src/MediKartX.API
RUN dotnet publish "MediKartX.API.csproj" -c Release -o /app/publish

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MediKartX.API.dll"]
