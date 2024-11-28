FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD

WORKDIR /src

# Copy NuGet.Config file
COPY NuGet.Config ./

# Configure authentication for the private NuGet feed
# The nuget config is not required for building on azure devops
#RUN dotnet nuget update source PrivateFeed \
#    --username "sandmann@codedata.solutions" \
#    --password "$NUGET_PASSWORD" \
#    --store-password-in-clear-text

# Copy project files and restore dependencies
COPY ["Blocktrust.Mediator.Server/Blocktrust.Mediator.Server.csproj", "Blocktrust.Mediator.Server/"]
COPY ["Blocktrust.Mediator.Common/Blocktrust.Mediator.Common.csproj", "Blocktrust.Mediator.Common/"]
RUN dotnet restore "Blocktrust.Mediator.Server/Blocktrust.Mediator.Server.csproj"

COPY . .
WORKDIR "/src/Blocktrust.Mediator.Server"
RUN dotnet build "Blocktrust.Mediator.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Blocktrust.Mediator.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blocktrust.Mediator.Server.dll"]
