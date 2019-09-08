#!/bin/bash -x

rancher_ip=${1}
admin_password=${2:-password}
rancher_version=${3:-latest}

echo "################################################################"
echo "rancher_ip:${rancher_ip}"
echo "admin_password:${admin_password}"
echo "rancher_version:${rancher_version}"
echo "################################################################"

echo "Start Rancher"
docker run -d --restart=unless-stopped -p 81:80 -p 444:443 -v /opt/rancher:/var/lib/rancher rancher/rancher:${rancher_version}

echo "wait until rancher server is ready"
while true; do
  curl -sLk https://127.0.0.1:444/ping && break
  sleep 5
done

echo "Login"
while true; do

    LOGINRESPONSE=$(curl "https://127.0.0.1:444/v3-public/localProviders/local?action=login" -H 'content-type: application/json' --data-binary '{"username":"admin","password":"admin"}' --insecure)
    LOGINTOKEN=$(echo $LOGINRESPONSE | jq -r .token)

    if [ "$LOGINTOKEN" != "null" ]; then
        break
    else
        sleep 5
    fi
done

echo "Change password"
curl -s 'https://127.0.0.1:444/v3/users?action=changepassword' -H 'content-type: application/json' -H "Authorization: Bearer $LOGINTOKEN" --data-binary '{"currentPassword":"admin","newPassword":"'$admin_password'"}' --insecure

echo "Create API key"
APIRESPONSE=$(curl -s 'https://127.0.0.1:444/v3/token' -H 'content-type: application/json' -H "Authorization: Bearer $LOGINTOKEN" --data-binary '{"type":"token","description":"automation"}' --insecure)

echo "Extract and store token"
export APITOKEN=`echo $APIRESPONSE | jq -r .token`

echo "Configure server-url"
RANCHER_SERVER="https://${rancher_ip}:444"
curl -s 'https://127.0.0.1:444/v3/settings/server-url' -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" -X PUT --data-binary '{"name":"server-url","value":"'$RANCHER_SERVER'"}' --insecure

echo "Create cluster"
CLUSTERRESPONSE=$(curl -s 'https://127.0.0.1:444/v3/cluster' -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" --data-binary ' {
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
export CLUSTERID=`echo $CLUSTERRESPONSE | jq -r .id`

echo "Generate registrationtoken"
TOKEN=$(curl -s 'https://127.0.0.1:444/v3/clusterregistrationtoken' -H 'content-type: application/json' -H "Authorization: Bearer $APITOKEN" --data-binary '{"type":"clusterRegistrationToken","clusterId":"'$CLUSTERID'"}' --insecure)

echo "Create configs"
NODECOMMAND=`echo $TOKEN | jq -r .nodeCommand`
KUBECONFIG=$(curl -s -X POST "https://${rancher_ip}:444/v3/clusters/${CLUSTERID}?action=generateKubeconfig" -H "content-type: application/json" -H "Authorization: Bearer $APITOKEN" --insecure)

mkdir -p /var/lidop/rancher
echo "${KUBECONFIG}" | jq -r .config > /var/lidop/rancher/config
echo $NODECOMMAND  >> /var/lidop/rancher/installNode.sh
echo $CLUSTERID > /var/lidop/rancher/clusterid
echo $APITOKEN > /var/lidop/rancher/token
