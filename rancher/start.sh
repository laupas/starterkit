#!/bin/bash -x

#############################################################################
# Configuration
nodes=2
admin_password="admin"
rancher_version="v2.2.8"
#############################################################################

if ! [ -f /usr/local/bin/docker-machine ]; then
    echo "install docker-machine"
    # base=https://github.com/docker/machine/releases/download/v0.16.0 &&
    #   curl -L $base/docker-machine-$(uname -s)-$(uname -m) >/usr/local/bin/docker-machine &&
    #   chmod +x /usr/local/bin/docker-machine
fi

echo "Cleanup existing servers"
docker-machine rm -f $(docker-machine ls --filter name=rancher*)

echo "Download image"
if ! [ -f ./../assets/rancheros.iso ]; then
  curl -o ./../assets/rancheros.iso "https://releases.rancher.com/os/latest/rancheros.iso"
fi

echo "Start Server1"
docker-machine create \
    -d virtualbox \
    --virtualbox-boot2docker-url ./../assets/rancheros.iso \
    --virtualbox-hostonly-nicpromisc "allow-all" \
    --virtualbox-memory 2048 \
    --virtualbox-cpu-count 1 \
    rancher1

echo "Copy files"
docker-machine ssh rancher1 "sudo mkdir -p /var/lidop && sudo chmod 777 /var/lidop"
docker-machine scp ./Dockerfile rancher1:/var/lidop/Dockerfile 
docker-machine scp ./server.sh rancher1:/var/lidop/server.sh 

echo "Start Rancher"
docker-machine ssh rancher1 bash /var/lidop/server.sh $(docker-machine ip rancher1) $admin_password $rancher_version

echo "Copy Docker Images to nodes"
docker-machine scp -r rancher1:/var/lidop ./../assets 

echo "Create Node Command"
baseCommand=$(docker-machine ssh rancher1 cat /var/lidop/node.sh)

echo "Add Server1"
ip=$(docker-machine ip rancher1)
command="${baseCommand} --address ${ip} --etcd --controlplane --worker"
docker-machine ssh rancher1 "${command}"


num=$((nodes + 1))
for (( i=2; i<=$num; i++ ))
do
    echo "Create node rancher${i}"
    docker-machine create \
        -d virtualbox \
        --virtualbox-boot2docker-url ./../assets/rancheros.iso \
        --virtualbox-hostonly-nicpromisc "allow-all" \
        --virtualbox-memory 1024 \
        --virtualbox-cpu-count 1 \
        "rancher${i}"

    docker-machine ssh "rancher${i}" "sudo mkdir -p /var/lidop && sudo chmod 777 /var/lidop"
    docker-machine scp -r ./../assets/lidop/ "rancher${i}":/var/
    docker-machine ssh "rancher${i}" "docker load < /var/lidop/rancher.tar"
    docker-machine ssh "rancher${i}" "docker load < /var/lidop/rancher-agent.tar"
    
    nodeIp=$(docker-machine ip rancher${i})
    nodeCommand="${baseCommand} --address ${nodeIp} --etcd --controlplane --worker"
    docker-machine ssh "rancher${i}" "${nodeCommand}"
done

docker-machine ssh rancher1 "helm init"
docker-machine ls
docker-machine ssh rancher1 "kubectl get nodes"
