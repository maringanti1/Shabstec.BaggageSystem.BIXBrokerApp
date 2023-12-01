namespace BlazorApp.API.Models
{ 
    public class CosmosDbSettings
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public string Environment { get; set; }
        
    } 
}