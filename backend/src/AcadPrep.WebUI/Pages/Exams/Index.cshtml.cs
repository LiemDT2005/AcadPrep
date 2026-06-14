using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace AcadPrep.WebUI.Pages.Exams
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SelectedSeries { get; set; } = "Tất cả";

        [BindProperty(SupportsGet = true)]
        public string SelectedYear { get; set; } = "Tất cả";

        [BindProperty(SupportsGet = true)]
        public string SelectedDifficulty { get; set; } = "Tất cả";

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public List<string> SeriesFilters { get; set; } = new() { "Tất cả", "ETS TOEIC", "Hacker TOEIC", "New Economy" };
        public List<string> YearFilters { get; set; } = new() { "Tất cả", "2024", "2023", "2022" };
        public List<string> DifficultyFilters { get; set; } = new() { "Tất cả", "Mục tiêu 450+", "Mục tiêu 650+", "Mục tiêu 800+" };

        public List<ExamMockDto> Exams { get; set; } = new();

        public void OnGet()
        {
            // Mock data representing AcadPrep Exam entities and ExamSeries
            Exams = new List<ExamMockDto>
            {
                new ExamMockDto
                {
                    Id = 1,
                    Title = "ETS TOEIC 2024 - Test 1",
                    SeriesName = "ETS TOEIC",
                    Year = 2024,
                    Difficulty = "Mục tiêu 650+",
                    Duration = 120,
                    QuestionCount = 200,
                    AttemptCount = 15243,
                    CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah"
                },
                new ExamMockDto
                {
                    Id = 2,
                    Title = "ETS TOEIC 2024 - Test 2",
                    SeriesName = "ETS TOEIC",
                    Year = 2024,
                    Difficulty = "Mục tiêu 650+",
                    Duration = 120,
                    QuestionCount = 200,
                    AttemptCount = 9812,
                    CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah"
                },
                new ExamMockDto
                {
                    Id = 3,
                    Title = "Hacker TOEIC Vol 3 - Test 1",
                    SeriesName = "Hacker TOEIC",
                    Year = 2023,
                    Difficulty = "Mục tiêu 800+",
                    Duration = 120,
                    QuestionCount = 200,
                    AttemptCount = 12450,
                    CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah"
                },
                new ExamMockDto
                {
                    Id = 4,
                    Title = "New Economy TOEIC 2022 - Test 3",
                    SeriesName = "New Economy",
                    Year = 2022,
                    Difficulty = "Mục tiêu 450+",
                    Duration = 120,
                    QuestionCount = 200,
                    AttemptCount = 5410,
                    CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah"
                },
                new ExamMockDto
                {
                    Id = 5,
                    Title = "ETS TOEIC 2023 - Test 5",
                    SeriesName = "ETS TOEIC",
                    Year = 2023,
                    Difficulty = "Mục tiêu 650+",
                    Duration = 120,
                    QuestionCount = 200,
                    AttemptCount = 18500,
                    CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah"
                },
                new ExamMockDto
                {
                    Id = 6,
                    Title = "Hacker TOEIC Vol 3 - Test 2",
                    SeriesName = "Hacker TOEIC",
                    Year = 2023,
                    Difficulty = "Mục tiêu 800+",
                    Duration = 120,
                    QuestionCount = 200,
                    AttemptCount = 7620,
                    CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah"
                }
            };
        }
    }

    public class ExamMockDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SeriesName { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
    }
}
