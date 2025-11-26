namespace DocumentManagerApi.Models
{
    public class Document {
        public int Id {get; set;}
        public string Title {get; set;} = "";
        public string Description {get; set;} = "";
        public List<string> Tags {get; set;} = new();
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    }
}
