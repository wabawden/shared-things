# Build the React frontend
FROM node:24-alpine AS frontend-build

WORKDIR /frontend

COPY src/CanIBorrow.Web/package*.json ./
RUN npm ci

COPY src/CanIBorrow.Web/ ./

# Override the local wwwroot output setting inside the build container
RUN npm run build -- --outDir dist


# Build and publish the .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build

WORKDIR /source

COPY src/SharedThings.Api/SharedThings.Api.csproj \
     src/SharedThings.Api/

RUN dotnet restore \
    src/SharedThings.Api/SharedThings.Api.csproj

COPY src/SharedThings.Api/ \
     src/SharedThings.Api/

COPY --from=frontend-build \
     /frontend/dist \
     src/SharedThings.Api/wwwroot

RUN dotnet publish \
    src/SharedThings.Api/SharedThings.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish


# Create the production runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=api-build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SharedThings.Api.dll"]