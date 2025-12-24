#!/usr/bin/env bash

# check for prereqs
if [[ -z "$SECRET_KEY" ]]; then
  echo '$SECRET_KEY is not defined'
  exit 1
fi

if [[ -z "$CORKBAN_DATA" ]]; then
  echo '$CORKBAN_DATA is not defined'
  exit 1
fi

if [[ -z "$CORKBAN_PRINTER_HOST" ]]; then
  echo '$CORKBAN_PRINTER_HOST is not defined'
  exit 1
fi

if [[ -z "$CORKBAN_PRINTER_PORT" ]]; then
  echo '$CORKBAN_PRINTER_PORT is not defined'
  exit 1
fi

REPO_NAME="corkban-printsrv-dotnet"
REPO_VERSION="0.1"
DOCKER_IMAGE_NAME="curtbrink/$REPO_NAME:v$REPO_VERSION"

# preflight checks
echo " > dotnet restore"
dotnet restore
echo " > dotnet test"
dotnet test

# build dotnet app inside docker image
echo " > docker build"
sudo docker build -t $DOCKER_IMAGE_NAME .

if [[ -n "$(sudo docker ps -a -q -f name=$REPO_NAME)" ]]; then
  echo " > docker stop $REPO_NAME"
  sudo docker stop $REPO_NAME
  echo " > docker rm $REPO_NAME"
  sudo docker rm $REPO_NAME
else
  echo " > container doesn't exist; skipping stop/rm"
fi

echo " > docker run"
sudo docker run -d -p 34201:8080 -e Printer__Hostname=$CORKBAN_PRINTER_HOST -e Printer__Port=$CORKBAN_PRINTER_PORT -e Printer__SecretKey=$SECRET_KEY --mount type=bind,src=$CORKBAN_DATA,dst=/db --name $REPO_NAME $DOCKER_IMAGE_NAME

echo " > done!"
