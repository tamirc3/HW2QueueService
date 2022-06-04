namespace TrafficManager.Controllers;

public class ErrorConstants
{
    public const string InvalidRequestDequeue = "top has to be atleast one";
    public const string InvalidRequestEnqueue = "Invalid Input , number of iterations has to be atleast one and request body has to be a string";
}