#!/bin/bash -x
# todo: offline function (same as for the admin image)
rancher_version=${1:-latest}

docker pull rancher/rancher:${rancher_version}
docker pull rancher/rancher-agent:${rancher_version}
