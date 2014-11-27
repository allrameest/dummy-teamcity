using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DummyTeamCity
{
    public static class Storage
    {
        public static readonly ConcurrentDictionary<string, Project> ProjectMap =
            new ConcurrentDictionary<string, Project>();

        public static readonly ConcurrentDictionary<string, BuildType> BuildTypeMap =
            new ConcurrentDictionary<string, BuildType>();

        public static readonly ConcurrentDictionary<string, IList<Build>> BuildMap =
            new ConcurrentDictionary<string, IList<Build>>();

        static Storage()
        {
            var foo = new Project
            {
                Id = "Foo",
                Name = "Foo",
                ParentId = "_Root",
                BuildTypes = new[] {"Foo_Build", "Foo_Test"}
            };

            var fooBuild = new BuildType
            {
                Id = "Foo_Build",
                Name = "build",
                Project = foo
            };
            var fooTest = new BuildType
            {
                Id = "Foo_Test",
                Name = "test",
                Project = foo
            };

            ProjectMap.TryAdd("Foo", foo);

            BuildTypeMap.TryAdd("Foo_Build", fooBuild);
            BuildTypeMap.TryAdd("Foo_Test", fooTest);
        }
    }
}