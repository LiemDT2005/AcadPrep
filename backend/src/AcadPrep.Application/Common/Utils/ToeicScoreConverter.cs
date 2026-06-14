using System;

namespace AcadPrep.Application.Common.Utils;

public static class ToeicScoreConverter
{
    // Standard ETS scaled score matrices for Listening and Reading.
    // Index represents the number of correct answers (0 to 100).
    // Value represents the scaled score (5 to 495).

    private static readonly int[] ListeningScores = new int[101];
    private static readonly int[] ReadingScores = new int[101];

    static ToeicScoreConverter()
    {
        // Simple linear interpolation approximation for standard ETS table.
        // In real life, TOEIC score mapping is slightly curved.
        for (int i = 0; i <= 100; i++)
        {
            // Listening: 0-6 correct = 5. Then roughly +5 per correct. Max 495.
            if (i <= 6) ListeningScores[i] = 5;
            else ListeningScores[i] = Math.Min(495, 5 + (i - 6) * 5);

            // Reading: 0-16 correct = 5. Then roughly +5 per correct. Max 495.
            // Adjusting slightly to fit 100 questions = 495.
            if (i <= 10) ReadingScores[i] = 5;
            else ReadingScores[i] = Math.Min(495, 5 + (i - 10) * 5 + (i > 80 ? (i - 80) * 1 : 0)); 
            
            // Just ensuring max is 495
            if (i >= 98) ListeningScores[i] = 495;
            if (i >= 98) ReadingScores[i] = 495;
        }
    }

    public static int CalculateListeningScore(int correctAnswers)
    {
        correctAnswers = Math.Clamp(correctAnswers, 0, 100);
        return ListeningScores[correctAnswers];
    }

    public static int CalculateReadingScore(int correctAnswers)
    {
        correctAnswers = Math.Clamp(correctAnswers, 0, 100);
        return ReadingScores[correctAnswers];
    }

    public static int CalculateTotalScore(int listeningCorrect, int readingCorrect)
    {
        return CalculateListeningScore(listeningCorrect) + CalculateReadingScore(readingCorrect);
    }
}
