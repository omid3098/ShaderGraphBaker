namespace ShaderGraphBaker
{
    public class BlockFieldDescriptor
    {
        public readonly string type;
        public readonly string name;
        public readonly string description;
        public readonly string id;
        public BlockFieldDescriptor(string type, string name, string id)
        {
            this.type = type;
            this.name = name;
            this.id = id;
        }

        public BlockFieldDescriptor(string type, string name, string description, string id)
        {
            this.type = type;
            this.name = name;
            this.description = description;
            this.id = id;
        }
        public override string ToString() => type + "." + name;
    }
}