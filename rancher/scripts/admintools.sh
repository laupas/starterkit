#!/bin/bash -x

if [ -f /var/starterkit/images/admin.tar ]; then
    echo "import existing admin image"
    docker load < /var/starterkit/images/admin.tar
else
    echo "build admin image"
    docker build -t admin /var/starterkit/scripts
    mkdir -p /var/starterkit/images
    docker save admin > /var/starterkit/images/admin.tar
fi

echo "Configure curl, kubectl and helm"
echo 'docker run --net=host -i --rm admin curl "$@"' | sudo tee /usr/bin/curl && sudo chmod +x /usr/bin/curl
echo 'docker run --net=host -i --rm -v /var/starterkit:/var/starterkit -v /root:/root admin helm "$@"' | sudo tee /usr/bin/helm && sudo chmod +x /usr/bin/helm
echo 'docker run --net=host -i --rm -v /var/starterkit:/var/starterkit -v /root:/root admin kubectl "$@"' | sudo tee /usr/bin/kubectl && sudo chmod +x /usr/bin/kubectl
