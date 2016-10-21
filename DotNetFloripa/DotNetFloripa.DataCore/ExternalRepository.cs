using DotNetFloripa.DataCore.DTO;
using DotNetFloripa.ModelCore;
using DotNetFloripa.ModelCore.Interfaces;
using MiniBiggy;
using MiniBiggy.SaveStrategies;
using MiniBiggy.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DotNetFloripa.Data
{
    public class ExternalRepository : IAppRepository
    {
        public ExternalRepository()
        {
        }

        private readonly TimeSpan ReloadThreshold = new TimeSpan(24, 0, 0);
        private DateTime NextMeetupsLoad;
        private DateTime NextCompaniesLoad;
        private DateTime NextJobsLoad;

        private bool ShouldLoadMeetups() => Events == null || Events.Length == 0 || NextMeetupsLoad < DateTime.Now;
        private bool ShouldLoadCompanies() => Companies == null || Companies.Length == 0 || NextCompaniesLoad < DateTime.Now;
        private bool ShouldLoadJobs() => Jobs == null || Jobs.Length == 0 || NextJobsLoad < DateTime.Now;

        private void LoadMeetups()
        {
            if(ShouldLoadMeetups())
            {
                List<Event> events = new List<Event>();

                try
                {
                    var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri("https://api.meetup.com/");
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = httpClient.GetAsync("dotnetfloripa/events?page=15&status=past,upcoming").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string serializedEvents = response.Content.ReadAsStringAsync().Result;

                        var jsonSerializer = new MiniBiggy.Serializers.JsonSerializer();
                        var meetupEvents = jsonSerializer.Deserialize<MeetupEvent>(System.Text.Encoding.UTF8.GetBytes(serializedEvents));

                        foreach (var meetup in meetupEvents)
                        {
                            events.Add(new Event()
                            {
                                Description = meetup.description,
                                ExternalUrl = meetup.link,
                                Title = meetup.name,
                                ImageUrl = "http://i.imgur.com/2ls3CgF.png",
                                Id = meetup.id,
                                Start = meetup.StartTime(),
                                End = meetup.EndTime()
                            });
                        }
                    }

                    NextMeetupsLoad = DateTime.Now.Add(ReloadThreshold);
                }
                catch (Exception)
                { }

                Events = events.ToArray();
            }
        }

        private void LoadCompanies ()
        {
            if(ShouldLoadCompanies())
            {
                try
                {
                    var companiesBiggy = new PersistentList<Company>(new GitHubDataStore("andrecalil/dotnetfloripa-content/master/companies.json"), new JsonSerializer(), new SaveOnlyWhenRequested());
                    Companies = companiesBiggy.ToArray();

                    NextCompaniesLoad = DateTime.Now.Add(ReloadThreshold);
                }
                catch (Exception)
                {
                    Companies = new Company[0];
                }
            }
        }

        private void LoadJobs()
        {
            if (ShouldLoadJobs())
            {
                try
                {
                    var jobsBiggy = new PersistentList<Job>(new GitHubDataStore("andrecalil/dotnetfloripa-content/master/jobs.json"), new JsonSerializer(), new SaveOnlyWhenRequested());
                    Jobs = jobsBiggy.ToArray();

                    NextJobsLoad = DateTime.Now.Add(ReloadThreshold);
                }
                catch (Exception e)
                {
                    Jobs = new Job[0];
                }
            }
        }

        private Event[] Events { get; set; }
        private Company[] Companies { get; set; }
        private Job[] Jobs { get; set;  }

        public IQueryable<Event> GetEvents()
        {
            LoadMeetups();
            return Events.AsQueryable();
        }

        public Event GetEventBySlug(string slug) => Events.FirstOrDefault(e => e.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));

        public IQueryable<Company> GetCompanies()
        {
            LoadCompanies();
            return Companies.AsQueryable();
        }

        public IQueryable<Job> GetJobs()
        {
            LoadJobs();
            return Jobs.AsQueryable();
        }
    }
}