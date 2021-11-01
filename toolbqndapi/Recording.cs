using System;
using System.Collections;
using System.Collections.Generic;

namespace toolbqndapi
{
    public class Recording
    {
        public int ConcertId { get; set; }
        public DateTime ConcertDate { get; set; }
        public string VenueName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public SortedList setlist { get; set; }
        public SortedList mp3links { get; set; }
        public bool needsMap { get; set; }
        public List<Map> maps { get; set; }
    }
}
