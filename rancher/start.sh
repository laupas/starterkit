#!/bin/bash -x

#############################################################################
# Configuration
nodes=1
admin_password="admin"
rancher_version="v2.2.8"
#############################################################################

if ! [ -f /usr/local/bin/docker-machine ]; then
    echo "install docker-machine"
    # base=https://github.com/docker/machine/releases/download/v0.16.0 &&
    #   curl -L $base/docker-machine-$(uname -s)-$(uname -m) >/usr/local/bin/docker-machine &&
    #   chmod +x /usr/local/bin/docker-machine
fi

echo "################################################################"
echo "Cleanup existing servers"
echo "################################################################"
docker-machine rm -f $(docker-machine ls --filter name=rancher*)

echo "################################################################"
echo "Download image"
echo "################################################################"
if ! [ -f ./../temp/rancheros.iso ]; then
  curl -o ./../temp/rancheros.iso "https://releases.rancher.com/os/latest/rancheros.iso"
fi

echo "################################################################"
echo "Start Server1"
echo "################################################################"
docker-machine create \
    -d virtualbox \
    --virtualbox-boot2docker-url ./../temp/rancheros.iso \
    --virtualbox-hostonly-nicpromisc "allow-all" \
    --virtualbox-memory 2048 \
    --virtualbox-cpu-count 1 \
    rancher1

echo "################################################################"
echo "Copy script folder"
echo "################################################################"
docker-machine ssh rancher1 "sudo mkdir -p /var/lidop && sudo chmod 777 /var/lidop"
docker-machine scp -r ./scripts rancher1:/var/lidop/ 

echo "################################################################"
echo "Create Admin Tools"
echo "################################################################"
docker-machine ssh rancher1 bash /var/lidop/scripts/admintools.sh 

echo "################################################################"
echo "Download Images"
echo "################################################################"
docker-machine ssh rancher1 bash /var/lidop/scripts/downloadImages.sh $rancher_version

echo "################################################################"
echo "Start Rancher"
echo "################################################################"
docker-machine ssh rancher1 bash /var/lidop/scripts/server.sh $(docker-machine ip rancher1) $admin_password $rancher_version

echo "Copy rancher configs from server1 to local machine"
docker-machine scp -r rancher1:/var/lidop/rancher ./../temp 

echo "Create add Node Command"
baseCommand=$(docker-machine ssh rancher1 cat /var/lidop/rancher/installNode.sh)

echo "Add Server1 to cluster"
ip=$(docker-machine ip rancher1)
command="${baseCommand} --address ${ip} --etcd --controlplane --worker"
docker-machine ssh rancher1 "${command}"

echo "################################################################"
echo "Add Nodes"
echo "################################################################"
num=$((nodes + 1))
for (( i=2; i<=$num; i++ ))
do
    echo "Create node rancher${i}"
    docker-machine create \
        -d virtualbox \
        --virtualbox-boot2docker-url ./../temp/rancheros.iso \
        --virtualbox-hostonly-nicpromisc "allow-all" \
        --virtualbox-memory 1024 \
        --virtualbox-cpu-count 1 \
        "rancher${i}"
    
    nodeIp=$(docker-machine ip rancher${i})
    nodeCommand="${baseCommand} --address ${nodeIp} --etcd --controlplane --worker"
    docker-machine ssh "rancher${i}" "${nodeCommand}"
done

echo "################################################################"
echo "Setup kubectl and helm"
echo "################################################################"
docker-machine ssh rancher1 "helm init --history-max 200"
docker-machine ssh rancher1 "helm repo update"
docker-machine ssh rancher1 "helm ls"
docker-machine ls
docker-machine ssh rancher1 "kubectl get nodes"

echo "################################################################"
echo "Rancher is ready"
echo "################################################################"
echo "Access rancher under: https://${ip}:444 with the user admin and the password ${admin_password}"