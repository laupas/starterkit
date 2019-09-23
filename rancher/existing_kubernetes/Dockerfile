FROM debian:stretch

RUN apt-get update && apt-get install -y curl jq

# Install Kubectl
RUN curl -LO https://storage.googleapis.com/kubernetes-release/release/`curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt`/bin/linux/amd64/kubectl && \
    chmod +x ./kubectl && \
    mv ./kubectl /usr/local/bin/

# Install Helm
RUN curl -LO https://git.io/get_helm.sh && \
    chmod a+x get_helm.sh && \
    ./get_helm.sh && \
    helm init --client-only && \
    helm repo update