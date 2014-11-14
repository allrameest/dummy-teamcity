using System.Collections.Generic;

namespace DummyTeamCity
{
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public IReadOnlyCollection<string> BuildTypes { get; set; }
    }
}