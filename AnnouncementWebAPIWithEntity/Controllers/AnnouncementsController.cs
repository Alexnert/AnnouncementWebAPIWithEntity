using AnnouncementWebAPIWithEntity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AnnouncementWebAPIWithEntity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        AnnouncementsContext db;
        public AnnouncementsController(AnnouncementsContext context)
        {
            db = context;
            if (!db.Announcements.Any())
            {
                db.Announcements.Add(new Announcement {Id=1,Titile="Tom",Description="tom"  });
                db.Announcements.Add(new Announcement { Id = 2, Titile = "Alex", Description = "alex" });
                db.Announcements.Add(new Announcement { Id = 3, Titile = "tom alex", Description = "Alex.tom" });
                db.Announcements.Add(new Announcement { Id = 4, Titile = "Tom tom", Description = "Tom.toM" });
                db.Announcements.Add(new Announcement { Id = 5, Titile = "AleX.Tom", Description = "tom" });
                db.SaveChanges();
            }

        }
        // GET: api/announcements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Announcement>>> Get()
        {
            return await db.Announcements.ToListAsync();
        }

        // GET: api/announcements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Announcement>> Get(int id)
        {
            Announcement announcement = await db.Announcements.FirstOrDefaultAsync(x => x.Id == id);
            if (announcement == null)
                return NotFound();
            return new ObjectResult(announcement);
        }
        // POST: api/announcements
        [HttpPost]
        public async Task<ActionResult<Announcement>> Post([FromBody]Announcement announcement)
        {
            if (announcement == null)
            {
                return BadRequest();
            }
            if(db.Announcements.Any(a=>a.Id==announcement.Id))
            {
                return this.StatusCode((int)HttpStatusCode.Conflict);
            }
      
                db.Announcements.Add(new Announcement { Titile = announcement.Titile, Description = announcement.Description });
                await db.SaveChangesAsync();
            
            return CreatedAtAction(nameof(Get), new { id = announcement.Id }, announcement);
        }

        // PUT: api/announcements/5
        [HttpPut]
        public async Task<ActionResult<Announcement>> Put(int Id, [FromBody] Announcement announcement)
        {
            if (announcement == null||Id!=announcement.Id)
            {
                return BadRequest();
            }
            if (!db.Announcements.Any(x => x.Id == Id))
            {
                return NotFound();
            }
            
            db.Announcements.Update(announcement);
            //db.Entry(announcement).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Ok(announcement);
        }

        // DELETE: api/announcements/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Announcement>> Delete(int id)
        {
            Announcement announcement = db.Announcements.FirstOrDefault(x => x.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }
            db.Announcements.Remove(announcement);
            await db.SaveChangesAsync();
            return Ok(announcement);
        }

        //api/announcements/similar
        [Route("similar")]
        public async Task<ActionResult<(Announcement,Announcement)[]>> Similar()
        {
            //if there are few elements in the database
            if ( db.Announcements.ToList().Count<4)
            {
                return BadRequest();
            }

            //divide the title and description into words
            char[] pattern = new char[] { ' ', '.' };
            var announcementsPlus = from a in db.Announcements
                                    select new
                                    {
                                        Id = a.Id,
                                        Titile = a.Titile.ToLower().Split(pattern).ToList<string>().Distinct().ToList(),
                                        Description = a.Description.ToLower().Split(pattern).ToList<string>().Distinct().ToList(),
                                    };
            //determine the size of the arrays
            List<int> idList = new List<int>();
            foreach (var aplus in announcementsPlus)
            {
                idList.Add(aplus.Id);
            }

            int tableSize = idList.Max() + 1;
            int[,] titileTable = new int[tableSize, tableSize];
            int[,] descriptionTable = new int[tableSize, tableSize];
            int[,] similarTable = new int[tableSize, tableSize];


            //algorithm for compiling a similarity matrix for announcements
            foreach (var aplus in announcementsPlus)
            {
                foreach (var ap in announcementsPlus)
                {
                    if (ap.Id > aplus.Id)
                    {
                        List<string> intersectionList = new List<string>();

                        foreach (var titile in ap.Titile)
                        {
                            intersectionList.AddRange(aplus.Titile.ToArray().Intersect(new string[] { titile }).ToList());
                        }
                        titileTable[aplus.Id, ap.Id] = intersectionList.Count;

                        List<string> descriptionList = new List<string>();

                        foreach (var description in ap.Description)
                        {
                            descriptionList.AddRange(aplus.Titile.ToArray().Intersect(new string[] { description }).ToList());
                        }
                        descriptionTable[aplus.Id, ap.Id] = descriptionList.Count;

                        similarTable[aplus.Id, ap.Id] = (titileTable[aplus.Id, ap.Id] == 0 || descriptionTable[aplus.Id, ap.Id] == 0) ? 0 : titileTable[aplus.Id, ap.Id] + descriptionTable[aplus.Id, ap.Id];
                    }
                }
            }

            List<int> similarList = new List<int>();
            for (int i = 0; i < tableSize; i++)
            {
                int[] temp = new int[similarTable.GetLength(1)];
                for (int j = 0; j < tableSize; j++)
                {
                    temp[j] = similarTable[i, j];
                }
                similarList.AddRange(temp);
            }
            //if a small number of similar
            if (similarList.Count(x => x != 0) < 3)
            {
                return BadRequest();
            }

            // create the result of the method
            Tuple<Announcement, Announcement>[] tupleArrayId = new Tuple<Announcement, Announcement>[3];

            for (int i = 0; i < 3; i++)
            {
                int id1, id2;
                id1 = similarList.IndexOf(similarList.Max()) / tableSize;
                id2 = similarList.IndexOf(similarList.Max()) % tableSize;
                tupleArrayId[i] = new Tuple<Announcement, Announcement>(db.Announcements.FirstOrDefault(x => x.Id == id1), db.Announcements.FirstOrDefault(x => x.Id == id2));
                
                similarList[similarList.IndexOf(similarList.Max())] = 0;
            }


            return new ObjectResult(tupleArrayId);
        }

    }
}
