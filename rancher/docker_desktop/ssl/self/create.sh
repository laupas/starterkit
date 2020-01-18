#!/usr/bin/env bash

# Specify where we will install
# the xip.io certificate
SSL_DIR="."


# A blank passphrase
PASSPHRASE="devops"

# Set our CSR variables
SUBJ1="
C=CH
ST=Lucern
O=
localityName=StarterKit
commonName=StarterKit Root CA
organizationalUnitName=StarterKit
emailAddress=starterkit@localhost
"

SUBJ2="
C=CH
ST=Lucern
O=
localityName=StarterKit
commonName=StarterKit Dev CA
organizationalUnitName=StarterKit
emailAddress=starterkit@localhost
"

# Generate our Private Key, CSR and Certificate
openssl genrsa -des3 -out "$SSL_DIR/rootCA.key"  -passout  pass:$PASSPHRASE 2048
openssl req -subj "$(echo -n "$SUBJ1" | tr "\n" "/")" -x509 -new -nodes -key "$SSL_DIR/rootCA.key" -sha256 -days 1024  -out "$SSL_DIR/cacerts.pem" -passin pass:$PASSPHRASE

openssl req -new -nodes -subj "$(echo -n "$SUBJ2" | tr "\n" "/")" -out "$SSL_DIR/server.csr" -newkey rsa:2048 -keyout "$SSL_DIR/server.key"
openssl x509 -req -in server.csr -CA "$SSL_DIR/cacerts.pem" -CAkey "$SSL_DIR/rootCA.key" -CAcreateserial -out "$SSL_DIR/server.crt" -days 500 -sha256 -extfile "$SSL_DIR/v3.ext" -passin pass:$PASSPHRASE

kubectl -n ingress-nginx create secret tls ingress-default-cert --cert="$SSL_DIR/server.crt" --key="$SSL_DIR/server.key" -o yaml --dry-run=true > ingress-default-cert.yaml
