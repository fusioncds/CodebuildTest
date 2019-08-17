FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY TestAPI2/TestAPI2.csproj TestAPI2/
RUN dotnet restore TestAPI2/TestAPI2.csproj
COPY . .
WORKDIR /src/TestAPI2
RUN dotnet build TestAPI2.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish TestAPI2.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TestAPI2.dll"]
