using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace DummyTeamCity
{
    public class ApiModule : NancyModule
    {
        private const string BaseUrl = "/guestAuth/app/rest";

        private static readonly Random Rnd = new Random();

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
                count = Storage.ProjectMap.Count,
                project = (from kvp in Storage.ProjectMap
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
            if (!Storage.ProjectMap.TryGetValue(projectId, out project))
            {
                return 404;
            }

            var buildTypes = (from btId in project.BuildTypes
                let bt = Storage.BuildTypeMap[btId]
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
            if (!Storage.BuildTypeMap.TryGetValue(buildTypeId, out buildType))
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
            if (!Storage.BuildTypeMap.TryGetValue(buildTypeId, out buildType))
            {
                return 404;
            }

            var builds = Storage.BuildMap.GetOrAdd(buildTypeId, s => new List<Build>());

            return new
            {
                count = 1,
                href = string.Format("{0}/buildTypes/id:{1}/builds/", BaseUrl, buildTypeId),
                nextHref = "?",
                build = builds
                .Select(b => new
                    {
                        id = b.Id,
                        buildTypeId,
                        number = "1",
                        status = b.Success ? "SUCCESS" : "FAILED",
                        state = "finished"
                    })
                .ToArray()
            };
        }
    }
}