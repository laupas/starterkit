FROM debian:stretch

RUN apt-get update && apt-get install -y curl jq

# Install Kubectl
RUN curl -LO https://storage.googleapis.com/kubernetes-release/release/`curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt`/bin/linux/amd64/kubectl
RUN chmod +x ./kubectl
RUN mv ./kubectl /usr/local/bin/

# Install Helm
RUN curl -LO https://git.io/get_helm.sh
RUN chmod a+x get_helm.sh
RUN ./get_helm.sh