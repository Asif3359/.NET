FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EcomApi.csproj", "./"]
RUN dotnet restore "EcomApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "EcomApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EcomApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomApi.dll"]