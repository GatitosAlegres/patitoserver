
ARG APP_NAME=patitoserver

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build

ARG APP_NAME

WORKDIR /opt/apps/${APP_NAME}

COPY . .

RUN dotnet publish -c Release -r linux-x64

RUN chmod +x /opt/apps/${APP_NAME}/bin/Release/net6.0/linux-x64/publish/PatitoServer

FROM ubuntu:23.10 as deployment

RUN apt update && apt install -y libicu-dev

ARG APP_NAME

WORKDIR /opt/apps/${APP_NAME}

COPY --from=build /opt/apps/${APP_NAME}/bin/Release/net6.0/linux-x64/publish/ /opt/apps/${APP_NAME}

CMD [ "/opt/apps/$APP_NAME/PatitoServer" ]

EXPOSE 1300