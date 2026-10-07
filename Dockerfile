FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 7860
ENV ASPNETCORE_URLS=http://+:7860
ENTRYPOINT ["dotnet", "CostQualityControl.API.dll"]
