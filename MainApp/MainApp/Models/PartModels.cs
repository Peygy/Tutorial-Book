using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainApp.Models
{
    // Section -> Subsection -> Chapter -> Subchapter -> Post

    // Model for Part, which is used to merge all parts into one
    public class PartModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Table { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? ParentId { get; set; }

        //Helpfuul options
        [NotMapped]
        public PartModel? Parent { get; set; }
        [NotMapped]
        public IEnumerable<PartModel>? Children { get; set; } = new List<PartModel>();
    }


    public class Section : PartModel {
        new public IEnumerable<Subsection>? Children { get; set; } = new List<Subsection>();
    }
    public class Subsection : PartModel {
        new public Section? Parent { get; set; }
        new public IEnumerable<Chapter>? Children { get; set; } = new List<Chapter>();
    }
    public class Chapter : PartModel {
        new public Subsection? Parent { get; set; }
        new public IEnumerable<Subchapter>? Children { get; set; } = new List<Subchapter>();
    }
    public class Subchapter : PartModel {
        new public Chapter? Parent { get; set; }
        new public IEnumerable<Post>? Children { get; set; } = new List<Post>();
    }
    public class Post : PartModel {
        new public Subchapter? Parent { get; set; }
        public string? ContentId { get; set; }
    }



    // Model for posts content
    public class ContentModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = string.Empty;
        public string Content { get; set; } = "Пост ничего не содержит";
    }
}
