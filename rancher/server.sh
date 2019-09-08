#!/bin/bash -x

rancher_ip=${1}
admin_password=${2:-password}
rancher_version=${3:-latest}
export curlimage="admin curl"
export jqimage="admin jq"


echo "################################################################"
echo "rancher_ip:${rancher_ip}"
echo "admin_password:${admin_password}"
echo "rancher_version:${rancher_version}"
echo "k8s_version:${k8s_version}"
echo "################################################################"

echo "################################################################"
echo "Configure curl, kubectl and helm"
echo "################################################################"
docker build -t admin /var/lidop 
echo 'docker run -i --rm admin curl $@' | sudo tee /usr/bin/curl && sudo chmod +x /usr/bin/curl
echo 'docker run -i --rm -v /var/lidop/kubeconfig:/root/.kube/config admin helm $@' | sudo tee /usr/bin/helm && sudo chmod +x /usr/bin/helm
echo 'docker run -i --rm -v /var/lidop/kubeconfig:/root/.kube/config admin kubectl $@' | sudo tee /usr/bin/kubectl && sudo chmod +x /usr/bin/kubectl

echo "################################################################"
echo "Download images"
echo "################################################################"
docker pull rancher/rancher:${rancher_version}
docker pull rancher/rancher-agent:${rancher_version}
docker save rancher/rancher:${rancher_version} > /var/lidop/rancher.tar
docker save rancher/rancher-agent:${rancher_version} > /var/lidop/rancher-agent.tar

echo "################################################################"
echo "Start Rancher"
echo "################################################################"
docker run -d --restart=unless-stopped -p 81:80 -p 444:443 -v /opt/rancher:/var/lib/rancher rancher/rancher:${rancher_version}

echo "wait until rancher server is ready"
while true; do
  docker run --rm --net=host $curlimage -sLk https://127.0.0.1:444/ping && break
  sleep 5
done

echo "Login"
while true; do

    LOGINRESPONSE=$(docker run \
        --rm \
        --net=host \
        $curlimage \
        -s "https://127.0.0.1:444/v3-public/localProviders/local?action=login" -H 'content-type: application/json' --data-binary '{"username":"admin","password":"admin"}' --insecure)
    LOGINTOKEN=$(echo $LOGINRESPONSE | docker run --rm -i $jqimage -r .token)

    if [ "$LOGINTOKEN" != "null" ]; then
        break
    else
        sleep 5
    fi
done


echo "Change password"
docker run --rm --net=host $curlimage -s 'https://127.0.0.1:444/v3/users?action=changepassword' -H 'content-type: application/json' -H "Authorization: Bearer $LOGINTOKEN" --data-binary '{"currentPassword":"admin","newPassword":"'$admin_password'"}' --insecure

echo "Create API key"
APIRESPONSE=$(docker run --rm --net=host $curlimage -s 'https://127.0.0.1:444/v3/token' -H 'content-type: application/json' -H "Authorization: Bearer $LOGINTOKEN" --data-binary '{"type":"token","description":"automation"}' --insecure)

echo "Extract and store token"
export APITOKEN=`echo $APIRESPONSE | docker run --rm -i $jqimage -r .token`

echo "Configure server-url"
RANCHER_SERVER="https://${rancher_ip}:444"
docker run --rm --net=host $curlimage -s 'https://127.0.0.1:444/v3/settings/server-url' -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" -X PUT --data-binary '{"name":"server-url","value":"'$RANCHER_SERVER'"}' --insecure

echo "Create cluster"
CLUSTERRESPONSE=$(docker run --rm --net=host $curlimage -s 'https://127.0.0.1:444/v3/cluster' -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" --data-binary ' {
 	"type": "cluster",
 	"dockerRootDir": "/var/lib/docker",
 	"enableNetworkPolicy": false,
 	"enableClusterMonitoring": false,
 	"enableClusterAlerting": false,
 	"localClusterAuthEndpoint": {
 		"type": "localClusterAuthEndpoints",
 		"enabled": true
 	},
 	"rancherKubernetesEngineConfig": {
 		"type": "rancherKubernetesEngineConfig",
 		"addonJobTimeout": 30,
 		"kubernetesversion": "v1.13.5-rancher1-2",
 		"ignoreDockerVersion": true,
 		"sshAgentAuth": false,
 		"authentication": {
 			"type": "authnConfig",
 			"strategy": "x509"
 		},
 		"network": {
 			"options": {
 				"flannel_backend_type": "vxlan"
 			},
 			"type": "networkConfig",
 			"plugin": "flannel"
 		},
 		"ingress": {
 			"type": "ingressConfig",
 			"provider": "nginx"
 		},
 		"services": {
 			"type": "rkeConfigServices",
 			"kubeApi": {
 				"type": "kubeAPIService",
 				"always_pull_images": false,
 				"podSecurityPolicy": false,
 				"service_node_port_range": "30000-32767"
 			},
 			"etcd": {
 				"type": "etcdService",
 				"snapshot": false,
 				"retention": "72h",
 				"creation": "12h",
 				"backup_config": {
 					"enabled": true,
 					"interval_hours": 12,
 					"retention": 6
 				},
 				"extraArgs": {
 					"heartbeat-interval": 500,
 					"election-timeout": 5000
 				}
 			}
 		}
 	},
 	"name": "lidop"
 }' --insecure)

echo "Extract clusterid to use for generating the docker run command"
export CLUSTERID=`echo $CLUSTERRESPONSE | docker run --rm -i $jqimage -r .id`

echo "Generate registrationtoken"
TOKEN=$(docker run --rm --net=host $curlimage -s 'https://127.0.0.1:444/v3/clusterregistrationtoken' -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" --data-binary '{"type":"clusterRegistrationToken","clusterId":"'$CLUSTERID'"}' --insecure)

echo "################################################################"
echo "Create configs"
echo "################################################################"
NODECOMMAND=`echo $TOKEN | docker run --rm -i $jqimage -r .nodeCommand`
echo $NODECOMMAND  >> /var/lidop/node.sh

KUBECONFIG=$(docker run --rm --net=host $curlimage -s -X POST "https://${rancher_ip}:444/v3/clusters/${CLUSTERID}?action=generateKubeconfig" -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" --insecure)
echo "${KUBECONFIG}" | docker run --rm -i $jqimage -r .config > /var/lidop/kubeconfig
cat /var/lidop/kubeconfig

echo $CLUSTERID > /var/lidop/clusterid
echo $APITOKEN > /var/lidop/token

export CLUSTERID=$(cat /var/lidop/clusterid)
export APITOKEN=$(cat /var/lidop/token)

