namespace TrafficManager.Controllers;

public interface ILoadBalancerClass
{
    string GetNodeUrl();
}

public class LoadBalancerClass : ILoadBalancerClass
{
    private readonly string nodeApiA;
    private readonly string nodeApiB;
    public LoadBalancerClass(string nodeApiA, string nodeApiB)
    {
        this.nodeApiA = nodeApiA;
        this.nodeApiB = nodeApiB;
    }
    public string GetNodeUrl()
    {
        string hostName;
        Random rand = new Random();

        if (rand.Next(0, 2) == 0)
        {
            hostName = nodeApiA;
        }
        else
        {
            hostName = nodeApiB;
        }

        return hostName;
    }
}