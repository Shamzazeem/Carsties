using System;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAutions(string date)
    {

        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
        .Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);
    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreatAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "Test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could NOT SAVE CHANGE TO DB");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid Id, UpdateAuctionDto UpdateAuctionDto)
    {

        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == Id);
        if (auction == null) return NotFound();

        //check seller username \
        auction.Item.Make = UpdateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = UpdateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = UpdateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = UpdateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = UpdateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;
        if (result) return Ok();

        return BadRequest("Problem Saving");

    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid Id)
    {
        var auction = await _context.Auctions.FindAsync(Id);
        if (auction == null) return NotFound();

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;
        if (result) return Ok();

        return BadRequest("Could Not update Db");
    }

}
