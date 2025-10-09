#!/bin/bash
sudo usermod -aG docker qopiq
sudo systemctl enable docker
sudo systemctl start docker
mkdir -p ~/qopiq/src ~/qopiq/logs
