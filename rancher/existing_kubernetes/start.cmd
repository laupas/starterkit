@echo off
setlocal EnableDelayedExpansion

REM #############################################################################
REM # Configuration
set url=localhost
set password=admin
set helm=helm
set kubectl=kubectl
set KUBECONFIG=%userprofile%\.kube\config
REM #############################################################################

echo "###############################"
echo "Build tools container"
echo "###############################"
docker build -t starterkit-tools .

WHERE /q %helm%
if ERRORLEVEL 1 (
    echo "Run helm inside a container"
    set helm=docker run --rm -i -v %userprofile%/.kube/config:/root/.kube/config starterkit-tools helm
    if not exist "%userprofile%/.helm" mkdir "%userprofile%/.helm"
)

WHERE /q %kubectl%
if ERRORLEVEL 1 (
    echo "Run kubectl inside a container"
    docker build -t starterkit-tools .
    set kubectl=docker run --rm -i -v %userprofile%/.kube/config:/root/.kube/config starterkit-tools kubectl
)

for /f "tokens=3" %%a in ('ping host.docker.internal ^| find /i "reply"') do (
  set address=%%a
  set address=!address:~0,-1!
)
echo "###############################"
echo "Host IP: %address%"
echo "###############################"


echo "###############################"
echo "Configure Helm"
echo "###############################"
%kubectl% -n kube-system create serviceaccount tiller
%kubectl% create clusterrolebinding tiller --clusterrole cluster-admin --serviceaccount=kube-system:tiller
%helm% init --service-account tiller --wait


echo "###############################"
echo "Configure Ingress"
echo "###############################"
%helm% install --tls-verify=false stable/nginx-ingress --name ingress-nginx --namespace ingress-nginx --wait


echo "###############################"
echo "Configure Cert Manager"
echo "###############################"
%kubectl% apply -f "https://raw.githubusercontent.com/jetstack/cert-manager/release-0.9/deploy/manifests/00-crds.yaml"
%kubectl% create namespace cert-manager
%kubectl% label namespace cert-manager certmanager.k8s.io/disable-validation=true
%helm% repo add jetstack "https://charts.jetstack.io"
%helm% repo update
%helm% install --tls-verify=false --name cert-manager --namespace cert-manager --version v0.9.1 jetstack/cert-manager --wait


echo "###############################"
echo "Install Rancher"
echo "###############################"
%helm% repo add rancher-latest "https://releases.rancher.com/server-charts/latest"
%helm% repo update
%helm% install --tls-verify=false rancher-latest/rancher --name rancher --namespace cattle-system --set hostname=%url% --wait


echo "###############################"
echo "Configure Rancher"
echo "###############################"
docker run --rm -i -v %cd%:/root --add-host=localhost:%address% starterkit-tools bash /root/configure.sh %password%