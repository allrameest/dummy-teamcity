using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;

namespace DummyTeamCity
{
    public class ManageModule : NancyModule
    {
        public ManageModule()
        {
            Get["/"] = x =>
            {
                var model = new
                {
                    Projects = Storage.ProjectMap.Values
                        .Select(p => new
                        {
                            p.Id,
                            p.Name,
                            BuildTypes = p.BuildTypes
                                .Select(btId => Storage.BuildTypeMap[btId])
                                .Select(bt => new
                                {
                                    bt.Id,
                                    bt.Name
                                })
                                .ToArray()
                        })
                        .ToArray()
                };
                return View["Home.handlebars", model];
            };

            Post["/build"] = x =>
            {
                var data = this.Bind<BuildPostData>();

                var builds = Storage.BuildMap.GetOrAdd(data.Id, _ => new List<Build>());
                builds.Insert(0, new Build
                {
                    Id = Guid.NewGuid().ToString(),
                    Success = data.Success
                });

                return Response.AsRedirect("/");
            };
        }

        private class BuildPostData
        {
            public string Id { get; set; }
            public bool Success { get; set; }
        }
    }
}