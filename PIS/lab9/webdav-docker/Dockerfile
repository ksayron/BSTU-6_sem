FROM ubuntu:24.04

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install -y nginx-extras apache2-utils ca-certificates \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 3000

CMD ["nginx", "-g", "daemon off;"]
