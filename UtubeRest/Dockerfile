# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root

RUN apt-get update
RUN apt-get install -y --no-install-recommends python3
RUN apt-get install -y --no-install-recommends python3-pip
RUN apt-get install -y --no-install-recommends python3-venv
RUN apt-get install -y --no-install-recommends pipx

RUN apt-get install -y --no-install-recommends ffmpeg
RUN rm -rf /var/lib/apt/lists/*

#RUN mkdir -p /home/app/cookies && chown -R $APP_UID:$APP_UID /home/app/cookies

USER $APP_UID

RUN pipx install yt-dlp
RUN pipx ensurepath 

ENV PATH="$PATH:/home/app/.local/bin"


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UtubeRest/UtubeRest.csproj", "UtubeRest/"]
RUN dotnet restore "UtubeRest/UtubeRest.csproj"
COPY UtubeRest/. UtubeRest/
WORKDIR /src/UtubeRest
RUN dotnet build "UtubeRest.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UtubeRest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#RUN mkdir -p /home/app/cookies

#ENV ASPNETCORE_HTTPS_PORT 8081
#ENV ASPNETCORE_HTTP_PORT 8080

ENTRYPOINT ["dotnet", "UtubeRest.dll"]