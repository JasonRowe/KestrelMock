#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KestrelMockServerInstance/KestrelMockServerInstance.csproj", "KestrelMockServerInstance/"]
COPY ["KestrelMock/KestrelMockServer.csproj", "KestrelMock/"]
RUN dotnet restore "KestrelMockServerInstance/KestrelMockServerInstance.csproj"
COPY . .
WORKDIR "/src/KestrelMockServerInstance"
RUN dotnet build "KestrelMockServerInstance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KestrelMockServerInstance.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KestrelMockServerInstance.dll"]

# custom docker image
#FROM final as FinalLast
#WORKDIR /app
#COPY ["responses","responses"]
#COPY ["appsettings.json", "appsettings.json"]
#ENTRYPOINT ["dotnet", "KestrelMockServer.dll"]

# docker build --no-cache -t kestrelmock:latest -f .\KestrelMockServer\Dockerfile .
# docker run -it --rm -p 5000:80 --name myapp kestrelmock:latest
