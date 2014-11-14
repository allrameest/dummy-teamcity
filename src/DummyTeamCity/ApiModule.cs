using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace DummyTeamCity
{
    public class ApiModule : NancyModule
    {
        private const string BaseUrl = "/guestAuth/app/rest";

        private static readonly IDictionary<string, Project> ProjectMap = new Dictionary<string, Project>();
        private static readonly IDictionary<string, BuildType> BuildTypeMap = new Dictionary<string, BuildType>();
        private static readonly Random Rnd = new Random();

        static ApiModule()
        {
            var foo = new Project
            {
                Id = "Foo",
                Name = "Foo",
                ParentId = "_Root",
                BuildTypes = new[] { "Foo_Build", "Foo_Test" }
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

            ProjectMap.Add("Foo", foo);

            BuildTypeMap.Add("Foo_Build", fooBuild);
            BuildTypeMap.Add("Foo_Test", fooTest);
        }

        public ApiModule()
            : base(BaseUrl)
        {
            Get["/projects"] = _ => Projects();
            Get["/projects/id:{id}"] = x => Project(x.id);
            Get["/buildTypes/id:{id}"] = x => BuildType(x.id);
            Get["/buildTypes/id:{id}/builds/"] = x => Builds(x.id);

            After += ctx => ctx.Response
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type");
        }

        private static object Projects()
        {
            return new
            {
                count = ProjectMap.Count,
                project = (from kvp in ProjectMap
                    let id = kvp.Key
                    let project = kvp.Value
                    select new
                    {
                        id,
                        name = project.Name,
                        parentProjectId = project.ParentId,
                        href = string.Format("{0}/projects/id:{1}", BaseUrl, id)
                    }).ToArray()
            };
        }

        private static object Project(string projectId)
        {
            Project project;
            if (!ProjectMap.TryGetValue(projectId, out project))
            {
                return 404;
            }

            var buildTypes = (from btId in project.BuildTypes
                let bt = BuildTypeMap[btId]
                select new
                {
                    id = btId,
                    name = bt.Name,
                    projectName = project.Name,
                    projectId,
                    href = string.Format("{0}/buildTypes/id:{1}", BaseUrl, btId)
                }).ToArray();

            return new
            {
                id = projectId,
                name = project.Name,
                parentProjectId = project.ParentId,
                href = string.Format("{0}/projects/id:{1}", BaseUrl, projectId),
                buildTypes = new
                {
                    count = project.BuildTypes.Count,
                    buildType = buildTypes
                },
                projects = new
                {
                    count = 0,
                    project = new object[0]
                }
            };
        }

        private static object BuildType(string buildTypeId)
        {
            BuildType buildType;
            if (!BuildTypeMap.TryGetValue(buildTypeId, out buildType))
            {
                return 404;
            }

            var project = buildType.Project;

            return new
            {
                id = buildTypeId,
                name = buildType.Name,
                projectName = project.Name,
                projectId = project.Id,
                href = string.Format("{0}/buildTypes/id:{1}", BaseUrl, buildTypeId),
                project = new
                {
                    id = project.Id,
                    name = project.Name,
                    parentProjectId = project.ParentId,
                    href = string.Format("{0}/projects/id:{1}", BaseUrl, project.Id),
                },
                builds = new
                {
                    href = string.Format("{0}/buildTypes/id:{1}/builds/", BaseUrl, buildTypeId)
                }
            };
        }

        private static object Builds(string buildTypeId)
        {
            BuildType buildType;
            if (!BuildTypeMap.TryGetValue(buildTypeId, out buildType))
            {
                return 404;
            }

            return new
            {
                count = 1,
                href = string.Format("{0}/buildTypes/id:{1}/builds/", BaseUrl, buildTypeId),
                nextHref = "?",
                build = new[]
                {
                    new
                    {
                        id = 1,
                        buildTypeId,
                        number = "1",
                        status = Rnd.Next(2) == 0 ? "SUCCESS" : "FAILED",
                        state = "finished"
                    }
                }
            };
        }
    }
}