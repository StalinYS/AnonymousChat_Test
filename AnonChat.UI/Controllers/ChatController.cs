﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnonChat.BLL.Interfaces;
using AnonChat.Models;
using AnonChat.UI.Hubs;
using AnonChat.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AnonChat.UI.Controllers
{
    public class SearchLine
    {
        public ApplicationUser user { get; set; }
        public string gender { get; set; }
        public int age_max { get; set; }
        public int age_min { get; set; }
        public DateTime searchStart { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private static Dictionary<string, SearchLine> _searchList = new Dictionary<string, SearchLine>();


        public readonly IAccountService accountService;
        public readonly IChatService chatService;

        public ChatController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        static List<string> UserIds = new List<string>();

        [HttpPost("UserSearch")]
        public async Task<Object> UserSearch([FromBody] SearchViewModel searchViewModel)
        {
            var resultId = "";
            ApplicationUser user = await accountService.FindUserById(User.Claims.First(c => c.Type == "UserID").Value);
            string userId = user.Id;
            SearchLine searchline = new SearchLine()
            {
                user = user,
                gender = searchViewModel.Gender,
                age_max = searchViewModel.AgeMax,
                age_min = searchViewModel.AgeMin,
                searchStart = DateTime.Now
            };
            _searchList.Remove(userId);
            _searchList.Add(userId, searchline);
            await Task.Run(() =>
            {
                var isEnd = false;
                var child = Task.Run(() =>
                {
                    while (!isEnd)
                    {
                        List<SearchLine> list = new List<SearchLine>();
                        var searchingRN = _searchList.Select(d=>d.Value).ToList().FindAll(u => EF.Functions.DateDiffYear(searchline.user.BirthDay, DateTime.Today) >= u.age_min &&
                                                                  EF.Functions.DateDiffYear(searchline.user.BirthDay, DateTime.Today) <= u.age_max &&
                                                                  u.gender == searchline.user.Gender && u.user.Id != searchline.user.Id);
                        var full_match = new List<SearchLine>();
                        if (searchingRN.Any())
                        {
                            full_match = searchingRN.FindAll(u => EF.Functions.DateDiffYear(user.BirthDay, DateTime.Today) >= u.age_min &&
                                                                      EF.Functions.DateDiffYear(user.BirthDay, DateTime.Today) <= u.age_max &&
                                                                      u.gender == (user.Gender) && u.user.Id != searchline.user.Id);
                        }


                        if (full_match.Count > 1)
                        {
                            resultId = full_match.OrderBy(sl => sl.searchStart).FirstOrDefault().user.Id;
                            break;
                        }
                        else if (full_match.Count == 1)
                        {
                            resultId = full_match.First().user.Id;
                            break;
                        }
                    }
                });
                for (int i = 0; i < 6; i++)
                {
                    Thread.Sleep(5000);

                    if (child.IsCompleted)
                    { 
                        break;
                    }
                }
                isEnd = true;
            });
            return resultId;
        }

        [HttpGet("{userId}")]
        public async Task<Object> GetUserProfile(string userId)
        {
            var user = await accountService.FindUserById(userId);
            return new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.BirthDay,
                user.Gender
            };
        }
    }
}