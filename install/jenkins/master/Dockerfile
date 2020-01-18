FROM jenkins/jenkins:lts-alpine

USER root
#RUN git config --system http.sslCAInfo /certs/rootCA.pem
RUN git config --system http.sslVerify false

USER jenkins
COPY plugins.txt /usr/share/jenkins/ref/plugins.txt
RUN /usr/local/bin/install-plugins.sh < /usr/share/jenkins/ref/plugins.txt