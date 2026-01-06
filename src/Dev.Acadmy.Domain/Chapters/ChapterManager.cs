using AutoMapper;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Chapters
{
    public class ChapterManager:DomainService
    {
        //private readonly LectureManager _lectureManger;
        //private readonly IMapper _mapper;
        //private readonly IIdentityUserRepository _userRepository;
        //private readonly ICurrentUser _currentUser;
        //private readonly IRepository<QuizStudent, Guid> _quizStudentRepository;
        //private readonly MediaItemManager _mediaItemManager;
        //private readonly IRepository<LectureStudent ,Guid> _lectureStudentRepository;
        //private readonly IRepository<LectureTry, Guid> _lectureTryRepository;
        //private readonly IRepository<Entities.Courses.Entities.Course , Guid> _courseRepository;
        //private readonly IdentityUserManager _userManager;
        //private readonly IMediaItemRepository _mediaItemRepository;
        //private readonly IRepository<IdentityUser, Guid> _userRepo;
        //private readonly IChapterRepository _chapterRepository;
        //public ChapterManager(IRepository<IdentityUser, Guid> userRepo, IMediaItemRepository mediaItemRepository, IdentityUserManager userManager, IRepository<Entities.Courses.Entities.Course, Guid> courseRepository, IRepository<LectureTry, Guid> lectureTryRepository, LectureManager lectureManger, IRepository<LectureStudent, Guid> lectureStudentRepository, MediaItemManager mediaItemManager, IRepository<QuizStudent, Guid> quizStudentRepository, ICurrentUser currentUser, IIdentityUserRepository userRepository, IMapper mapper, IChapterRepository chapterRepository)
        //{
        //    _userRepo = userRepo;
        //    _mediaItemRepository = mediaItemRepository;
        //    _userManager = userManager;
        //    _courseRepository = courseRepository;
        //    _lectureTryRepository = lectureTryRepository;
        //    _lectureManger = lectureManger;
        //    _lectureStudentRepository = lectureStudentRepository;
        //    _mediaItemManager = mediaItemManager;
        //    _quizStudentRepository = quizStudentRepository;
        //    _currentUser = currentUser;
        //    _userRepository = userRepository;
        //    _chapterRepository = chapterRepository;
        //    _mapper = mapper;
        //}

        protected IChapterRepository _chapterRepository => LazyServiceProvider.LazyGetRequiredService<IChapterRepository>();
        protected IMediaItemRepository _mediaItemRepository => LazyServiceProvider.LazyGetRequiredService<IMediaItemRepository>();
        protected IdentityUserManager _userManager => LazyServiceProvider.LazyGetRequiredService<IdentityUserManager>();

        protected IRepository<Entities.Courses.Entities.Course, Guid> _courseRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Entities.Courses.Entities.Course, Guid>>();
        protected IRepository<LectureTry, Guid> _lectureTryRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<LectureTry, Guid>>();
        protected LectureManager _lectureManager => LazyServiceProvider.LazyGetRequiredService<LectureManager>();
        protected IRepository<LectureStudent, Guid> _lectureStudentRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<LectureStudent, Guid>>();
        protected MediaItemManager _mediaItemManager => LazyServiceProvider.LazyGetRequiredService<MediaItemManager>();
        protected IRepository<QuizStudent, Guid> _quizStudentRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<QuizStudent, Guid>>();
        protected ICurrentUser _currentUser => LazyServiceProvider.LazyGetRequiredService<ICurrentUser>();
        protected IIdentityUserRepository _userRepository => LazyServiceProvider.LazyGetRequiredService<IIdentityUserRepository>();
        protected IRepository<IdentityUser, Guid> _identityUserRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<IdentityUser, Guid>>();
        protected IMapper _mapper => LazyServiceProvider.LazyGetRequiredService<IMapper>();

        public ChapterManager()
        {
            // Constructor فارغ - التبعيات يتم حلها عند أول استخدام (On-demand)
        }

        public async Task<ResponseApi<ChapterDto>> GetAsync(Guid id)
        {
            var chapter = await _chapterRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (chapter == null) return new ResponseApi<ChapterDto> { Data = null, Success = false, Message = "Not found chapter" };
            var dto = _mapper.Map<ChapterDto>(chapter);
            return new ResponseApi<ChapterDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<ChapterDto>> GetListAsync(int pageNumber, int pageSize, string? search, Guid courseId)
        {
            var skipCount = (pageNumber - 1) * pageSize;
            var currentUserId = _currentUser.GetId();

            // 1. إنشاء الاستعلام الأساسي مع Include و فلترة الكورس الإلزامية
            var queryable = (await _chapterRepository.GetQueryableAsync())
                .Include(x => x.Course)
                .Where(x => x.CourseId == courseId); // استخدام الـ courseId الممرر

            // 2. فلترة البحث (إذا وُجد)
            if (!string.IsNullOrWhiteSpace(search))
            {
                queryable = queryable.Where(c => c.Name.Contains(search) || c.Course.Name.Contains(search));
            }
            var user = await _userRepository.GetAsync(currentUserId);

            // 3. منطق الصلاحيات: إذا لم يكن أدمن، يرى فقط ما أنشأه
            var isAdmin = await _userManager.IsInRoleAsync( user,RoleConsts.Admin.ToLower());
            if (!isAdmin)
            {
                queryable = queryable.Where(c => c.CreatorId == currentUserId);
            }

            // 4. حساب العدد الإجمالي للنتائج المفلترة
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // 5. جلب البيانات بطلب واحد مرتب ومقسم لصفحات
            var chapters = await AsyncExecuter.ToListAsync(
                queryable.OrderByDescending(c => c.CreationTime)
                         .Skip(skipCount)
                         .Take(pageSize)
            );

            // 6. التحويل لـ DTO
            var chapterDtos = _mapper.Map<List<ChapterDto>>(chapters);

            return new PagedResultDto<ChapterDto>(totalCount, chapterDtos);
        }

        public async Task<ResponseApi<ChapterDto>> CreateAsync(CreateUpdateChapterDto input)
        {
            var chapter = _mapper.Map<Chapter>(input);
            var result = await _chapterRepository.InsertAsync(chapter);
            var dto = _mapper.Map<ChapterDto>(result);
            return new ResponseApi<ChapterDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<ChapterDto>> UpdateAsync(Guid id, CreateUpdateChapterDto input)
        {
            var chapterDB = await _chapterRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (chapterDB == null) return new ResponseApi<ChapterDto> { Data = null, Success = false, Message = "Not found chapter" };
            var chapter = _mapper.Map(input, chapterDB);
            var result = await _chapterRepository.UpdateAsync(chapter);
            var dto = _mapper.Map<ChapterDto>(result);
            return new ResponseApi<ChapterDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var chapter = await(await _chapterRepository.GetQueryableAsync()).Include(x=>x.Lectures).FirstOrDefaultAsync(x => x.Id == id);
            if (chapter == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found chapter" };
            foreach (var lec in chapter.Lectures) await _lectureManager.DeleteAsync(lec.Id);
            await _chapterRepository.DeleteAsync(chapter);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetListChaptersAsync()
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());
            var queryable = await _chapterRepository.GetQueryableAsync();
            var chapters = new List<Chapter>();
            if (roles.Any(x => x.Name.ToUpper() == RoleConsts.Admin.ToUpper())) chapters = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime).Take(100));
            else chapters = await AsyncExecuter.ToListAsync(queryable.Where(c => c.CreatorId == _currentUser.GetId()).OrderByDescending(c => c.CreationTime).Take(100));
            var chapterDtos = _mapper.Map<List<LookupDto>>(chapters);
            return new PagedResultDto<LookupDto>(chapterDtos.Count, chapterDtos);
        }

        public async Task<PagedResultDto<LookupDto>> GetChaptersByCourseLookUpAsync(Guid courseId)
        {
            // هات الـ Chapters اللي ليها نفس CourseId
            var queryable = await _chapterRepository.GetQueryableAsync();
            var chapters = await queryable
                .Where(c => c.CourseId == courseId)
                .ToListAsync();

            if (!chapters.Any())
                return new PagedResultDto<LookupDto>(0,new List<LookupDto>());

            // اعمل Map للـ DTOs
            var chapterDtos = _mapper.Map<List<LookupDto>>(chapters);

            return new PagedResultDto<LookupDto>(chapterDtos.Count(), chapterDtos);
        }

    }
}
