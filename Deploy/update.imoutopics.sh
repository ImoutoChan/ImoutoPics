#!/bin/bash

cd /home/ImoutoPics/
docker compose -f docker-compose.yml down
docker compose -f docker-compose.yml pull imoutopics
docker compose -f docker-compose.yml up -d
