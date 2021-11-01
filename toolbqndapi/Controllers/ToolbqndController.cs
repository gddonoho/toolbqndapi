using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace toolbqndapi.Controllers
{
    [ApiController]
    [Route("/")]
    public class ToolbqndController : ControllerBase
    {
        public ToolbqndController()
        {
        }
        [HttpGet]
        public Recording Get()
        {
            var connectionString = System.Environment.GetEnvironmentVariable("DB_CONN_STR");
            Console.WriteLine($"connection string: {connectionString}");

            Recording speck = new Recording();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = $"select * from get_random_recording()";
                using (NpgsqlCommand comm = new NpgsqlCommand(command, connection))
                {
                    var dr = comm.ExecuteReader();
                    while (dr.Read())
                    {
                        speck.ConcertId = Int32.Parse(dr[0].ToString());
                        speck.ConcertDate = DateTime.Parse(dr[1].ToString());
                        speck.VenueName = dr[2].ToString();
                        speck.City = dr[3].ToString();
                        speck.Country = dr[4].ToString();
                        speck.Latitude = Decimal.Parse(dr[5].ToString());
                        speck.Longitude = Decimal.Parse(dr[6].ToString());
                        break;
                    }
                }
            }
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = $"select songorder, songtitle from setlist where concertid = {speck.ConcertId} order by 1; ";
                using (NpgsqlCommand comm = new NpgsqlCommand(command, connection))
                {
                    var dr = comm.ExecuteReader();
                    var setlist = new SortedList();
                    while (dr.Read())
                    {
                        setlist.Add(dr[0], dr[1].ToString().Replace("&", "and"));
                    }
                    speck.setlist = setlist;
                }
            }
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = $"select l.songorder, l.linkurl from mp3link l, recording r where l.recordingid = r.recordingid and r.concertid = {speck.ConcertId} order by 1;";
                using (NpgsqlCommand comm = new NpgsqlCommand(command, connection))
                {
                    var dr = comm.ExecuteReader();
                    var mp3list = new SortedList();
                    while (dr.Read())
                    {
                        mp3list.Add(dr[0], dr[1].ToString().Replace(" ", "%20"));
                    }
                    speck.mp3links = mp3list;
                }
            }
            speck.needsMap = (speck.mp3links.Count != 1) && (speck.mp3links.Count != speck.setlist.Count) ? true : false;
            speck.maps = new List<Map>();
            if (speck.needsMap)
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = $"select m.setlistsongorder, m.mp3linksongorder from mapsetlist2mp3 m where m.concertid = {speck.ConcertId} order by 1;";
                    using (NpgsqlCommand comm = new NpgsqlCommand(command, connection))
                    {
                        var dr = comm.ExecuteReader();
                        while (dr.Read())
                        {
                            speck.maps.Add(new Map() { SetlistItem = Int32.Parse(dr[0].ToString()), Mp3linkItem = Int32.Parse(dr[1].ToString()) });
                        }
                    }
                }
            }
            return speck;
        }
    }
}
