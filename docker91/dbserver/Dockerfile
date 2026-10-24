ARG VERSION=0.7.314
FROM example.invalid/target:${VERSION}
ENV PATH "/approot/dbserver:${PATH}"
WORKDIR /approot/dbserver/
ENTRYPOINT ["dbserver"]
