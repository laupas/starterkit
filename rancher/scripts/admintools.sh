#!/bin/bash -x

echo "Configure curl, kubectl and helm"
docker build -t admin /var/lidop/scripts
echo 'docker run --net=host -i --rm admin curl "$@"' | sudo tee /usr/bin/curl && sudo chmod +x /usr/bin/curl
echo 'docker run --net=host -i --rm -v /root:/root  -v /var/lidop/rancher/kubeconfig:/root/.kube/config admin helm "$@"' | sudo tee /usr/bin/helm && sudo chmod +x /usr/bin/helm
echo 'docker run --net=host -i --rm -v /root:/root -v /var/lidop/rancher/kubeconfig:/root/.kube/config admin kubectl "$@"' | sudo tee /usr/bin/kubectl && sudo chmod +x /usr/bin/kubectl

# docker save admin > /var/lidop/images/admin.tar
