#!/bin/bash
cd /GameServer
git pull;docker-compose down;docker-compose up --build -d