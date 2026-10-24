FROM alpine:latest

RUN mkdir -p /app/web/dist

WORKDIR /app

ADD admin-api /app/admin-api

CMD [ "/app/admin-api" ]