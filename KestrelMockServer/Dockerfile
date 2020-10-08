#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["KestrelMockServer/KestrelMockServer.csproj", "KestrelMockServer/"]
COPY ["KestrelMock/KestrelMock.csproj", "KestrelMock/"]
RUN dotnet restore "KestrelMockServer/KestrelMockServer.csproj"
COPY . .
WORKDIR "/src/KestrelMockServer"
RUN dotnet build "KestrelMockServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KestrelMockServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KestrelMockServer.dll"]

# custom docker image
#FROM final as FinalLast
#WORKDIR /app
#COPY ["responses","responses"]
#COPY ["appsettings.json", "appsettings.json"]
#ENTRYPOINT ["dotnet", "KestrelMockServer.dll"]

# docker build --no-cache -t kestrelmock:latest -f .\KestrelMockServer\Dockerfile .
# docker run -it --rm -p 5000:80 --name myapp kestrelmock:latest
