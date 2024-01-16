#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Childrens-Social-Care-CPD-Indexer/Childrens-Social-Care-CPD-Indexer.csproj", "Childrens-Social-Care-CPD-Indexer/"]
RUN dotnet restore "./Childrens-Social-Care-CPD-Indexer/./Childrens-Social-Care-CPD-Indexer.csproj"
COPY . .
WORKDIR "/src/Childrens-Social-Care-CPD-Indexer"
RUN dotnet build "./Childrens-Social-Care-CPD-Indexer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Childrens-Social-Care-CPD-Indexer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Childrens-Social-Care-CPD-Indexer.dll"]