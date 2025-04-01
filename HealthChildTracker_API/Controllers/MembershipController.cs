using BusinessLogic.DTOs.Membership;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền quản lý membership
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(
            IMembershipService membershipService,
            ILogger<MembershipController> logger)
        {
            _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MembershipDTO>>> GetAllMemberships()
        {
            try
            {
                _logger.LogInformation("Bắt đầu lấy danh sách gói membership");
                var memberships = await _membershipService.GetAllMembershipsAsync();

                return Ok(new
                {
                    success = true,
                    data = memberships
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách gói membership");
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách gói membership"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipDTO>> GetMembershipById(int id)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu lấy thông tin gói membership {id}");
                var membership = await _membershipService.GetMembershipByIdAsync(id);

                return Ok(new
                {
                    success = true,
                    data = membership
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Không tìm thấy gói membership với ID {id}");
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin gói membership {id}");
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thông tin gói membership"
                });
            }
        }

        [HttpPut("{id}/price")]
        public async Task<ActionResult<MembershipDTO>> UpdateMembershipPrice(int id, [FromBody] decimal newPrice)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu cập nhật giá gói membership {id} thành {newPrice}");
                var membership = await _membershipService.UpdateMembershipPriceAsync(id, newPrice);

                return Ok(new
                {
                    success = true,
                    data = membership,
                    message = "Cập nhật giá thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Không tìm thấy gói membership với ID {id}");
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Giá mới không hợp lệ: {newPrice}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật giá gói membership {id}");
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật giá"
                });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<MembershipDTO>> UpdateMembershipStatus(int id, [FromBody] bool status)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu cập nhật trạng thái gói membership {id} thành {status}");
                var membership = await _membershipService.UpdateMembershipStatusAsync(id, status);

                return Ok(new
                {
                    success = true,
                    data = membership,
                    message = "Cập nhật trạng thái thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Không tìm thấy gói membership với ID {id}");
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật trạng thái gói membership {id}");
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật trạng thái"
                });
            }
        }

      /*  [HttpPut("{id}")]
        public async Task<ActionResult<MembershipDTO>> UpdateMembership(int id, [FromBody] UpdateMembershipDTO updateDto)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu cập nhật thông tin gói membership {id}");
                var membership = await _membershipService.UpdateMembershipAsync(id, updateDto);

                return Ok(new
                {
                    success = true,
                    data = membership,
                    message = "Cập nhật thông tin thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Không tìm thấy gói membership với ID {id}");
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật thông tin gói membership {id}");
                return BadRequest(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật thông tin"
                });
            }
        }*/
    }
}
