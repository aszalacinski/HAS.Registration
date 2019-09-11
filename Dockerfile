FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["HAS.Registration.csproj", ""]
RUN dotnet restore "./HAS.Registration.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "HAS.Registration.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HAS.Registration.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HAS.Registration.dll"]