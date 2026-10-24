FROM example.invalid/dotnet/aspnet:8.0 AS runtime
ARG APP_NAME
ARG SHA1
ARG PIPE_ID

ENV APP_NAME ${APP_NAME}
ENV Git_SHA1 ${SHA1}
ENV Pipeline_ID ${PIPE_ID}

RUN sed -i 's/\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf
RUN printf "\n\n[openssl_init]\nssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_sect]\nsystem_default = ssl_default_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0\n" >> /etc/ssl/openssl.cnf

EXPOSE 3100/tcp
EXPOSE 3101/tcp
EXPOSE 3102/tcp
EXPOSE 3109/tcp

RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .

CMD ["sh", "-c", "exec dotnet \"${APP_NAME}.dll\""]
