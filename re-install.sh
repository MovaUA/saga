#!/usr/bin/bash

helm delete mongo
kubectl delete pvc mongo-mongodb
helm delete rabbit
kubectl delete pvc data-rabbit-rabbitmq-0

# helm install mongo stable/mongodb --set mongodbRootPassword=a,mongodbUsername=mova,mongodbPassword=a,mongodbDatabase=api
# helm install mongo stable/mongodb --set mongodbRootPassword=a
helm install mongo stable/mongodb --set mongodbRootPassword=a,replicaSet.enabled=true

helm install rabbit stable/rabbitmq --set rabbitmq.username=mova,rabbitmq.password=a
