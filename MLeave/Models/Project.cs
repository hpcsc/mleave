
using System.Collections.Generic;
namespace MLeave.Models
{
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
