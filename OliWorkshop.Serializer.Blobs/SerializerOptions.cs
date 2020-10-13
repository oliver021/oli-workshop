namespace OliWorkshop.Serializer.Blobs
{
    public class SerializerOptions
    {
        public static SerializerOptions Default => new SerializerOptions { };

        public bool InFields = true;
        public bool InProperties = false;
        public bool AllowSubTypes = true;
    }
}