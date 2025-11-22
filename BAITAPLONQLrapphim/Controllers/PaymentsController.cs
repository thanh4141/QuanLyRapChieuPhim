using CinemaBooking.BLL.Services;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request == null || request.InvoiceId <= 0)
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Thông tin thanh toán không hợp lệ"));
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentService.CreatePaymentAsync(request, userId);
            if (result == null)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Hóa đơn không hợp lệ hoặc thanh toán thất bại"));
            }
            return Ok(ApiResponse<PaymentDto>.SuccessResult(result, "Thanh toán thành công! Vé đã được xác nhận."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResult($"Lỗi khi thanh toán: {ex.Message}"));
        }
    }

    [HttpGet("invoice/{invoiceId}")]
    public async Task<ActionResult<ApiResponse<List<PaymentDto>>>> GetPaymentsByInvoice(int invoiceId)
    {
        var payments = await _paymentService.GetPaymentsByInvoiceAsync(invoiceId);
        return Ok(ApiResponse<List<PaymentDto>>.SuccessResult(payments));
    }

    [HttpPost("staff/for-customer")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePaymentForCustomer(
        [FromBody] CreatePaymentRequest request,
        [FromQuery] int customerUserId)
    {
        if (request == null || request.InvoiceId <= 0 || customerUserId <= 0)
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Thông tin thanh toán không hợp lệ"));
        }

        try
        {
            var staffUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentService.CreatePaymentForCustomerAsync(request, staffUserId, customerUserId);
            if (result == null)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Hóa đơn không hợp lệ hoặc thanh toán thất bại"));
            }
            return Ok(ApiResponse<PaymentDto>.SuccessResult(result, "Thanh toán thành công! Vé đã được xác nhận."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResult($"Lỗi khi thanh toán: {ex.Message}"));
        }
    }
}

