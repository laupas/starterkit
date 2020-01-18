#!/bin/bash -x
echo "###############################"
echo "Install Applications"
echo "###############################"
helm="docker run --rm -i -v $PWD:/home -v $HOME/.helm:/root/.helm -v $HOME/.kube:/root/.kube starterkit-admin helm"
kubectl="docker run --rm -i -v $PWD:/home -v $HOME/.helm:/root/.helm -v $HOME/.kube:/root/.kube starterkit-admin kubectl"

echo "StarterKit"
$kubectl create namespace starterkit
#kubectl annotate namespace starterkit field.cattle.io/projectId=$PROJECTID

echo "Create Secrets"
$kubectl apply -f /home/common/cert.yaml

echo "OpenLdap"
$helm install ldap --namespace starterkit stable/openldap -f /home/openldap/values.yaml
$kubectl -n starterkit rollout status deploy/ldap-openldap 
$helm install ldap-ui --namespace starterkit /home/openldapui
$kubectl -n starterkit rollout status deploy/ldap-openldap 
$kubectl -n starterkit rollout status deploy/ldap-ui-openldapui

echo "Jenkins"
$kubectl create configmap starterkit --namespace starterkit --from-file=/home/jenkins/config
$helm install jenkins --namespace starterkit stable/jenkins -f /home/jenkins/values.yaml --set master.ingress.hostName=jenkins.starterkit.devops.family
$kubectl apply -f /home/jenkins/jenkins.ingress.yml
$kubectl -n starterkit rollout status deploy/jenkins 

# # consul
# kubectl create namespace consul
# helm install --name consul --namespace consul ./consul-helm

#echo "GitLab"
#kubectl create secret tls starterkit-cert --cert=./../certs/server.crt --key=./../certs/server.key
#helm repo add gitlab https://charts.gitlab.io/
#helm repo update

#helm install --name gitlab --namespace starterkit gitlab/gitlab -f ./gitlab/gitlab.values.yaml

#kubectl -n starterkit rollout status deploy/gitlab-gitlab-shell 
#kubectl -n starterkit rollout status deploy/gitlab-registry  
#kubectl -n starterkit rollout status deploy/gitlab-sidekiq-all-in-1 
#kubectl -n starterkit rollout status deploy/gitlab-unicorn  

# echo "harbor"
# helm repo add harbor https://helm.goharbor.io
# helm install --name harbor --namespace starterkit harbor/harbor -f ./harbor/harbor.values.yaml
# kubectl -n starterkit rollout status deploy/harbor-harbor-chartmuseum 
# kubectl -n starterkit rollout status deploy/harbor-harbor-core
# kubectl -n starterkit rollout status deploy/harbor-harbor-portal 
# kubectl -n starterkit rollout status deploy/harbor-harbor-registry 

#ehoo "Info Application"
#docker pull llaaccssaapp/info
#docker tag llaaccssaapp/info registry.lauener.home/library/info
#docker push registry.lauener.home/library/info
#kubectl apply -f ./test



# # openfaas
# kubectl apply -f https://raw.githubusercontent.com/openfaas/faas-netes/master/namespaces.yml
# kubectl -n openfaas create secret generic basic-auth \
# --from-literal=basic-auth-user=admin \
# --from-literal=basic-auth-password=admin

# helm repo add openfaas https://openfaas.github.io/faas-netes/
# helm repo update
# helm upgrade openfaas --install openfaas/openfaas --namespace openfaas --set basic_auth=true --set functionNamespace=openfaas-fn

# kubectl apply -f ./openfaas

# echo "install longhron"
#helm install ./longhorn/chart --name longhorn --namespace longhorn-system



# kubectl apply -f ./ingress/longhorn.home.yml
# kubectl apply -f ./ingress/longhorn.zone.yml
