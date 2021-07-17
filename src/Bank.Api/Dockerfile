#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/Bank.Api/Bank.Api.csproj", "src/Bank.Api/"]
COPY ["src/Bank.Infra.Identity/Bank.Infra.Identity.csproj", "src/Bank.Infra.Identity/"]
COPY ["src/Bank.Domain/Bank.Domain.csproj", "src/Bank.Domain/"]
COPY ["src/Bank.Infra.IoC/Bank.Infra.IoC.csproj", "src/Bank.Infra.IoC/"]
COPY ["src/Bank.Infra.Consumers/Bank.Infra.Consumers.csproj", "src/Bank.Infra.Consumers/"]
COPY ["src/Bank.Infra.Data/Bank.Infra.Data.csproj", "src/Bank.Infra.Data/"]
COPY ["src/Bank.App/Bank.App.csproj", "src/Bank.App/"]
RUN dotnet restore "src/Bank.Api/Bank.Api.csproj"
COPY . .
WORKDIR "/src/src/Bank.Api"
RUN dotnet build "Bank.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bank.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bank.Api.dll"]
