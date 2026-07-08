# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LeoEducation.Api.csproj ./
RUN dotnet restore ./LeoEducation.Api.csproj

COPY . ./
RUN dotnet publish ./LeoEducation.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "LeoEducation.Api.dll"]
