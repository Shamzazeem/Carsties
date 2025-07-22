using System;
using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionsFinished>
{
   private readonly AuctionDbContext _dbContext;
   public AuctionFinishedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<AuctionsFinished> context)
    {
        Console.WriteLine("---> Consuming Auction Finished consumer");
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
                ? Entities.Status.Finished : Entities.Status.ReserveNotMet;

        await _dbContext.SaveChangesAsync(); 
    }
}
