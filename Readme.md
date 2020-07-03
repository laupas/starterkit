# Installation

## Prerequisites

1. Docker Desktop to be installed
2. Kubernetes activated in Docker Desktop preferences

## Run starterkit on Mac

1. Enable Kubernetes in Docker Desktop (Docker Desktop preferences)
2. Ensure your kubectl points to docker-desktop (Docker Desktop Menu => Kubernetes)
3. Run the following command in the terminal:

`````
docker run --rm -it -v $HOME/.kube:/root/.kube laupas/starterkit.installer --verbose
`````

## Run starterkit on Windows

1. Enable Kubernetes in Docker Desktop (Docker Desktop preferences)
2. Ensure your kubectl points to docker-desktop (Docker Desktop Menu => Kubernetes)
3. Run on of the following commands:

`````
#in CMD
docker run --rm -it -v %homepath%/.kube:/root/.kube laupas/starterkit.installer --verbose
`````

`````
#in Powershell
docker run --rm -it -v $HOME/.kube:/root/.kube laupas/starterkit.installer --verbose
`````

## Use starterkit

### Rancher

You can access the Rancher Dashboard under <https://starterkit.devops.family> with username _admin_ and password _admin_

### Jenkins

You can access Jenkins under <https://jenkins.starterkit.devops.family/> with username _devops_ and password _devops_

### Ldap

You can access Jenkins under <https://ldap.starterkit.devops.family/> with username _devops_ and password _devops_
