using BusinessLogic.DTOs.Transaction;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionService transactionService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetAllTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync();
                return Ok(new
                {
                    success = true,
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch");
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách giao dịch"
                });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactionsByUserId(int userId)
        {
            try
            {
                // Kiểm tra quyền truy cập
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch của user {UserId}", userId);
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách giao dịch"
                });
            }
        }
    }
}
