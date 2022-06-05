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

5.Queue service api x 2

6.Traffic manager node

The resources will be created at a resource group called 'HomeWork2-cloudcompute-rg-' concatenated with a randomIdentifier



## design
In our implementation we created the following nodes:

1.Worker node- this node will take the message from the 'requests' queue process the message and place the response in the 'completed' queue

2.Queue node- this node is an implementation of a queue, it holds 2 queues -'requests' and 'completed' messages.

3.Queue service Api- this node has 2 endpoints of 'enqueue' and 'pullCompleted'. the node will send the message got for the 'enqueue' and will place it in the 
'request' queue.  message for 'pullCompleted' will call the queue node and get the completed messages and return it.

4.Traffic manager- will have the application URL that will route the requests into 2 instances of 'queue service api'. the requests are routed in random to one of the queue service api nodes.


we added to each node a 'swagger' api for debugging and troubleshooting, for that after each resource is created we can go to the resource endpoint and add 'swagger'

Mock URL :

'https://hw2-web-app-traffic-manager-1174986663.azurewebsites.net/swagger'


![image](https://user-images.githubusercontent.com/25264394/172049830-eb213200-de48-49c6-8805-c2a2c192ccbc.png)


We also added swagger to check the Queue status, each queue (requests and completed) has a GET API to check the number of items in the queue, and also we have an api to check with is the longest waiting message in the request queue

![image](https://user-images.githubusercontent.com/25264394/172049899-cef2073b-c928-4a44-8908-397bc6a3ec07.png)


Scaling rules:

our scale manager nodes is checking the requests queue each seconds for duration of a minutes, in each check we are doing peek to the queue and checking what is the oldest message waiting time in the queue, if we have more then 20 events that a message waited for more then 3 seconds we are creating a new worker and adding it to the pool of workers.

worker deletion - we are trying to optimize the work rather than cost (same for production, we will prefer to pay more to serve more requests and wait for their completion then stop the work in progress).
If we are getting more than 45 events that the message is waiting less than 3 seconds we are calling the worker to stop working,which means it will not take any more messages from the queue and wait for it to complete the current message ( we are using 'isBusy' api to check when the worker will stop working).
After we get that the worker has stopped working we are deleting the worker.


## notes
1.the free plan that we are using doesn't enable the 'Always On' feature, after 20m that the app is idle it is turned off automatically
(https://docs.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal) "Always On: Keeps the app loaded even when there's no traffic. When Always On is not turned on (default), the app is unloaded after 20 minutes without any incoming requests. The unloaded app can cause high latency for new requests because of its warm-up time.

2.network connection limitations
since we are using free sku we are limited 250 connections per instance,which means that more than that we will get connection failures to the servers

https://www.freekpaans.nl/2015/08/starving-outgoing-connections-on-windows-azure-web-sites/

3.since we are using the free SKU we have a 
we got quota encforment
https://docs.microsoft.com/en-us/azure/app-service/web-sites-monitor

and after a few minutes of usage we can get to CPU quota limit and get:

![image](https://user-images.githubusercontent.com/25264394/172049393-a5714d4e-807f-4488-8764-1f34b85cedec.png)

we can see it in the app service dashboard:

![image](https://user-images.githubusercontent.com/25264394/172049358-de6a3b31-ad5f-4ad6-ae1f-3472cc53167f.png)




## If the system was made for production:
0. Choosing the right SKU

As we SAW using the free SKU has its quota limitations, in production we would do capacity planning and according to the expected load we will choose the SKU such that we won't get to a point that the app is stopped due to lack of quota.

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



