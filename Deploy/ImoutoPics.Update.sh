#!/bin/bash

cd /home/ImoutoPics/
docker-compose -f docker-compose.yml -f docker-compose.production.yml down
docker-compose -f docker-compose.yml -f docker-compose.production.yml pull imoutopics
docker-compose -f docker-compose.yml -f docker-compose.production.yml up -d
