using AutoMapper;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // UserTelegramConnection
            CreateMap<UserTelegramConnectionEntity, UserTelegramConnectionModel>().ReverseMap();
            CreateMap<UserTelegramConnectionEntity, CreateUserTelegramConnectionRequest>().ReverseMap();
            CreateMap<UserTelegramConnectionEntity, UpdateUserTelegramConnectionRequest>().ReverseMap();

            // TelegramBotCommand
            CreateMap<TelegramBotCommandEntity, TelegramBotCommandModel>().ReverseMap();
            CreateMap<TelegramBotCommandEntity, CreateTelegramBotCommandRequest>().ReverseMap();
            CreateMap<TelegramBotCommandEntity, UpdateTelegramBotCommandRequest>().ReverseMap();

            // Role
            CreateMap<RoleEntity, RoleModel>().ReverseMap();
            CreateMap<RoleEntity, CreateRoleRequest>().ReverseMap();

            // User
            CreateMap<UserEntity, UserModel>().ReverseMap();

            // Exception
            CreateMap<ExceptionItemEntity, ExceptionItemModel>().ReverseMap();

            // Permission
            CreateMap<PermissionEntity, PermissionModel>().ReverseMap();

            // Translation
            CreateMap<TranslationEntity, TranslationModel>().ReverseMap();
            CreateMap<TranslationEntity, CreateTranslationRequest>().ReverseMap();
            CreateMap<TranslationEntity, UpdateLanguageRequest>().ReverseMap();

            // Langugage
            CreateMap<ChangeLanguageRequest, ChangeLanguageResult>().ReverseMap();
            CreateMap<LanguageEntity, LanguageModel>().ReverseMap();
            CreateMap<LanguageEntity, CreateLanguageRequest>().ReverseMap();
            CreateMap<LanguageEntity, UpdateLanguageRequest>().ReverseMap();

            // File
            CreateMap<FileEntity, FileModel>().ReverseMap();

            // Chat
            CreateMap<ChatMessageEntity, ChatMessageModel>().ReverseMap();

            // Vocab Word
            CreateMap<VocabWordEntity, VocabWordModel>();
            CreateMap<CreateVocabWordRequest, VocabWordEntity>();
            CreateMap<UpdateVocabWordRequest, VocabWordEntity>();

            // Vocab Topic
            CreateMap<VocabTopicEntity, VocabTopicModel>();
            CreateMap<CreateVocabTopicRequest, VocabTopicEntity>();
            CreateMap<UpdateVocabTopicRequest, VocabTopicEntity>();

            // Vocab Deck
            CreateMap<CreateVocabDeckRequest, VocabDeckEntity>();

            // CodeSequence
            CreateMap<CodeSequenceEntity, CodeSequenceModel>();
            CreateMap<CreateCodeSequenceRequest, CodeSequenceEntity>();
            CreateMap<UpdateCodeSequenceRequest, CodeSequenceEntity>();

            // ShortUrl
            CreateMap<ShortUrlEntity, ShortUrlModel>();
            CreateMap<CreateShortUrlRequest, ShortUrlEntity>();

            // Note
            CreateMap<NoteEntity, NoteModel>();
            CreateMap<NoteTagEntity, NoteTagModel>();
            CreateMap<CreateNoteRequest, NoteEntity>();
            CreateMap<UpdateNoteRequest, NoteEntity>();

            // Todo — OpenCount, ListName/ListColor, ChecklistTotal/Done do service tính riêng.
            // Thời gian lưu UTC nhưng MySQL trả Kind Unspecified: đánh dấu UTC để JSON có hậu tố Z.
            CreateMap<TodoListEntity, TodoListModel>();
            CreateMap<TodoTaskEntity, TodoTaskModel>()
                .ForMember(d => d.ReminderAt, o => o.MapFrom(s => AsUtc(s.ReminderAt)))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s => AsUtc(s.CompletedAt)))
                .ForMember(d => d.CreatedTime, o => o.MapFrom(s => DateTime.SpecifyKind(s.CreatedTime, DateTimeKind.Utc)))
                .ForMember(d => d.NextOccurrenceDueDate, o => o.Ignore());
            CreateMap<TodoTaskEntity, TodoTaskDetailModel>()
                .IncludeBase<TodoTaskEntity, TodoTaskModel>();
            CreateMap<TodoChecklistItemEntity, TodoChecklistItemModel>();
        }

        private static DateTime? AsUtc(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    }
}
