#!/bin/bash -x

echo "wait for server be ready"
CLUSTERID=$(cat /var/lidop/rancher/clusterid)
LOGINTOKEN=$(cat /var/lidop/rancher/token)
 
echo "wait until rancher server is ready"
while true; do
    result=$(curl -sLk "https://127.0.0.1:444/v3/clusters/${CLUSTERID}" -H "content-type: application/json" -H "Authorization: Bearer $LOGINTOKEN" | jq -r .state)
    
    if [ "$result" == "active" ]; then
        echo "ready"
        break
    else
        echo "wait until cluster is active. current state: ${result}"
    fi
  sleep 5
done

echo "Setup kubectl and helm"
kubectl create -f /var/lidop/scripts/helm.yml
helm init --service-account helm --history-max 200
helm repo update
helm ls
kubectl get nodes