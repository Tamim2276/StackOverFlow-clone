namespace StackOverflowLite.Application.Features.Questions;

public record QuestionSummaryDto(
    Guid Id,
    string Title,
    string AuthorName,
    int VoteScore,
    int AnswerCount,
    int ViewCount,
    List<string> Tags,
    DateTime CreatedAt);

public record QuestionDetailDto(
    Guid Id,
    string Title,
    string Body,
    string AuthorId,
    string AuthorName,
    int VoteScore,
    int ViewCount,
    Guid? AcceptedAnswerId,
    List<string> Tags,
    List<AnswerDto> Answers,
    DateTime CreatedAt);

public record AnswerDto(
    Guid Id,
    string Body,
    string AuthorId,
    string AuthorName,
    int VoteScore,
    bool IsAccepted,
    DateTime CreatedAt);
