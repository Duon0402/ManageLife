using ManageLife.Interfaces;

namespace ManageLife.Services
{
    public class SequentialCodeGenerator : ISequentialCodeGenerator
    {
        private readonly ICodeSequenceRepository _repo;

        public SequentialCodeGenerator(ICodeSequenceRepository repo) => _repo = repo;

        public async Task<string> NextAsync(string category, CancellationToken ct = default)
        {
            var entity = await _repo.IncrementAndGetAsync(category, ct);
            return $"{entity.Prefix}{entity.CurrentSeq.ToString().PadLeft(entity.NumberLength, '0')}{entity.Suffix}";
        }
    }
}
