using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface IProductQAService
{
    // Questions
    Task<QuestionListResponse> GetProductQuestionsAsync(int productId, int page = 1, int pageSize = 10, int? currentUserId = null);
    Task<ProductQuestionDto?> GetQuestionByIdAsync(int questionId, int? currentUserId = null);
    Task<ProductQuestionDto> CreateQuestionAsync(int userId, CreateQuestionRequest request);
    Task<ProductQuestionDto> UpdateQuestionAsync(int questionId, int userId, string newText);
    Task<bool> DeleteQuestionAsync(int questionId, int userId, bool isAdmin = false);

    // Answers
    Task<ProductAnswerDto> CreateAnswerAsync(int userId, CreateAnswerRequest request);
    Task<ProductAnswerDto> UpdateAnswerAsync(int answerId, int userId, string newText);
    Task<bool> DeleteAnswerAsync(int answerId, int userId, bool isAdmin = false);

    // Voting
    Task<VoteResponse> VoteQuestionAsync(int questionId, int userId);
    Task<VoteResponse> RemoveQuestionVoteAsync(int questionId, int userId);
    Task<VoteResponse> VoteAnswerAsync(int answerId, int userId, bool isHelpful);
    Task<VoteResponse> RemoveAnswerVoteAsync(int answerId, int userId);

    // Moderation (Admin)
    Task<List<ProductQuestionDto>> GetPendingQuestionsAsync(int page = 1, int pageSize = 20);
    Task<List<ProductAnswerDto>> GetPendingAnswersAsync(int page = 1, int pageSize = 20);
    Task<ProductQuestionDto> ModerateQuestionAsync(int questionId, ModerateQuestionRequest request);
    Task<ProductAnswerDto> ModerateAnswerAsync(int answerId, ModerateAnswerRequest request);

    // User's Q&A
    Task<List<ProductQuestionDto>> GetUserQuestionsAsync(int userId, int page = 1, int pageSize = 10);
    Task<List<ProductAnswerDto>> GetUserAnswersAsync(int userId, int page = 1, int pageSize = 10);
}
