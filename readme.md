# PigeonHorde

[![Docker Image CI](https://github.com/zlzforever/PigeonHorde/actions/workflows/docker-image.yml/badge.svg)](https://github.com/zlzforever/PigeonHorde/actions/workflows/docker-image.yml)

PigeonHorde is a lightweight service registry and discovery solution engineered to power modern microservices architectures. Designed with the agility of pigeons and the resilience of hordes, it excels at managing thousands of service instances while maintaining ultra-low latency and minimal resource overhead.

## Depoly

``` 
  pigeonhorde:
    image: zlzforever/pigeonhorde:20250501
    restart: always
    ports:
      - 8501:9500
    command: --auth Password --password  OCekSK28Do4Bi6QL --aof true   --recover true --checkpointdir /data/checkpoint --lua
    environment:
      - PIGEON_HORDE_REDIS_URL=127.0.0.1:6379,password=OCekSK28Do4Bi6QL,defaultDatabase=0
      - TZ=Asia/Shanghai
    volumes:
      - /data/pigeonhorde:/data
```