#!/bin/bash -x

rancher_version=${1:-latest}

docker pull rancher/rancher:${rancher_version}
docker pull rancher/rancher-agent:${rancher_version}
# docker save rancher/rancher:${rancher_version} > /var/lidop/images/rancher.tar
# docker save rancher/rancher-agent:${rancher_version} > /var/images/lidop/rancher-agent.tar
