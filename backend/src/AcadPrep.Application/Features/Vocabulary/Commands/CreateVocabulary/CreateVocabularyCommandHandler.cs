using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Commands.CreateVocabulary;

public class CreateVocabularyCommandHandler : IRequestHandler<CreateVocabularyCommand, Result<int>>
{
    private readonly IAppDbContext _context;
    private readonly IAiGenerationService _aiGenerationService;
    private readonly IMediator _mediator;
    private readonly IBillingAccessService _billing;

    public CreateVocabularyCommandHandler(
        IAppDbContext context,
        IAiGenerationService aiGenerationService,
        IMediator mediator,
        IBillingAccessService billing)
    {
        _context = context;
        _aiGenerationService = aiGenerationService;
        _mediator = mediator;
        _billing = billing;
    }

    public async Task<Result<int>> Handle(CreateVocabularyCommand request, CancellationToken cancellationToken)
    {
        var quota = await _billing.EnsureCanSaveVocabularyAsync(request.UserId, cancellationToken);
        if (!quota.Allowed)
        {
            return Result<int>.Failure($"{quota.ErrorCode}|{quota.Message}");
        }

        // 1. Create Vocabulary
        var vocabulary = new Domain.Entities.Vocabulary
        {
            Word = request.Word,
            Phonetic = request.Phonetic ?? string.Empty,
            Meaning = request.Meaning,
            Example = request.Example ?? string.Empty
        };

        _context.Vocabularies.Add(vocabulary);
        
        // Save first to get the Id generated
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Automatically generate context passage using AI
        var aiPassage = await _aiGenerationService.GenerateVocabularyContextAsync(request.Word, cancellationToken);

        var vocabPassage = new VocabPassage
        {
            VocabularyId = vocabulary.Id,
            Content = aiPassage
        };

        _context.VocabPassages.Add(vocabPassage);

        // 3. Save to User's Notebook
        var savedVocab = new SavedVocabulary
        {
            UserId = request.UserId,
            VocabularyId = vocabulary.Id,
            Interval = 1,
            NextReviewDate = DateTime.UtcNow.Date.AddDays(1),
            DateSaved = DateTime.UtcNow
        };
        _context.SavedVocabularies.Add(savedVocab);

        await _context.SaveChangesAsync(cancellationToken);

        // 4. Check for vocabulary-related achievements
        await _mediator.Send(new AcadPrep.Application.Features.Performance.Commands.CheckAndGrantAchievements.CheckAndGrantAchievementsCommand(request.UserId), cancellationToken);

        return Result<int>.Success(vocabulary.Id);
    }
}
