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
            speck.setlist = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = $"select songorder, songtitle from setlist where concertid = {speck.ConcertId} order by 1; ";
                using (NpgsqlCommand comm = new NpgsqlCommand(command, connection))
                {
                    var dr = comm.ExecuteReader();
                    while (dr.Read())
                    {
                        speck.setlist.Add(dr[1].ToString().Replace("&", "and"));
                    }
                }
            }
            speck.mp3links = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = $"select l.songorder, l.linkurl from mp3link l, recording r where l.recordingid = r.recordingid and r.concertid = {speck.ConcertId} order by 1;";
                using (NpgsqlCommand comm = new NpgsqlCommand(command, connection))
                {
                    var dr = comm.ExecuteReader();
                    while (dr.Read())
                    {
                        speck.mp3links.Add(dr[1].ToString().Replace(" ", "%20"));
                    }
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
                            speck.maps.Add(new Map() { SetlistIndex = Int32.Parse(dr[0].ToString()) - 1, Mp3Index = Int32.Parse(dr[1].ToString()) - 1 });
                        }
                    }
                }
            }
            return speck;
        }
    }
}
