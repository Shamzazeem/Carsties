using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("-----------> consuming Auction created: " + context.Message.Id);

        //var item = _mapper.Map<Item>(context.Message);
        var item = new Item
        {
            ID = context.Message.Id.ToString(),
            Make = context.Message.Make,
            Model = context.Message.Model,
            Year = context.Message.Year,
            Color = context.Message.Color,
            Mileage = context.Message.Mileage,
            ImageUrl = context.Message.ImageUrl,
            CreatedAt = context.Message.CreatedAt,
            UpdatedAt = context.Message.UpdatedAt
        };

        await item.SaveAsync();
    }
}
