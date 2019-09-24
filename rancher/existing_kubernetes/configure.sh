#!/bin/bash -x

password=${1}

echo "###############################"
echo "Configure Rancher"
echo "###############################"

echo "wait until rancher server is started"
while true; do
curl -sLk "https://localhost/ping" && break
  sleep 5
done

echo "Login to rancher"
while true; do

    LOGINRESPONSE=$(curl -s "https://localhost/v3-public/localProviders/local?action=login" \
        -H "content-type: application/json" \
        --data-binary '{"username":"admin","password":"admin"}' --insecure)
    LOGINTOKEN=$(echo $LOGINRESPONSE | jq -r .token)

    if [ "$LOGINTOKEN" != "null" ]; then
        break
    else
        sleep 5
    fi
done

echo "Change password"
curl -s "https://localhost/v3/users?action=changepassword" \
    -H 'content-type: application/json' \
    -H "Authorization: Bearer $LOGINTOKEN" \
    --data-binary '{"currentPassword":"admin","newPassword":"'${password}'"}' \
    --insecure

echo "Create API key"
apiresponse=$(curl -s "https://localhost/v3/token" \
    -H 'content-type: application/json' \
    -H "Authorization: Bearer $LOGINTOKEN" \
    --data-binary '{"type":"token","description":"automation"}' \
    --insecure)

echo "Extract and store token"
export APITOKEN=`echo $apiresponse | jq -r .token`

echo "Configure server-url"
curl -s "https://localhost/v3/settings/server-url" \
    -H 'content-type: application/json' \
    -H "Authorization: Bearer $APITOKEN" \
    -X PUT \
    --data-binary '{"name":"server-url","value":"'https://localhost'"}' \
    --insecure

