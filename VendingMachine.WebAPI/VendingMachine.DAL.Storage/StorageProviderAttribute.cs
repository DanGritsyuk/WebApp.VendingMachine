namespace VendingMachine.DAL.Storage
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StorageProviderAttribute : Attribute
    {
        public StorageProviderAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
