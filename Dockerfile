FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Tripcare360.Domain/Tripcare360.Domain.csproj Tripcare360.Domain/
COPY Tripcare360.Application/Tripcare360.Application.csproj Tripcare360.Application/
COPY Tripcare360.Infrastructure/Tripcare360.Infrastructure.csproj Tripcare360.Infrastructure/
COPY Tripcare360.WebApi/Tripcare360.WebApi.csproj Tripcare360.WebApi/
RUN dotnet restore Tripcare360.WebApi/Tripcare360.WebApi.csproj

COPY . .
RUN dotnet publish Tripcare360.WebApi/Tripcare360.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Tripcare360.WebApi.dll"]
