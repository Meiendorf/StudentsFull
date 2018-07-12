FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 1611
EXPOSE 44378

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Students/Students.csproj Students/
COPY MessageContracts/MessageContracts.csproj MessageContracts/
RUN dotnet restore Students/Students.csproj
COPY . .
WORKDIR /src/Students
RUN dotnet build Students.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Students.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Students.dll"]
