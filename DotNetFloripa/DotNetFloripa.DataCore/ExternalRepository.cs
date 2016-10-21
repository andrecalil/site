using DotNetFloripa.DataCore.DTO;
using DotNetFloripa.ModelCore;
using DotNetFloripa.ModelCore.Interfaces;
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
            #region Events from Meetup

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

                    foreach(var meetup in meetupEvents)
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
            }
            catch (Exception)
            { }

            Events = events.ToArray();

            #endregion

            #region Companies from Github



            #endregion

            #region Jobs from Github



            #endregion

            Companies = new[]
            {
                new Company
                {
                    Name = "Way2 Techonology",
                    Description =
                        "A Way2 desenvolve softwares e serviços para telemedição e gestão de dados de medição de energia. Sediada em Florianópolis, Santa Catarina, a empresa possui uma equipe multidisciplinar especializada no atendimento às demandas de empresas do setor elétrico, como geradoras, transmissoras, distribuidoras e comercializadoras.",
                    Site = "http://way2.com.br",
                    Address = "Rodovia SC 401, 4150, CIA Acate sala 17 - Florianópolis - SC",
                    LogoUrl = "http://i.imgur.com/Slsbsu1.png"
                },
                new Company
                {
                    Name = "Paradigma",
                    Description =
                        "Em 18 anos de mercado a Paradigma especializou-se no desenvolvimento de soluções para relacionamento e negociação eletrônica. Presente em mais de 20 setores da economia, acumula experiência e conhecimento com um ciclo contínuo de inovação tecnológica e de processos, acompanhando a evolução das melhores práticas de mercado. A empresa oferece soluções consolidadas, confiáveis e com alta disponibilidade, para os segmentos de energia, indústria, serviços, setor público e mercado aberto.",
                    Site = "http://www.paradigmabs.com.br",
                    Address = "Rodovia José Carlos Daux (SC 401), 8.600, Sala 102 - Bloco 04 - Florianópolis - SC",
                    LogoUrl = "http://i.imgur.com/iiqapkl.png"
                }
            };
        }

        public Event[] Events { get; }
        public Company[] Companies { get; }
        public Job[] Jobs { get; }

        public IQueryable<Event> GetEvents() => Events.AsQueryable();

        public Event GetEventBySlug(string slug) => Events.FirstOrDefault(e => e.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));

        public IQueryable<Company> GetCompanies() => Companies.AsQueryable();

        public IQueryable<Job> GetJobs() => Jobs.AsQueryable();        
    }
}