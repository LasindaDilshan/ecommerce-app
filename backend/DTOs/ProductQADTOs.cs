namespace EcommerceAPI.DTOs;

public class ProductQuestionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsAnswered { get; set; }
    public int UpvoteCount { get; set; }
    public int AnswerCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductAnswerDto> Answers { get; set; } = new();
    public bool HasUserVoted { get; set; }
}

public class ProductAnswerDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsSellerAnswer { get; set; }
    public bool IsAccepted { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasUserVoted { get; set; }
}

public class CreateQuestionRequest
{
    public int ProductId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
}

public class CreateAnswerRequest
{
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}

public class QuestionListResponse
{
    public List<ProductQuestionDto> Questions { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class VoteResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int NewCount { get; set; }
}

public class ModerateQuestionRequest
{
    public bool IsApproved { get; set; }
}

public class ModerateAnswerRequest
{
    public bool IsApproved { get; set; }
    public bool IsAccepted { get; set; }
}
