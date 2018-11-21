namespace GlutenFree.OddJob.Serializable
{
    public class OddJobSerializedParameter
    {
        public OddJobSerializedParameter()
        {

        }

        public OddJobSerializedParameter(string name, object value)
        {
            Name = name;
            Value = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            TypeName = value.GetType().AssemblyQualifiedName;
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public string TypeName { get; set; }

    }
}
