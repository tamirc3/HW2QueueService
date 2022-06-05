# HW2QueueService

Assignment 2 for cloud computing course Computer Science MSc Idc

## Prerequisite:
1.create a free subscription in Azure (https://azure.microsoft.com/en-us/free/) 

2.download and install Azure CLI (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli)

## How to run:

rung the PS script Assignment2.ps1, the script will create:

1.Resource group

2.App service plan

3.Queue node

4.Worker node

5.Queue service api x 2 (which has the enqueue and pullCompleted APIs)

6.Traffic manager node

The resources will be created at a resource group called 'HomeWork2-cloudcompute-rg-' concatenated with a randomIdentifier. During runtime when there is a need to create addtional workers they will be created at a resource group called 'workers'



## design
In our implementation we created the following nodes:

1.Worker node- this node will take the message from the 'requests' queue process the message and place the response in the 'completed' queue. We are creating one worker by default menaning that we will always have at least one worker node in the system. In each worker node we are creating long running tasks as the number of cores in the server in order to maximize the work in that node.

The code is in:https://github.com/evyatarweiss/HW2_WorkerNode

The worker resource will be called hw2-web-app-WorkerNode1-<randomIdentifier>

2.Queue node- this node is an implementation of a queue, it holds 2 queues -'requests' and 'completed' messages, each queue is an in memory queue.

The queue resource will be called hw2CloudComputing-queue-endpoint- <randomIdentifier>

The code is in:https://github.com/evyatarweiss/HW2_QueueAPI

3.Queue service Api- this node has 2 endpoints of 'enqueue' and 'pullCompleted'. the node will send the message got from the traffic manager for the 'enqueue' and will place it in the 'request' queue.  message from the traffic manager for 'pullCompleted' will call the queue node and get the completed messages and return it.

The code is in :https://github.com/evyatarweiss/HW2_QueueServiceAPI
  
The first api resource will be called hw2-CloudComputing-web-app-API1-<randomIdentifier>
The second api resource will be called hw2-CloudComputing-web-app-API2-<randomIdentifier>

  
4.Scale manger - this node job it create an autoscale functionality for the worker nodes, it will check that queue status (detailed explnation in the next section 'Scaling rules') abd will create and delete workers as needed.

The code is in:https://github.com/tamirc3/HW2_ScaleManager
  
The resource will be called hw2-web-app-Scale-Manager-<randomIdentifier>
  
5.Traffic manager- will have the application URL that will get requests from clients and will route the requests into 2 instances of 'queue service api'. the requests are routed in random to one of the queue service api nodes.

The code is in:https://github.com/evyatarweiss/HW2_TrafficManager

The resource will be called hw2-web-app-Traffic-Manager-<randomIdentifier>

The url for the application to send enqueue and pullCompleted is (see in the next section swagger example):
https://hw2-web-app-traffic-manager-<randomIdentifier>.azurewebsites.net/
  
**Example request for enqueue, PUT request**
https://hw2-web-app-traffic-manager-1174986663.azurewebsites.net/enqueue?iterations=50

and the string content is in the body of the request.
  
the response will be an Id to track the request, for example:
 
![image](https://user-images.githubusercontent.com/25264394/172063020-79021efd-7bb5-48fb-aa10-2d9449a3bffc.png)

**Example request for pullCompleted, POST request**

the url will be: https://hw2-web-app-traffic-manager-1174986663.azurewebsites.net/pullCompleted?top=10
  
Example response 
  
![image](https://user-images.githubusercontent.com/25264394/172063078-71d620eb-f93e-4fa4-95a9-90cb5eb0ab27.png)

  
'buffer' is the hased result of the data, 'id' is the id of the request that was genereated in the enqueue request
  
  
**Swagger:**


We added to each node a 'swagger' api for troubleshooting and diagnostics,after each resource is created we can go to the resource endpoint and add 'swagger'

Mock URL :

'https://hw2-web-app-traffic-manager-1174986663.azurewebsites.net/swagger'


![image](https://user-images.githubusercontent.com/25264394/172049830-eb213200-de48-49c6-8805-c2a2c192ccbc.png)


We also added swagger to check the Queue status, each queue (requests and completed) has a GET API to check the number of items in the queue, and also we have an api to check what is the longest waiting message in the request queue which is used by the scaling manager to decide if to create/delete worker nodes.

![image](https://user-images.githubusercontent.com/25264394/172049899-cef2073b-c928-4a44-8908-397bc6a3ec07.png)
  
 Worker node swagger:
  
![image](https://user-images.githubusercontent.com/25264394/172063334-5fd97727-7fda-44e9-87fd-036d2fadd496.png)


**Scaling rules:**

Our scale manager nodes is checking the requests queue each second for the duration of one minute. in each check we are doing peek to the queue and checking what is the oldest message waiting time in the queue, if we have more then 20 events that a message waited for more then 3 seconds we are creating a new worker and adding it to the pool of workers.

worker deletion - we are trying to optimize the work rather than cost (same for production, we will prefer to pay more to serve more requests and wait for their completion then stop the work in progress).
If we are getting more than 45 events that the message is waiting less than 3 seconds we are calling the worker to stop working ('stopworking' api), which means it will not take any more messages from the queue and wait for it to complete the current message ( we are using 'IsBusy' api to check if the worker currently has a message that its working on-if yes wait for it to complete). After we get that the worker has stopped working we are deleting the worker and its app service plan.


## notes
1.the free plan that we are using doesn't enable the 'Always On' feature, after 20m that the app is idle it is turned off automatically
(https://docs.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal) "Always On: Keeps the app loaded even when there's no traffic. When Always On is not turned on (default), the app is unloaded after 20 minutes without any incoming requests. The unloaded app can cause high latency for new requests because of its warm-up time.

2.network connection limitations
since we are using free sku we are limited 250 connections per instance,which means that more than that we will get connection failures to the servers

https://www.freekpaans.nl/2015/08/starving-outgoing-connections-on-windows-azure-web-sites/

3.since we are using the free SKU we have a we got quota encforment
https://docs.microsoft.com/en-us/azure/app-service/web-sites-monitor

so its possible that aftera few minutes of usage we can get to CPU quota limit and get:

![image](https://user-images.githubusercontent.com/25264394/172049393-a5714d4e-807f-4488-8764-1f34b85cedec.png)

we can see it in the app service dashboard:

![image](https://user-images.githubusercontent.com/25264394/172049358-de6a3b31-ad5f-4ad6-ae1f-3472cc53167f.png)

once hitting the quota limit the app is stopped, the mitigation is to restart / wait a few minutes to regain our quota.



## If the system was made for production:
0. Choosing the right SKU

As we saw using the free SKU has its quota limitations, in production we would do capacity planning and according to the expected load we will choose the SKU such that we won't get to a point that the app is stopped due to lack of quota and has sufficent amount of connections.

1.Security

The current implementation does not have any authentication and authorization which makes the system vulnerable for attackers.In case of production systems all the APIs should not be public and should be authentication using for example a JWT token issued by AAD and have an allowed list of applications that can send requests.
In case we want our service to be publicly available we will keep the traffic manager endpoint authentication, but all other apis will be authenticated since we only want to expose the 'enqueue'  and 'pullCompleted' APIs

2.Latency

right now we are only using one region- EastUS, which means that users not near to that regions will have high latency. If it was a production environment we would create several clusters of the application in a few regions across the globe,such that each user will be routed to the nearest data center in his geo.


3.Single point of failure
In our implementation we have 3 points of failure- the Traffic manager,scale manager and queue.
Scale manager:
 in a production environment we can have 2 options for handling the scale manager, create 2 instances of it one 'on' and one 'off' and in case of failure we will do a failover to the backup server.
 The second option is that in case of a failure don't use that scale manager or even the entire resources in the region such that all the requests will be routed to another cluster.

4.Scaling/Queue size

We are using the free Sku and have our 2 queues on one server, if we want to increase our scale we can scale up the server for more memory but that can not scale indefinitely.
in order to be able to continue and scale we need to scale out, for that we need to implement the queue in such a way that we can divide the message into several queues in several servers, to do that we need to use partitioning such that we will partition the message into different queue servers.

5.Failures
In our implementation we don't handle failures, in a production environment we will need to handle failures in order to avoid data loss.

Persistency- the queue that we implement we are using in memory queue, in production we will write the message also to the disk such that if the server will restart we will not lose any message.
As mentioned above in order to have a global application we will have the cluster in several regions across the globe meaning each region will have its own queue, our assumption is that there is no need to server client cross regions, if we will need to do it we will have a one centralized queue which is divided into several servers and partitions.

Failures in the worker- in case there is an issue with the worker like a crashing/ network issue, we don't want to lose messages that the worker took.
For that we will need to do 'soft delete' approach meaning when the worker took a message from the queue we really remove it from the queue only when the worker will send back a response that it completed the work on that message, while the worker is working on the message the message will be marked in a way that other workers won't fetch it from the queue. For implementing the above logic we will need to implement the queue using a DB which we will created a queue abstraction for it.

Failure in the Scale manager - in case there is a crash/ the server is in unhealthy state we will do a fail over to a back up scale manager


6.network limitation
as we seen in the free SKU there a limitation of the number of connections to the server, in production environment we will do capacity planning for the application and get an estimation of how many requests we expect to get, according to that we will select the right SKU and allocate enough servers to distribute the load between them.

7.monitoring and telemetry
in a production environment we will need to gather telemetry about how the application is working, we would collect the logs and create monitoring for success/failures such that if there is an incident we will be aware and engage to resolve it.

8.BCDR (Business Continuity Disaster Recovery)
our application needs to be 'disaster safe' meaning that if one region will be down our application will continue running.
creating several replicas of the application in several regions will enable us to have high availability and in case of a failures in one of Azure regions stop using that region and routing the traffic to another region



