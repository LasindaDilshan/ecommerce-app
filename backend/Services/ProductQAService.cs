using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class ProductQAService : IProductQAService
{
    private readonly ApplicationDbContext _context;

    public ProductQAService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Questions

    public async Task<QuestionListResponse> GetProductQuestionsAsync(int productId, int page = 1, int pageSize = 10, int? currentUserId = null)
    {
        var query = _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.ProductId == productId && q.IsApproved)
            .OrderByDescending(q => q.UpvoteCount)
            .ThenByDescending(q => q.CreatedAt);

        var totalCount = await query.CountAsync();

        var questions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get user's votes if logged in
        HashSet<int> userQuestionVotes = new();
        HashSet<int> userAnswerVotes = new();

        if (currentUserId.HasValue)
        {
            var questionIds = questions.Select(q => q.Id).ToList();
            userQuestionVotes = (await _context.Set<QuestionVote>()
                .Where(v => v.UserId == currentUserId.Value && questionIds.Contains(v.QuestionId))
                .Select(v => v.QuestionId)
                .ToListAsync())
                .ToHashSet();

            var answerIds = questions.SelectMany(q => q.Answers.Select(a => a.Id)).ToList();
            userAnswerVotes = (await _context.Set<AnswerVote>()
                .Where(v => v.UserId == currentUserId.Value && answerIds.Contains(v.AnswerId))
                .Select(v => v.AnswerId)
                .ToListAsync())
                .ToHashSet();
        }

        return new QuestionListResponse
        {
            Questions = questions.Select(q => MapToQuestionDto(q, userQuestionVotes.Contains(q.Id), userAnswerVotes)).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductQuestionDto?> GetQuestionByIdAsync(int questionId, int? currentUserId = null)
    {
        var question = await _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .Include(q => q.Product)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null) return null;

        bool hasVoted = false;
        HashSet<int> userAnswerVotes = new();

        if (currentUserId.HasValue)
        {
            hasVoted = await _context.Set<QuestionVote>()
                .AnyAsync(v => v.QuestionId == questionId && v.UserId == currentUserId.Value);

            var answerIds = question.Answers.Select(a => a.Id).ToList();
            userAnswerVotes = (await _context.Set<AnswerVote>()
                .Where(v => v.UserId == currentUserId.Value && answerIds.Contains(v.AnswerId))
                .Select(v => v.AnswerId)
                .ToListAsync())
                .ToHashSet();
        }

        return MapToQuestionDto(question, hasVoted, userAnswerVotes);
    }

    public async Task<ProductQuestionDto> CreateQuestionAsync(int userId, CreateQuestionRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            throw new Exception("Product not found");
        }

        var question = new ProductQuestion
        {
            ProductId = request.ProductId,
            UserId = userId,
            QuestionText = request.QuestionText,
            IsApproved = false, // Requires moderation
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ProductQuestion>().Add(question);
        await _context.SaveChangesAsync();

        // Reload with user
        question = await _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .Include(q => q.Product)
            .FirstAsync(q => q.Id == question.Id);

        return MapToQuestionDto(question, false, new HashSet<int>());
    }

    public async Task<ProductQuestionDto> UpdateQuestionAsync(int questionId, int userId, string newText)
    {
        var question = await _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .FirstOrDefaultAsync(q => q.Id == questionId && q.UserId == userId);

        if (question == null)
        {
            throw new Exception("Question not found or you don't have permission to edit it");
        }

        question.QuestionText = newText;
        question.UpdatedAt = DateTime.UtcNow;
        question.IsApproved = false; // Re-requires moderation after edit

        await _context.SaveChangesAsync();

        return MapToQuestionDto(question, false, new HashSet<int>());
    }

    public async Task<bool> DeleteQuestionAsync(int questionId, int userId, bool isAdmin = false)
    {
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId && (q.UserId == userId || isAdmin));

        if (question == null) return false;

        _context.Set<ProductQuestion>().Remove(question);
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Answers

    public async Task<ProductAnswerDto> CreateAnswerAsync(int userId, CreateAnswerRequest request)
    {
        var question = await _context.Set<ProductQuestion>()
            .Include(q => q.Product)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

        if (question == null)
        {
            throw new Exception("Question not found");
        }

        // Check if user has purchased the product
        var isVerifiedPurchase = await _context.OrderItems
            .Include(oi => oi.Order)
            .AnyAsync(oi => oi.ProductId == question.ProductId &&
                           oi.Order.UserId == userId &&
                           oi.Order.Status == OrderStatus.Delivered);

        var answer = new ProductAnswer
        {
            QuestionId = request.QuestionId,
            UserId = userId,
            AnswerText = request.AnswerText,
            IsVerifiedPurchase = isVerifiedPurchase,
            IsApproved = false, // Requires moderation
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ProductAnswer>().Add(answer);

        // Update question's answered status
        question.IsAnswered = true;

        await _context.SaveChangesAsync();

        // Reload with user
        answer = await _context.Set<ProductAnswer>()
            .Include(a => a.User)
            .FirstAsync(a => a.Id == answer.Id);

        return MapToAnswerDto(answer, false);
    }

    public async Task<ProductAnswerDto> UpdateAnswerAsync(int answerId, int userId, string newText)
    {
        var answer = await _context.Set<ProductAnswer>()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == answerId && a.UserId == userId);

        if (answer == null)
        {
            throw new Exception("Answer not found or you don't have permission to edit it");
        }

        answer.AnswerText = newText;
        answer.UpdatedAt = DateTime.UtcNow;
        answer.IsApproved = false; // Re-requires moderation after edit

        await _context.SaveChangesAsync();

        return MapToAnswerDto(answer, false);
    }

    public async Task<bool> DeleteAnswerAsync(int answerId, int userId, bool isAdmin = false)
    {
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId && (a.UserId == userId || isAdmin));

        if (answer == null) return false;

        _context.Set<ProductAnswer>().Remove(answer);
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Voting

    public async Task<VoteResponse> VoteQuestionAsync(int questionId, int userId)
    {
        var question = await _context.Set<ProductQuestion>().FindAsync(questionId);
        if (question == null)
        {
            return new VoteResponse { Success = false, Message = "Question not found" };
        }

        var existingVote = await _context.Set<QuestionVote>()
            .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);

        if (existingVote != null)
        {
            return new VoteResponse { Success = false, Message = "You have already voted on this question", NewCount = question.UpvoteCount };
        }

        var vote = new QuestionVote
        {
            QuestionId = questionId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<QuestionVote>().Add(vote);
        question.UpvoteCount++;
        await _context.SaveChangesAsync();

        return new VoteResponse { Success = true, Message = "Vote recorded", NewCount = question.UpvoteCount };
    }

    public async Task<VoteResponse> RemoveQuestionVoteAsync(int questionId, int userId)
    {
        var question = await _context.Set<ProductQuestion>().FindAsync(questionId);
        if (question == null)
        {
            return new VoteResponse { Success = false, Message = "Question not found" };
        }

        var vote = await _context.Set<QuestionVote>()
            .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);

        if (vote == null)
        {
            return new VoteResponse { Success = false, Message = "You haven't voted on this question", NewCount = question.UpvoteCount };
        }

        _context.Set<QuestionVote>().Remove(vote);
        question.UpvoteCount = Math.Max(0, question.UpvoteCount - 1);
        await _context.SaveChangesAsync();

        return new VoteResponse { Success = true, Message = "Vote removed", NewCount = question.UpvoteCount };
    }

    public async Task<VoteResponse> VoteAnswerAsync(int answerId, int userId, bool isHelpful)
    {
        var answer = await _context.Set<ProductAnswer>().FindAsync(answerId);
        if (answer == null)
        {
            return new VoteResponse { Success = false, Message = "Answer not found" };
        }

        var existingVote = await _context.Set<AnswerVote>()
            .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);

        if (existingVote != null)
        {
            return new VoteResponse { Success = false, Message = "You have already voted on this answer", NewCount = answer.HelpfulCount };
        }

        var vote = new AnswerVote
        {
            AnswerId = answerId,
            UserId = userId,
            IsHelpful = isHelpful,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AnswerVote>().Add(vote);
        if (isHelpful)
        {
            answer.HelpfulCount++;
        }
        await _context.SaveChangesAsync();

        return new VoteResponse { Success = true, Message = "Vote recorded", NewCount = answer.HelpfulCount };
    }

    public async Task<VoteResponse> RemoveAnswerVoteAsync(int answerId, int userId)
    {
        var answer = await _context.Set<ProductAnswer>().FindAsync(answerId);
        if (answer == null)
        {
            return new VoteResponse { Success = false, Message = "Answer not found" };
        }

        var vote = await _context.Set<AnswerVote>()
            .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);

        if (vote == null)
        {
            return new VoteResponse { Success = false, Message = "You haven't voted on this answer", NewCount = answer.HelpfulCount };
        }

        if (vote.IsHelpful)
        {
            answer.HelpfulCount = Math.Max(0, answer.HelpfulCount - 1);
        }
        _context.Set<AnswerVote>().Remove(vote);
        await _context.SaveChangesAsync();

        return new VoteResponse { Success = true, Message = "Vote removed", NewCount = answer.HelpfulCount };
    }

    #endregion

    #region Moderation

    public async Task<List<ProductQuestionDto>> GetPendingQuestionsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .Include(q => q.Product)
            .Where(q => !q.IsApproved)
            .OrderBy(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new ProductQuestionDto
            {
                Id = q.Id,
                ProductId = q.ProductId,
                ProductName = q.Product.Name,
                UserId = q.UserId,
                UserName = q.User.FirstName + " " + q.User.LastName,
                QuestionText = q.QuestionText,
                IsApproved = q.IsApproved,
                IsAnswered = q.IsAnswered,
                UpvoteCount = q.UpvoteCount,
                AnswerCount = q.Answers.Count,
                CreatedAt = q.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProductAnswerDto>> GetPendingAnswersAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Set<ProductAnswer>()
            .Include(a => a.User)
            .Where(a => !a.IsApproved)
            .OrderBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ProductAnswerDto
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                UserId = a.UserId,
                UserName = a.User.FirstName + " " + a.User.LastName,
                AnswerText = a.AnswerText,
                IsApproved = a.IsApproved,
                IsVerifiedPurchase = a.IsVerifiedPurchase,
                IsSellerAnswer = a.IsSellerAnswer,
                IsAccepted = a.IsAccepted,
                HelpfulCount = a.HelpfulCount,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductQuestionDto> ModerateQuestionAsync(int questionId, ModerateQuestionRequest request)
    {
        var question = await _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .Include(q => q.Product)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
        {
            throw new Exception("Question not found");
        }

        question.IsApproved = request.IsApproved;
        await _context.SaveChangesAsync();

        return MapToQuestionDto(question, false, new HashSet<int>());
    }

    public async Task<ProductAnswerDto> ModerateAnswerAsync(int answerId, ModerateAnswerRequest request)
    {
        var answer = await _context.Set<ProductAnswer>()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == answerId);

        if (answer == null)
        {
            throw new Exception("Answer not found");
        }

        answer.IsApproved = request.IsApproved;
        answer.IsAccepted = request.IsAccepted;
        await _context.SaveChangesAsync();

        return MapToAnswerDto(answer, false);
    }

    #endregion

    #region User's Q&A

    public async Task<List<ProductQuestionDto>> GetUserQuestionsAsync(int userId, int page = 1, int pageSize = 10)
    {
        return await _context.Set<ProductQuestion>()
            .Include(q => q.User)
            .Include(q => q.Product)
            .Include(q => q.Answers)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new ProductQuestionDto
            {
                Id = q.Id,
                ProductId = q.ProductId,
                ProductName = q.Product.Name,
                UserId = q.UserId,
                UserName = q.User.FirstName + " " + q.User.LastName,
                QuestionText = q.QuestionText,
                IsApproved = q.IsApproved,
                IsAnswered = q.IsAnswered,
                UpvoteCount = q.UpvoteCount,
                AnswerCount = q.Answers.Count(a => a.IsApproved),
                CreatedAt = q.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProductAnswerDto>> GetUserAnswersAsync(int userId, int page = 1, int pageSize = 10)
    {
        return await _context.Set<ProductAnswer>()
            .Include(a => a.User)
            .Include(a => a.Question)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ProductAnswerDto
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                UserId = a.UserId,
                UserName = a.User.FirstName + " " + a.User.LastName,
                AnswerText = a.AnswerText,
                IsApproved = a.IsApproved,
                IsVerifiedPurchase = a.IsVerifiedPurchase,
                IsSellerAnswer = a.IsSellerAnswer,
                IsAccepted = a.IsAccepted,
                HelpfulCount = a.HelpfulCount,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    #endregion

    #region Private Helpers

    private ProductQuestionDto MapToQuestionDto(ProductQuestion question, bool hasUserVoted, HashSet<int> userAnswerVotes)
    {
        return new ProductQuestionDto
        {
            Id = question.Id,
            ProductId = question.ProductId,
            ProductName = question.Product?.Name ?? "",
            UserId = question.UserId,
            UserName = question.User != null ? $"{question.User.FirstName} {question.User.LastName}" : "",
            QuestionText = question.QuestionText,
            IsApproved = question.IsApproved,
            IsAnswered = question.IsAnswered,
            UpvoteCount = question.UpvoteCount,
            AnswerCount = question.Answers?.Count ?? 0,
            CreatedAt = question.CreatedAt,
            HasUserVoted = hasUserVoted,
            Answers = question.Answers?.Select(a => MapToAnswerDto(a, userAnswerVotes.Contains(a.Id))).ToList() ?? new()
        };
    }

    private ProductAnswerDto MapToAnswerDto(ProductAnswer answer, bool hasUserVoted)
    {
        return new ProductAnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            UserId = answer.UserId,
            UserName = answer.User != null ? $"{answer.User.FirstName} {answer.User.LastName}" : "",
            AnswerText = answer.AnswerText,
            IsApproved = answer.IsApproved,
            IsVerifiedPurchase = answer.IsVerifiedPurchase,
            IsSellerAnswer = answer.IsSellerAnswer,
            IsAccepted = answer.IsAccepted,
            HelpfulCount = answer.HelpfulCount,
            CreatedAt = answer.CreatedAt,
            HasUserVoted = hasUserVoted
        };
    }

    #endregion
}
