#!/bin/bash -x

#############################################################################
# Configuration
url="starterkit.devops.family"
ssl=self
password="admin"
helm="docker run --rm -i -v $PWD:/home -v $HOME/.helm:/root/.helm -v $HOME/.kube:/root/.kube starterkit-admin helm"
kubectl="docker run --rm -i -v $PWD:/home -v $HOME/.helm:/root/.helm -v $HOME/.kube:/root/.kube starterkit-admin kubectl"

#############################################################################

echo "###############################"
echo "Create Admin container"
echo "###############################"
docker build -t starterkit-admin .

echo "###############################"
echo "Configure Ingress"
echo "###############################"
$kubectl create namespace ingress-nginx
$kubectl -n ingress-nginx create secret tls ingress-default-cert \
  --cert=/home/ssl/$ssl/server.crt \
  --key=/home/ssl/$ssl/server.key
$helm install ingress-nginx stable/nginx-ingress --namespace ingress-nginx --wait --set controller.extraArgs.default-ssl-certificate=ingress-nginx/ingress-default-cert

echo "###############################"
echo "Install Rancher"
echo "###############################"
$kubectl create namespace cattle-system
$kubectl -n cattle-system create secret tls tls-rancher-ingress \
  --cert=/home/ssl/$ssl/server.crt \
  --key=/home/ssl/$ssl/server.key

$kubectl -n cattle-system create secret generic tls-ca \
  --from-file=/home/ssl/$ssl/cacerts.pem

$helm install rancher rancher-latest/rancher \
  --namespace cattle-system \
  --set hostname=$url \
  --set privateCA=true \
  --set ingress.tls.source=secret
$kubectl -n cattle-system rollout status deploy/rancher

echo "###############################"
echo "Configure Rancher"
echo "###############################"

echo "wait until rancher server is started"
while true; do
curl -sLk "https://${url}/ping" && break
  sleep 5
done

echo "Login to rancher"
while true; do

    LOGINRESPONSE=$(curl -s "https://${url}/v3-public/localProviders/local?action=login" \
        -H "content-type: application/json" \
        --data-binary '{"username":"admin","password":"admin"}' --insecure)
    LOGINTOKEN=$(echo $LOGINRESPONSE | docker run -i --rm starterkit-admin jq -r .token)

    if [ "$LOGINTOKEN" != "null" ]; then
        break
    else
        sleep 5
    fi
done

echo "Change password"
curl -s "https://${url}/v3/users?action=changepassword" \
    -H 'content-type: application/json' \
    -H "Authorization: Bearer $LOGINTOKEN" \
    --data-binary '{"currentPassword":"admin","newPassword":"'${password}'"}' \
    --insecure

echo "Create API key"
apiresponse=$(curl -s "https://${url}/v3/token" \
    -H 'content-type: application/json' \
    -H "Authorization: Bearer $LOGINTOKEN" \
    --data-binary '{"type":"token","description":"automation"}' \
    --insecure)

echo "Extract and store token"
export APITOKEN=`echo $apiresponse | docker run -i --rm starterkit-admin jq -r .token`

echo "Configure server-url"
curl -s "https://${url}/v3/settings/server-url" \
    -H 'content-type: application/json' \
    -H "Authorization: Bearer $APITOKEN" \
    -X PUT \
    --data-binary '{"name":"server-url","value":"'https://${url}'"}' \
    --insecure

