FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

COPY . .
RUN dotnet restore "./CME/CME.csproj"
RUN dotnet publish "./CME/CME.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS runtime

COPY --from=build /app /app
COPY ./CME/start.sh /start.sh
WORKDIR /app
ENTRYPOINT ["/start.sh"]
CMD ["dotnet","./CME.dll"]