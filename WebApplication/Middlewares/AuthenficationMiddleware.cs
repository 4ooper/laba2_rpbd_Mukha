﻿using BuildingContractor.Application.Interfaces;
using BuildingContractor.Persistence;
using System.Text;
using System.Text.Json;
using MediatR;
using BuildingContractor.Application.Users.Quieres.GetUsersList;
using Microsoft.EntityFrameworkCore;
using BuildingContractor.Domain.JWTModels;

namespace WebApplication.Middlewares
{
    public class AuthenficationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenficationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IHost host, AppDbContext appDbContext)
        {

            //using (var scope = host.Services.CreateScope())
            //{
            //    var serviceProvider = scope.ServiceProvider;

            //    try
            //    {
            //        INotesDbContext dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            //        string requestToken;
            //        context.Request.Cookies.TryGetValue("token", out requestToken);
            //        if (requestToken != null)
            //        {
            //            var TokenParts = requestToken.Split('.');
            //            var header = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Convert.FromBase64String(TokenParts[0])));
            //            var payload = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Convert.FromBase64String(TokenParts[1])));
            //            var signature = Encoding.UTF8.GetString(Convert.FromBase64String(TokenParts[2]));

            //            if (signature != "I love C Sharp")
            //            {
            //                context.Response.Cookies.Delete("token");
            //                context.Response.Redirect("/authenfication/SignIn");
            //            }
            //            var id = payload.Value<int>("userID");
            //            var user = dbContext.Users.FirstOrDefault(user => user.id == payload.Value<int>("userID"));
            //            context.Items["User"] = dbContext.Users.FirstOrDefault(user => user.id == payload.Value<int>("id"));
            //        }
            //    }
            //    catch
            //    {

            //    }
            //}

            var token = context.Request.Cookies["token"];

            if(token != null)
            {
                string[] tokenParts = token.Split(".");
                var header = JsonSerializer.Deserialize<Header>(Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[0])));
                var payload = JsonSerializer.Deserialize<Payload>(Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1])));
                var signature = JsonSerializer.Deserialize<Signature>(Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[2])));
                if(signature.key != "I love C Sharp" || ConvertInt32ToDateTime(payload.exp) < DateTime.Now || signature == null || payload == null)
                {
                    context.Items["User"] = null;
                }
                else
                {
                    context.Items["User"] = appDbContext.Users.Include(user => user.Role).FirstOrDefault(user => user.id == payload.id);
                }
            }
            await _next.Invoke(context);
        }

        private DateTime ConvertInt32ToDateTime(int i)
        {
            return new DateTime(2017, 1, 1).AddSeconds(i);
        }
    }
}
