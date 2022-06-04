# HW2QueueService

Assignment 2 for cloud computing course Computer Science MSc Idc

## Prerequisite:
1.create a free subscription in Azure (https://azure.microsoft.com/en-us/free/) 2.download and install Azure CLI (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli)

## How to run:

## notes
1.the free plan that we are using doesn't enable the 'Always On' feature, after 20m that the app is idle its turned off automatically
(https://docs.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal) "Always On: Keeps the app loaded even when there's no traffic. When Always On is not turned on (default), the app is unloaded after 20 minutes without any incoming requests. The unloaded app can cause high latency for new requests because of its warm-up time.

2.network connection limitations
since we are using free sku we are limited 250 connections per instance,which means that more then that we will get connection failures to the servers

https://www.freekpaans.nl/2015/08/starving-outgoing-connections-on-windows-azure-web-sites/

## If the system was made for production:

1.Secuirty

the current implmention does not have any authentication and authorization which makes the system vunelrable for attackers.In case of production systems all the APIs should not be public and should be authentication using for example a JWT token issued by AAD and have an allowed list of applications that can send requets.
In case we wnat our service to be publicly avilable we will keep the traffic manager end point unauthenticated, but all other apis will be authetincated since we only want to expose the 'enqueue'  and 'pullCompleted' APIs

2.Latnecy

right now we are only using one region- EastUS, which means that users not near to that regions will have high latency. If it was production enviorment we would create several clusters of the applicaiton in a few regions across the globe,such that each user will be reouted to the nearest data center in his geo.

3.BCDR





4.Single point of failure

5.Scaling/Queue size

6.Failures

7.network limitation

8.monitoring and telemtry




