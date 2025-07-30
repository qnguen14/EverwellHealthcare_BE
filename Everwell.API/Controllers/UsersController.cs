// ============================================================================
// USERS CONTROLLER
// ============================================================================
// This controller manages user accounts, profiles, and role-based operations
// It handles user CRUD operations, profile management, and administrative functions
// 
// USER MANAGEMENT FLOW:
// 1. REGISTRATION: New users created via AuthController
// 2. PROFILE SETUP: Users complete their profile information
// 3. ROLE ASSIGNMENT: Admin assigns roles (Customer, Consultant, Staff, Manager)
// 4. PROFILE UPDATES: Users can modify their own profiles
// 5. ADMIN OPERATIONS: Admins can manage all user accounts
// 
// ROLE HIERARCHY:
// - Admin: Full system access, can manage all users
// - Manager: Can view and manage staff operations
// - Staff: Can assist with customer service operations
// - Consultant: Healthcare providers, manage appointments
// - Customer: End users, book appointments and track health
// 
// SECURITY MODEL:
// - JWT-based authentication for all operations
// - Role-based authorization for different access levels
// - Users can only modify their own profiles (except Admin)
// - Sensitive operations require Admin privileges

using Everwell.API.Constants;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Metadata;
using Everwell.DAL.Data.Requests.User;
using Everwell.DAL.Data.Responses.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Everwell.API.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
        }

        /// <summary>
        /// GET ALL USERS - ADMIN ONLY
        /// ===========================
        /// Retrieves complete list of all users in the system
        /// 
        /// ADMIN OPERATIONS:
        /// - View all user accounts across all roles
        /// - Monitor user activity and status
        /// - Perform bulk user management operations
        /// - Generate user reports and analytics
        /// 
        /// RESPONSE DATA:
        /// - User basic information (name, email, role)
        /// - Account status (active/inactive)
        /// - Registration date and last activity
        /// - Role assignments and permissions
        /// 
        /// SECURITY:
        /// - Restricted to Admin role only
        /// - Contains sensitive user information
        /// - Used for administrative oversight
        /// </summary>
        [HttpGet(ApiEndpointConstants.User.GetAllUsersEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateUserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")] // Admin-only access for user management
        public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetUsers()
        {
            // Service returns all users with basic profile information
            // Includes role information and account status
            var response = await _userService.GetUsers();
            
            var apiResponse = new ApiResponse<IEnumerable<CreateUserResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Users retrieved successfully",
                IsSuccess = true,
                Data = response // Complete user list for administrative purposes
            };
            return Ok(apiResponse);
        }
        
        /// <summary>
        /// GET USERS BY ROLE
        /// =================
        /// Retrieves users filtered by specific role for role-based operations
        /// 
        /// USE CASES:
        /// - Customers: Find available consultants for booking
        /// - Staff: Get list of customers for support operations
        /// - Managers: View consultants and staff for management
        /// - Admins: Role-based user management and reporting
        /// 
        /// ROLE-BASED FILTERING:
        /// - Customer: Can view Consultants only (for appointment booking)
        /// - Consultant: Can view Customers (for appointment management)
        /// - Staff/Manager: Can view multiple roles based on permissions
        /// - Admin: Can view any role
        /// 
        /// BUSINESS LOGIC:
        /// - Service applies additional filtering based on caller's role
        /// - Prevents unauthorized access to user lists
        /// - Returns role-appropriate user information
        /// </summary>
        [HttpGet(ApiEndpointConstants.User.GetUsersByRoleEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreateUserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetUsersByRole(string role)
        {
            // Service applies role-based filtering and authorization
            // Ensures users only see appropriate user lists for their role
            var response = await _userService.GetUsersByRole(role);
            var apiResponse = new ApiResponse<IEnumerable<CreateUserResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Users retrieved successfully",
                IsSuccess = true,
                Data = response // Filtered user list based on requested role
            };
            return Ok(apiResponse);
        }

        [HttpPost(ApiEndpointConstants.User.CreateUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUserResponse>> CreateUser(CreateUserRequest request)
        {
            var response = await _userService.CreateUser(request);
            if (response == null)
            {
                return NotFound(new { message = "Tài khoản với email này đã tồn tại." });
            }
            var apiResponse = new ApiResponse<CreateUserResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User created successfully",
                IsSuccess = true,
                Data = response
            };
            return Ok(apiResponse);
        }

        [HttpGet(ApiEndpointConstants.User.GetUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Manager, Consultant, Staff, Customer")]
        public async Task<ActionResult<GetUserResponse>> GetUserById(Guid id)
        {
            try
            {
                var response = await _userService.GetUserById(id);
                
                var apiResponse = new ApiResponse<GetUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User retrieved successfully",
                    IsSuccess = true,
                    Data = response
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        
        //to do: fix update and delete to follow other controller patterns

        [HttpPut(ApiEndpointConstants.User.UpdateUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateUser(Guid id, UpdateUserRequest request)
        {
            try
            {
                var response = await _userService.UpdateUser(id, request);
                
                if (response == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User updated successfully",
                    IsSuccess = true,
                    Data = response
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete(ApiEndpointConstants.User.DeleteUserEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                if (result)
                {
                    var apiResponse = new ApiResponse<CreateUserResponse>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "User deleted successfully",
                        IsSuccess = true,
                        Data = null // No data to return on delete
                    };
                    
                    return Ok(apiResponse);
                }
                return BadRequest(new { message = "Failed to delete user" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.ToggleUserStatusEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateUserResponse>> ToggleUserStatus(Guid id)
        {
            try
            {
                var response = await _userService.ToggleUserStatus(id);
                
                if (response == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User status toggled successfully",
                    IsSuccess = true,
                    Data = response
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.SetRoleEndpoint)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateUserResponse>> SetUserRole(Guid id, SetUserRoleRequest request)
        {
            try
            {
                var user = await _userService.SetUserRole(id, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User role updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.UpdateProfileEndpoint)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateProfile(Guid id, UpdateProfileRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                // Users can only update their own profile, unless they're admin
                if (currentUserRole != "Admin" && (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId) || currentUserId != id))
                {
                    return Forbid("You can only update your own profile.");
                }

                var user = await _userService.UpdateProfile(id, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User profile updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut(ApiEndpointConstants.User.UpdateAvatarEndpoint)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateAvatar(Guid id, UpdateAvatarRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                // Users can only update their own avatar, unless they're admin
                if (currentUserRole != "Admin" && (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId) || currentUserId != id))
                {
                    return Forbid("You can only update your own avatar.");
                }

                var user = await _userService.UpdateAvatar(id, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User avatar updated successfully",
                    IsSuccess = true,
                    Data = user
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // New Profile API Endpoints

        /// <summary>
        /// GET MY PROFILE
        /// ==============
        /// Retrieves current user's complete profile information
        /// 
        /// PROFILE INFORMATION:
        /// - Personal details (name, email, phone, address)
        /// - Role and permissions
        /// - Account status and settings
        /// - Role-specific data (consultant specialization, customer preferences)
        /// 
        /// JWT TOKEN FLOW:
        /// 1. Extract user ID from JWT token claims
        /// 2. Validate token authenticity and expiration
        /// 3. Retrieve user profile from database
        /// 4. Return complete profile information
        /// 
        /// SECURITY:
        /// - Users can only access their own profile
        /// - JWT token validation ensures authenticity
        /// - No sensitive information like passwords returned
        /// 
        /// USE CASES:
        /// - Profile page display
        /// - User settings management
        /// - Role-based feature access
        /// </summary>
        [HttpGet(ApiEndpointConstants.User.GetMyProfileEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Consultant, Staff, Manager, Customer")] // All authenticated users
        public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
        {
            try
            {
                // Extract user ID from JWT token claims (NameIdentifier claim)
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                // Validate token contains valid user ID
                if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                // Service retrieves complete user profile with role-specific information
                var profile = await _userService.GetCurrentUserProfile(currentUserId);
                
                var apiResponse = new ApiResponse<UserProfileResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User profile retrieved successfully",
                    IsSuccess = true,
                    Data = profile // Complete user profile with role-specific data
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// UPDATE MY PROFILE
        /// =================
        /// Allows users to update their own profile information
        /// 
        /// PROFILE UPDATE FLOW:
        /// 1. Validate JWT token and extract user ID
        /// 2. Validate request data (required fields, format)
        /// 3. Update user profile in database
        /// 4. Return updated profile information
        /// 
        /// UPDATABLE FIELDS:
        /// - Personal information (name, phone, address)
        /// - Professional details (for consultants)
        /// - Preferences and settings
        /// - Contact information
        /// 
        /// BUSINESS RULES:
        /// - Users can only update their own profile
        /// - Email changes may require verification
        /// - Role-specific fields validated based on user role
        /// - Certain fields may require admin approval
        /// 
        /// SECURITY:
        /// - JWT token validation ensures user identity
        /// - Input validation prevents malicious data
        /// - Audit trail for profile changes
        /// </summary>
        [HttpPut(ApiEndpointConstants.User.UpdateMyProfileEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize] // All authenticated users can update their own profile
        public async Task<ActionResult<UpdateUserResponse>> UpdateMyProfile(UpdateProfileRequest request)
        {
            try
            {
                // Extract user ID from JWT token for profile ownership validation
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                // Service validates request data and updates user profile
                // Applies business rules and role-specific validations
                var user = await _userService.UpdateProfile(currentUserId, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User profile updated successfully",
                    IsSuccess = true,
                    Data = user // Updated profile information
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// UPDATE MY AVATAR
        /// ================
        /// Allows users to update their profile avatar/picture
        /// 
        /// AVATAR UPDATE FLOW:
        /// 1. Validate JWT token and user identity
        /// 2. Validate image format and size
        /// 3. Process and store avatar image
        /// 4. Update user profile with new avatar URL
        /// 5. Return updated profile information
        /// 
        /// IMAGE PROCESSING:
        /// - Validate file format (JPG, PNG, etc.)
        /// - Check file size limits
        /// - Resize/optimize image if needed
        /// - Store in secure file storage
        /// - Generate accessible URL
        /// 
        /// BUSINESS RULES:
        /// - Users can only update their own avatar
        /// - Image must meet size and format requirements
        /// - Previous avatar may be deleted to save storage
        /// - Avatar changes are immediately visible
        /// 
        /// SECURITY:
        /// - File type validation prevents malicious uploads
        /// - Size limits prevent storage abuse
        /// - Secure file storage with access controls
        /// </summary>
        [HttpPut(ApiEndpointConstants.User.UpdateMyAvatarEndpoint)]
        [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize] // All authenticated users can update their avatar
        public async Task<ActionResult<UpdateUserResponse>> UpdateMyAvatar(UpdateAvatarRequest request)
        {
            try
            {
                // Extract user ID from JWT token for avatar ownership validation
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                // Service handles image validation, processing, and storage
                // Updates user profile with new avatar URL
                var user = await _userService.UpdateAvatar(currentUserId, request);
                
                var apiResponse = new ApiResponse<UpdateUserResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User avatar updated successfully",
                    IsSuccess = true,
                    Data = user // Updated profile with new avatar URL
                };
                return Ok(apiResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
