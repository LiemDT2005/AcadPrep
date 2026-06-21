namespace Application.Features.Exam.Queries.GetExamDetail;

public class GetExamDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public string SeriesName { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? CoverImageUrl { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAttempts { get; set; }
    // Thông tin phân tích cấu trúc đề (Số câu hỏi mỗi Part)
    public List<PartSummaryDto> Parts { get; set; } = new();
    // Lịch sử làm bài của User hiện tại cho đề thi này
    public List<UserAttemptHistoryDto> AttemptHistory { get; set; } = new();
}

public class PartSummaryDto
{
    public int PartNumber { get; set; } // Ví dụ: 1, 2, 3...
    public string PartName { get; set; } = string.Empty; // Ví dụ: "Part 1: Photographs"
    public int QuestionCount { get; set; }
}

public class UserAttemptHistoryDto
{
    public int AttemptId { get; set; }
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RemainingTime { get; set; }
    public bool IsSubmitted { get; set; }
}