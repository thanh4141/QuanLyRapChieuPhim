using AutoMapper;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoicesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<InvoiceDto>>>> GetMyInvoices()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var invoices = await _unitOfWork.Invoices.FindAsync(i => i.UserId == userId && !i.IsDeleted);
        
        var invoiceDtos = new List<InvoiceDto>();
        foreach (var invoice in invoices)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.InvoiceId == invoice.InvoiceId && !p.IsDeleted);
            var dto = _mapper.Map<InvoiceDto>(invoice);
            dto.Payments = _mapper.Map<List<PaymentDto>>(payments);
            invoiceDtos.Add(dto);
        }

        return Ok(ApiResponse<List<InvoiceDto>>.SuccessResult(invoiceDtos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoice(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
        
        if (invoice == null || invoice.IsDeleted || invoice.UserId != userId)
        {
            return NotFound(ApiResponse<InvoiceDto>.ErrorResult("Invoice not found"));
        }

        var payments = await _unitOfWork.Payments.FindAsync(p => p.InvoiceId == id && !p.IsDeleted);
        var dto = _mapper.Map<InvoiceDto>(invoice);
        dto.Payments = _mapper.Map<List<PaymentDto>>(payments);

        return Ok(ApiResponse<InvoiceDto>.SuccessResult(dto));
    }
}

