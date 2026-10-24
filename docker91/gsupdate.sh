#!/bin/bash
git pull
while read line; do ( docker pull "example.invalid/target:$line";) & done < versionneed;wait
docker-compose down;docker-compose up --build -d