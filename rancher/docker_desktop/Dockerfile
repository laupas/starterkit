FROM debian:stretch

RUN apt-get update && apt-get install -y curl jq
RUN echo "insecure" > $HOME/.curlrc

# Install Kubectl
RUN curl -LO https://storage.googleapis.com/kubernetes-release/release/`curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt`/bin/linux/amd64/kubectl && \
    chmod +x ./kubectl && \
    mv ./kubectl /usr/local/bin/

# Install Helm
RUN curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3  && \
    chmod 700 get_helm.sh && \
    ./get_helm.sh && \
    helm repo add stable https://kubernetes-charts.storage.googleapis.com/  && \
    helm repo add jetstack "https://charts.jetstack.io"  && \
    helm repo add rancher-latest "https://releases.rancher.com/server-charts/latest"  && \
    helm repo update