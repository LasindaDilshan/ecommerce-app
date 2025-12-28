using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductQAController : ControllerBase
{
    private readonly IProductQAService _qaService;

    public ProductQAController(IProductQAService qaService)
    {
        _qaService = qaService;
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? int.Parse(userIdClaim) : null;
    }

    #region Questions

    [HttpGet("products/{productId}/questions")]
    public async Task<ActionResult<QuestionListResponse>> GetProductQuestions(
        int productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var result = await _qaService.GetProductQuestionsAsync(productId, page, pageSize, userId);
        return Ok(result);
    }

    [HttpGet("questions/{questionId}")]
    public async Task<ActionResult<ProductQuestionDto>> GetQuestion(int questionId)
    {
        var userId = GetUserId();
        var question = await _qaService.GetQuestionByIdAsync(questionId, userId);

        if (question == null)
        {
            return NotFound(new { message = "Question not found" });
        }

        return Ok(question);
    }

    [HttpPost("questions")]
    [Authorize]
    public async Task<ActionResult<ProductQuestionDto>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        try
        {
            var userId = GetUserId()!.Value;
            var question = await _qaService.CreateQuestionAsync(userId, request);
            return CreatedAtAction(nameof(GetQuestion), new { questionId = question.Id }, question);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("questions/{questionId}")]
    [Authorize]
    public async Task<ActionResult<ProductQuestionDto>> UpdateQuestion(int questionId, [FromBody] string newText)
    {
        try
        {
            var userId = GetUserId()!.Value;
            var question = await _qaService.UpdateQuestionAsync(questionId, userId, newText);
            return Ok(question);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("questions/{questionId}")]
    [Authorize]
    public async Task<ActionResult> DeleteQuestion(int questionId)
    {
        var userId = GetUserId()!.Value;
        var isAdmin = User.IsInRole("Admin");
        var result = await _qaService.DeleteQuestionAsync(questionId, userId, isAdmin);

        if (!result)
        {
            return NotFound(new { message = "Question not found or you don't have permission to delete it" });
        }

        return NoContent();
    }

    #endregion

    #region Answers

    [HttpPost("answers")]
    [Authorize]
    public async Task<ActionResult<ProductAnswerDto>> CreateAnswer([FromBody] CreateAnswerRequest request)
    {
        try
        {
            var userId = GetUserId()!.Value;
            var answer = await _qaService.CreateAnswerAsync(userId, request);
            return Ok(answer);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("answers/{answerId}")]
    [Authorize]
    public async Task<ActionResult<ProductAnswerDto>> UpdateAnswer(int answerId, [FromBody] string newText)
    {
        try
        {
            var userId = GetUserId()!.Value;
            var answer = await _qaService.UpdateAnswerAsync(answerId, userId, newText);
            return Ok(answer);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("answers/{answerId}")]
    [Authorize]
    public async Task<ActionResult> DeleteAnswer(int answerId)
    {
        var userId = GetUserId()!.Value;
        var isAdmin = User.IsInRole("Admin");
        var result = await _qaService.DeleteAnswerAsync(answerId, userId, isAdmin);

        if (!result)
        {
            return NotFound(new { message = "Answer not found or you don't have permission to delete it" });
        }

        return NoContent();
    }

    #endregion

    #region Voting

    [HttpPost("questions/{questionId}/vote")]
    [Authorize]
    public async Task<ActionResult<VoteResponse>> VoteQuestion(int questionId)
    {
        var userId = GetUserId()!.Value;
        var result = await _qaService.VoteQuestionAsync(questionId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("questions/{questionId}/vote")]
    [Authorize]
    public async Task<ActionResult<VoteResponse>> RemoveQuestionVote(int questionId)
    {
        var userId = GetUserId()!.Value;
        var result = await _qaService.RemoveQuestionVoteAsync(questionId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("answers/{answerId}/vote")]
    [Authorize]
    public async Task<ActionResult<VoteResponse>> VoteAnswer(int answerId, [FromQuery] bool helpful = true)
    {
        var userId = GetUserId()!.Value;
        var result = await _qaService.VoteAnswerAsync(answerId, userId, helpful);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("answers/{answerId}/vote")]
    [Authorize]
    public async Task<ActionResult<VoteResponse>> RemoveAnswerVote(int answerId)
    {
        var userId = GetUserId()!.Value;
        var result = await _qaService.RemoveAnswerVoteAsync(answerId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    #endregion

    #region Moderation (Admin)

    [HttpGet("admin/questions/pending")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<ProductQuestionDto>>> GetPendingQuestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var questions = await _qaService.GetPendingQuestionsAsync(page, pageSize);
        return Ok(questions);
    }

    [HttpGet("admin/answers/pending")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<ProductAnswerDto>>> GetPendingAnswers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var answers = await _qaService.GetPendingAnswersAsync(page, pageSize);
        return Ok(answers);
    }

    [HttpPut("admin/questions/{questionId}/moderate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductQuestionDto>> ModerateQuestion(
        int questionId,
        [FromBody] ModerateQuestionRequest request)
    {
        try
        {
            var question = await _qaService.ModerateQuestionAsync(questionId, request);
            return Ok(question);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("admin/answers/{answerId}/moderate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductAnswerDto>> ModerateAnswer(
        int answerId,
        [FromBody] ModerateAnswerRequest request)
    {
        try
        {
            var answer = await _qaService.ModerateAnswerAsync(answerId, request);
            return Ok(answer);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    #endregion

    #region User's Q&A

    [HttpGet("my/questions")]
    [Authorize]
    public async Task<ActionResult<List<ProductQuestionDto>>> GetMyQuestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId()!.Value;
        var questions = await _qaService.GetUserQuestionsAsync(userId, page, pageSize);
        return Ok(questions);
    }

    [HttpGet("my/answers")]
    [Authorize]
    public async Task<ActionResult<List<ProductAnswerDto>>> GetMyAnswers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId()!.Value;
        var answers = await _qaService.GetUserAnswersAsync(userId, page, pageSize);
        return Ok(answers);
    }

    #endregion
}
