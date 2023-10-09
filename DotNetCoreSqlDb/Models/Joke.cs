using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class Joke
    {
        public int ID { get; set; }
        public string? Type { get; set; }
        public string? Setup { get; set; }
        public string? Punchline { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
