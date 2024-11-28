FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_PASSWORD
ENV NUGET_PASSWORD=$NUGET_PASSWORD

WORKDIR /src

# Copy NuGet.Config file
COPY nuget.config ./

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

# Configure authentication for the private NuGet feed
RUN dotnet nuget update source blocktrust-nuget \
    --username "sandmann@codedata.solutions" \
    --password ${{NUGET_PASSWORD}} \
    --store-password-in-clear-text

# Copy project files and restore dependencies
COPY ["Blocktrust.Mediator.Server/Blocktrust.Mediator.Server.csproj", "Blocktrust.Mediator.Server/"]
COPY ["Blocktrust.Mediator.Common/Blocktrust.Mediator.Common.csproj", "Blocktrust.Mediator.Common/"]
RUN dotnet restore "Blocktrust.Mediator.Server/Blocktrust.Mediator.Server.csproj"

COPY . .
WORKDIR "/src/Blocktrust.Mediator.Server"
RUN npm install
RUN dotnet build "Blocktrust.Mediator.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Blocktrust.Mediator.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blocktrust.Mediator.Server.dll"]
