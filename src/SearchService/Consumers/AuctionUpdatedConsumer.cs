using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer: IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("-----------> Consuming Auction updated: " + context.Message.Id);

        var item = _mapper.Map<Item>(context.Message);

        var result = await DB.Update<Item>()
            .Match(i => i.ID == context.Message.Id)
            .ModifyOnly(i => new
            {
                i.Color,
                i.Make,
                i.Year,
                i.Mileage,
                i.Model
            }, item)
            .ExecuteAsync();
            
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), "Problem Updating the Mongo");
    }
}