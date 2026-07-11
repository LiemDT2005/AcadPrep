namespace AcadPrep.Application.Common.Utils;

public static class ToeicPartLimits
{
  public static readonly int[] QuestionLimits = { 6, 25, 39, 30, 30, 16, 54 };

  public const int ListeningGroupQuestionCount = 3;
  public const int TextCompletionQuestionCount = 4;
  public const int ReadingSetMinQuestionCount = 2;

  public static int GetLimit(int part) =>
    part is >= 1 and <= 7 ? QuestionLimits[part - 1] : 0;

  public static bool CanAddQuestionCount(int part, int currentCount, int toAdd = 1) =>
    currentCount + toAdd <= GetLimit(part);

  public static bool IsPartFull(int part, int currentCount) =>
    currentCount >= GetLimit(part);
}
