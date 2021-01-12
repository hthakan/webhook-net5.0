using System;
using System.Linq;
using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AirlineWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookSubscriptionController : ControllerBase
    {
        private readonly AirlineDbContext _context;
        private readonly IMapper _mapper;

        public WebhookSubscriptionController(AirlineDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("{secret}",Name = "GetSubscriptionBySecret")]
        public ActionResult<WebhookSubscriptionReadDto> GetSubscriptionBySecret(string secret)
        {
            var subscription = _context.WebhookSubscriptions.FirstOrDefault(s=> s.Secret == secret);

            if(subscription == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<WebhookSubscriptionReadDto>(subscription));
        }

        [HttpPost]
        public ActionResult<WebhookSubscriptionReadDto> CreateSubscription(WebhookSubscriptionCreateDto webhookSubscriptionCraeteDto)
        {
           var subscription = _context.WebhookSubscriptions.FirstOrDefault(s=> s.WebhookURI == webhookSubscriptionCraeteDto.WebhookURI); 

           if(subscription == null)
           {
               subscription = _mapper.Map<WebhookSubscription>(webhookSubscriptionCraeteDto);

               subscription.Secret = Guid.NewGuid().ToString();
               subscription.WebhookPublisher = "THY";

               try
               {
                   _context.WebhookSubscriptions.Add(subscription);
                   _context.SaveChanges();
               }
               catch(Exception ex)
               {
                   return BadRequest(ex.Message);
               }


               var webhookSubscriptionReadDto = _mapper.Map<WebhookSubscriptionReadDto>(subscription);

               return CreatedAtRoute(nameof(GetSubscriptionBySecret),new {secret = webhookSubscriptionReadDto.Secret },webhookSubscriptionReadDto);
           }
           else
           {
               return NoContent();
           }
        }
    }
}