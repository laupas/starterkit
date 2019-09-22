#!/bin/bash -x
rancher_ip=${1}

echo "wait for server be ready"
CLUSTERID=$(cat /var/starterkit/rancher/clusterid)
LOGINTOKEN=$(cat /var/starterkit/rancher/token)
 
echo "wait until rancher server is ready"
while true; do
    result=$(curl -sLk "https://127.0.0.1:444/v3/clusters/${CLUSTERID}" -H "content-type: application/json" -H "Authorization: Bearer $LOGINTOKEN" | jq -r .state)
    
    if [ "$result" == "active" ]; then
        echo "Cluster is Ready"
        break
    else
        echo "wait until cluster is active. current state: ${result}. You can also check the current status under: https://${rancher_ip}:444"
    fi
  sleep 5
done

echo "Setup kubectl and helm"
kubectl create -f /var/starterkit/scripts/helm.yml
helm init --service-account helm --history-max 200

echo "wait until helm is ready"
while true; do
    result=$(helm ls 2>&1)
    
    if [[ $result == *Error* ]]; then
        echo "wait until helm is active. current state: ${result}. You can also check the current status under: https://${rancher_ip}:444"
    else
        echo "Helm is ready."
        break
    fi
  sleep 5
done

echo "kubectl and helm info"
kubectl get nodes
helm ls