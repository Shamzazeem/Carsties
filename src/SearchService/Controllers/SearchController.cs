using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var collection = DB.Collection<Item>();
        var indexes = await collection.Indexes.ListAsync();
        var indexList = await indexes.ToListAsync();

        foreach (var index in indexList)
        {
            Console.WriteLine("[INDEX] " + index.ToJson());
        }
        var allItems = await DB.Find<Item>().ExecuteAsync();
        Console.WriteLine($"[DEBUG] Total items in DB: {allItems.Count}");
        
        foreach (var item in allItems)
        {
            Console.WriteLine($"Item: {item.Make} + {item.Model} - Ends: {item.AuctionEnd} - Seller: {item.Seller} - Winner: {item.Winner}");
        }

        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }
        
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make))
                .Sort(x => x.Ascending(a => a.Model)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };
        

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }
        
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
    [HttpGet("test-text-search")]
    public async Task<IActionResult> TestTextSearch([FromQuery] string term)
    {
        var results = await DB.Find<Item>()
                            .Match(Search.Full, term)
                            .SortByTextScore()
                            .ExecuteAsync();

        return Ok(results);
    }

}