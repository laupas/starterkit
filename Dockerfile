FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine as run
# https://github.com/kubernetes/kubernetes/releases
ENV KUBE_LATEST_VERSION="v1.17.1"
# https://github.com/kubernetes/helm/releases
ENV HELM_VERSION="v3.0.2"

RUN apk add --no-cache ca-certificates bash git openssh curl \
    && wget -q https://storage.googleapis.com/kubernetes-release/release/${KUBE_LATEST_VERSION}/bin/linux/amd64/kubectl -O /usr/local/bin/kubectl \
    && chmod +x /usr/local/bin/kubectl \
    && wget -q https://get.helm.sh/helm-${HELM_VERSION}-linux-amd64.tar.gz -O - | tar -xzO linux-amd64/helm > /usr/local/bin/helm \
    && chmod +x /usr/local/bin/helm 

RUN helm repo add stable https://kubernetes-charts.storage.googleapis.com/  && \
    helm repo add jetstack "https://charts.jetstack.io"  && \
    helm repo add rancher-latest "https://releases.rancher.com/server-charts/latest"  && \
    helm repo update

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
COPY . /app

WORKDIR /app
RUN dotnet restore
RUN dotnet test

WORKDIR /app/installer
RUN dotnet publish  -c Release -o /app/installer/out

FROM run
WORKDIR /app
COPY --from=build /app/installer/out ./
COPY ./ssl /ssl
COPY ./components /components

ENTRYPOINT [ "dotnet", "installer.dll" ]
CMD ["default" ]