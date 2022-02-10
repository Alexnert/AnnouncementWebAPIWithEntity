using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnnouncementWebAPIWithEntity.Models
{
    public class Announcement
    {

        private DateTime _AddedDate = DateTime.Now;
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Titile { get; set; }
        public string Description { get; set; }
        public DateTime AddedDate
        {
            get { return _AddedDate; }
            set { _AddedDate = value; }
        }

    }
}
