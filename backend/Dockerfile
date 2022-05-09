FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app
COPY backend/*.sln ./
COPY backend/*.csproj ./
RUN dotnet restore

COPY backend/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:7040
ENTRYPOINT [ "dotnet", "backend.dll" ]
EXPOSE 7040

