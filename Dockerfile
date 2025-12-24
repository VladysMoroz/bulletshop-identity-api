# ========================
# Runtime
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# ========================
# Build
# ========================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# 1. Копіюємо solution
COPY IdentityApp.sln ./

# 2. Копіюємо csproj (для кешування)
COPY Api/Api.csproj Api/
COPY AccountControllerTest/AccountControllerTest.csproj AccountControllerTest/
COPY ApiTests/ApiTests.csproj ApiTests/

# 3. Restore
RUN dotnet restore Api/Api.csproj

# 4. Копіюємо весь код
COPY . .

# 5. Publish
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ========================
# Final
# ========================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]