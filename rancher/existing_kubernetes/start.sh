#!/bin/bash -x

#############################################################################
# Configuration
url="rancher.127.0.0.1.xip.io"
password="admin"
helm="docker run --rm -it -v $HOME/.helm:/root/.helm -v $HOME/.kube:/root/.kube starterkit-admin helm"
kubectl="docker run --rm -it -v $HOME/.helm:/root/.helm -v $HOME/.kube:/root/.kube starterkit-admin kubectl"

#############################################################################

echo "###############################"
echo "Create Admin container"
echo "###############################"
docker build -t starterkit-admin .

echo "###############################"
echo "Configure Helm"
echo "###############################"
$kubectl -n kube-system create serviceaccount tiller
$kubectl create clusterrolebinding tiller --clusterrole cluster-admin --serviceaccount=kube-system:tiller
$helm init --service-account tiller --wait

echo "###############################"
echo "Configure Ingress"
echo "###############################"
$helm install stable/nginx-ingress --name ingress-nginx --namespace ingress-nginx --wait

echo "###############################"
echo "Configure Cert Manager"
echo "###############################"
$kubectl apply -f "https://raw.githubusercontent.com/jetstack/cert-manager/release-0.9/deploy/manifests/00-crds.yaml"
$kubectl create namespace cert-manager
$kubectl label namespace cert-manager certmanager.k8s.io/disable-validation=true
$helm repo add jetstack "https://charts.jetstack.io"
$helm repo update
$helm install --name cert-manager --namespace cert-manager --version v0.9.1 jetstack/cert-manager --wait

echo "###############################"
echo "Install Rancher"
echo "###############################"
$helm repo add rancher-latest "https://releases.rancher.com/server-charts/latest"
$helm repo update
$helm install rancher-latest/rancher --name rancher --namespace cattle-system --set hostname=$url --wait

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

