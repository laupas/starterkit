#!/bin/bash -x

if ! [ -f /usr/local/bin/docker-machine ]; then
    echo "install docker-machine"
    # base=https://github.com/docker/machine/releases/download/v0.16.0 &&
    #   curl -L $base/docker-machine-$(uname -s)-$(uname -m) >/usr/local/bin/docker-machine &&
    #   chmod +x /usr/local/bin/docker-machine
fi

echo "Cleanup existing servers"
docker-machine rm -f $(docker-machine ls --filter name=rancher*)

echo "Download image"
if ! [ -f ./.rancheros.iso ]; then
  curl -o ./.rancheros.iso "https://releases.rancher.com/os/latest/rancheros.iso"
fi

echo "Start Server1"
docker-machine create \
    -d virtualbox \
    --virtualbox-boot2docker-url ./.rancheros.iso \
    --virtualbox-hostonly-nicpromisc "allow-all" \
    --virtualbox-memory 2048 \
    --virtualbox-cpu-count 1 \
    rancher1

echo "Create admin image"
docker-machine scp ./Dockerfile rancher1:/tmp/Dockerfile 
docker-machine ssh rancher1 docker build -t admin /tmp 

echo "Start Rancher"
docker-machine scp ./server.sh rancher1:/tmp/server.sh 
docker-machine ssh rancher1 bash /tmp/server.sh $(docker-machine ip rancher1)

echo "Create Node Command"
baseCommand=$(docker-machine ssh rancher1 cat /tmp/node.sh)

echo "Add Server1"
ip=$(docker-machine ip rancher1)
command="${baseCommand} --address ${ip} --etcd --controlplane --worker"
docker-machine ssh rancher1 "${command}"

for i in {2..3}
do
    echo "Create node rancher${i}"
    docker-machine create \
        -d virtualbox \
        --virtualbox-boot2docker-url ./.rancheros.iso \
        --virtualbox-hostonly-nicpromisc "allow-all" \
        --virtualbox-memory 1024 \
        --virtualbox-cpu-count 1 \
        "rancher${i}"

    nodeIp=$(docker-machine ip rancher${i})
    nodeCommand="${baseCommand} --address ${nodeIp} --etcd --controlplane --worker"
    docker-machine ssh "rancher${i}" "${nodeCommand}"
done

echo "Configure kubectl and helm"
docker-machine ssh rancher1 "docker run  -v /tmp/kubeconfig:/root/.kube/config  --rm -i admin helm init"
docker-machine ssh rancher1 "docker run  -v /tmp/kubeconfig:/root/.kube/config  --rm -i admin kubectl get nodes"
docker-machine ls

