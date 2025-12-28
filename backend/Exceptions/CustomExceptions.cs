using System;

namespace EcommerceAPI.Exceptions
{
    /// <summary>
    /// Base exception for all custom application exceptions
    /// </summary>
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message) { }
        public ApplicationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Thrown when a requested resource is not found
    /// </summary>
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with ID '{key}' was not found.") { }

        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when user is not found
    /// </summary>
    public class UserNotFoundException : NotFoundException
    {
        public UserNotFoundException(int userId)
            : base("User", userId) { }

        public UserNotFoundException(string email)
            : base($"User with email '{email}' was not found.") { }

        public UserNotFoundException()
            : base("User not found") { }
    }

    /// <summary>
    /// Thrown when product is not found
    /// </summary>
    public class ProductNotFoundException : NotFoundException
    {
        public ProductNotFoundException(int productId)
            : base("Product", productId) { }
    }

    /// <summary>
    /// Thrown when category is not found
    /// </summary>
    public class CategoryNotFoundException : NotFoundException
    {
        public CategoryNotFoundException(int categoryId)
            : base("Category", categoryId) { }
    }

    /// <summary>
    /// Thrown when order is not found
    /// </summary>
    public class OrderNotFoundException : NotFoundException
    {
        public OrderNotFoundException(int orderId)
            : base("Order", orderId) { }
    }

    /// <summary>
    /// Thrown when cart item is not found
    /// </summary>
    public class CartItemNotFoundException : NotFoundException
    {
        public CartItemNotFoundException(int cartItemId)
            : base("Cart item", cartItemId) { }
    }

    /// <summary>
    /// Thrown when discount code is not found
    /// </summary>
    public class DiscountCodeNotFoundException : NotFoundException
    {
        public DiscountCodeNotFoundException(string code)
            : base($"Discount code '{code}' was not found.") { }
    }

    /// <summary>
    /// Thrown when a business rule is violated
    /// </summary>
    public class BusinessRuleException : ApplicationException
    {
        public BusinessRuleException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when validation fails
    /// </summary>
    public class ValidationException : ApplicationException
    {
        public Dictionary<string, List<string>> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, List<string>>();
        }

        public ValidationException(Dictionary<string, List<string>> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Thrown when authentication fails
    /// </summary>
    public class AuthenticationException : ApplicationException
    {
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException() : base("Authentication failed.") { }
    }

    /// <summary>
    /// Thrown when authorization fails
    /// </summary>
    public class AuthorizationException : ApplicationException
    {
        public AuthorizationException(string message) : base(message) { }
        public AuthorizationException() : base("You are not authorized to perform this action.") { }
    }

    /// <summary>
    /// Thrown when payment processing fails
    /// </summary>
    public class PaymentException : ApplicationException
    {
        public PaymentException(string message) : base(message) { }
        public PaymentException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Thrown when file upload fails
    /// </summary>
    public class FileUploadException : ApplicationException
    {
        public FileUploadException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when operation conflicts with current state
    /// </summary>
    public class ConflictException : ApplicationException
    {
        public ConflictException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when an item already exists
    /// </summary>
    public class DuplicateException : ConflictException
    {
        public DuplicateException(string message) : base(message) { }
        public DuplicateException(string resourceName, string identifier)
            : base($"{resourceName} with '{identifier}' already exists.") { }
    }

    /// <summary>
    /// Thrown when stock is insufficient
    /// </summary>
    public class InsufficientStockException : BusinessRuleException
    {
        public InsufficientStockException(string productName, int requested, int available)
            : base($"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}") { }
    }

    /// <summary>
    /// Thrown when discount code is invalid or expired
    /// </summary>
    public class InvalidDiscountCodeException : BusinessRuleException
    {
        public InvalidDiscountCodeException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when cart is empty
    /// </summary>
    public class EmptyCartException : BusinessRuleException
    {
        public EmptyCartException() : base("Cart is empty. Cannot proceed with checkout.") { }
    }

    /// <summary>
    /// Thrown when email is already in use
    /// </summary>
    public class EmailAlreadyExistsException : DuplicateException
    {
        public EmailAlreadyExistsException(string email)
            : base($"An account with email '{email}' already exists.") { }
    }

    /// <summary>
    /// Thrown when refresh token is invalid or expired
    /// </summary>
    public class InvalidRefreshTokenException : AuthenticationException
    {
        public InvalidRefreshTokenException() : base("Invalid or expired refresh token.") { }
        public InvalidRefreshTokenException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when cart is not found
    /// </summary>
    public class CartNotFoundException : NotFoundException
    {
        public CartNotFoundException() : base("Cart not found") { }
        public CartNotFoundException(int cartId) : base("Cart", cartId) { }
    }

    /// <summary>
    /// Thrown when a question is not found
    /// </summary>
    public class QuestionNotFoundException : NotFoundException
    {
        public QuestionNotFoundException(int questionId) : base("Question", questionId) { }
        public QuestionNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when an answer is not found
    /// </summary>
    public class AnswerNotFoundException : NotFoundException
    {
        public AnswerNotFoundException(int answerId) : base("Answer", answerId) { }
        public AnswerNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a reward is not found
    /// </summary>
    public class RewardNotFoundException : NotFoundException
    {
        public RewardNotFoundException(int rewardId) : base("Reward", rewardId) { }
    }

    /// <summary>
    /// Thrown when an invalid role is specified
    /// </summary>
    public class InvalidRoleException : ValidationException
    {
        public InvalidRoleException(string role) : base($"Invalid role: {role}") { }
    }

    /// <summary>
    /// Thrown when password verification fails
    /// </summary>
    public class PasswordMismatchException : AuthenticationException
    {
        public PasswordMismatchException() : base("Current password is incorrect.") { }
    }

    /// <summary>
    /// Thrown when user account is deactivated
    /// </summary>
    public class AccountDeactivatedException : AuthenticationException
    {
        public AccountDeactivatedException() : base("User account is deactivated.") { }
    }

    /// <summary>
    /// Thrown when user account is locked
    /// </summary>
    public class AccountLockedException : AuthenticationException
    {
        public AccountLockedException(double minutesRemaining)
            : base($"Account is locked. Try again in {Math.Ceiling(minutesRemaining)} minutes.") { }
    }

    /// <summary>
    /// Thrown when order cannot be cancelled
    /// </summary>
    public class OrderCancellationException : BusinessRuleException
    {
        public OrderCancellationException(string reason) : base(reason) { }
    }

    /// <summary>
    /// Thrown when file validation fails
    /// </summary>
    public class InvalidFileException : ValidationException
    {
        public InvalidFileException(string message) : base(message) { }
    }
}