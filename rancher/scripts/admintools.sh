#!/bin/bash -x

if [ -f /var/lidop/images/admin.tar ]; then
    echo "import existing admin image"
    docker load < /var/lidop/images/admin.tar
else
    echo "build admin image"
    docker build -t admin /var/lidop/scripts
    mkdir -p /var/lidop/images
    docker save admin > /var/lidop/images/admin.tar
fi

echo "Configure curl, kubectl and helm"
echo 'docker run --net=host -i --rm admin curl "$@"' | sudo tee /usr/bin/curl && sudo chmod +x /usr/bin/curl
echo 'docker run --net=host -i --rm -v /var/lidop:/var/lidop -v /root:/root admin helm "$@"' | sudo tee /usr/bin/helm && sudo chmod +x /usr/bin/helm
echo 'docker run --net=host -i --rm -v /var/lidop:/var/lidop -v /root:/root admin kubectl "$@"' | sudo tee /usr/bin/kubectl && sudo chmod +x /usr/bin/kubectl
