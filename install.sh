#!/usr/bin/bash

# minikube stop
# minikube delete --all
# minikube start

minikube addons enable dashboard
minikube addons enable default-storageclass
minikube addons enable logviewer
minikube addons enable metrics-server
minikube addons enable storage-provisioner

# helm install mongo stable/mongodb --set mongodbRootPassword=a,mongodbUsername=mova,mongodbPassword=a,mongodbDatabase=api
# helm install mongo stable/mongodb --set mongodbRootPassword=a
helm install mongo stable/mongodb --set mongodbRootPassword=a,replicaSet.enabled=true

helm install rabbit stable/rabbitmq --set rabbitmq.username=mova,rabbitmq.password=a

# minikube dashboard
