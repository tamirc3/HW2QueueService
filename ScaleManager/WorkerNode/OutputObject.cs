namespace WorkerNode
{
    public class OutputObject
    {
        string id { get; set; }
        string hashedOutput { get; set; }
        public OutputObject(string id, string hashedOutput)
        {
            this.id = id;
            this.hashedOutput = hashedOutput;
        }

    }
}
