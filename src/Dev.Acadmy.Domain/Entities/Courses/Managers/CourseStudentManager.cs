using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Microsoft.EntityFrameworkCore;
using Dev.Acadmy.Response;
using Dev.Acadmy.MediaItems;
using static Dev.Acadmy.Permissions.AcadmyPermissions;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Lectures;
using Volo.Abp.Data;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Courses;
using Dev.Acadmy.Interfaces;


namespace Dev.Acadmy.Entities.Courses.Managers
{
    public class CourseStudentManager :DomainService
    {
        private readonly IRepository<Entities.Course, Guid> _courseRepository;
        private readonly IRepository<CourseStudent> _coursestudentRepository;
        private readonly IMapper _mapper;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IRepository<QuizStudent ,Guid> _quizStudentRepository;
        private readonly IRepository<LectureTry, Guid> _lectureTryRepository;
        private readonly IMediaItemRepository _mediaItemRepository;
        public CourseStudentManager(IMediaItemRepository mediaItemRepository, CourseManager courseManager, IRepository<Entities.Course, Guid> courseRepository, IRepository<LectureTry, Guid> lectureTryRepository, IRepository<QuizStudent, Guid> quizStudentRepository, MediaItemManager mediaItemManager, ICurrentUser currentUser, IIdentityUserRepository userRepository, IMapper mapper, IRepository<CourseStudent> coursestudentRepository)
        {
            _mediaItemRepository = mediaItemRepository;
            _courseRepository = courseRepository;
            _lectureTryRepository = lectureTryRepository;
            _quizStudentRepository = quizStudentRepository;
            _mediaItemManager = mediaItemManager;
            _currentUser = currentUser;
            _userRepository =userRepository;
            _coursestudentRepository = coursestudentRepository;
            _mapper = mapper;
        }

        public async Task<ResponseApi<CourseStudentDto>> GetAsync(Guid id)
        {
            var coursestudent = await _coursestudentRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (coursestudent == null) return new ResponseApi<CourseStudentDto> { Data = null, Success = false, Message = "Not found coursestudent" };
            var user = await _userRepository.GetAsync(coursestudent.UserId);
            var dto = _mapper.Map<CourseStudentDto>(coursestudent);
            dto.Name = user.Name;
            var medaiItem = await _mediaItemManager.GetAsync(coursestudent.UserId);
            dto.LogoUrl = medaiItem?.Url??"";
            dto.Email = user.Email;
            return new ResponseApi<CourseStudentDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task AssignStudentToCourses(CreateUpdateStudentCoursesDto input)
        {
            if (input == null) return;
            var coursessStudent = await (await _coursestudentRepository.GetQueryableAsync()).Where(x=>x.UserId == input.UserId).ToListAsync();
            await _coursestudentRepository.DeleteManyAsync(coursessStudent);
            foreach (var courseId in input.CourseIds)
            {
                var courseStudent = new CourseStudent
                {
                    CourseId = courseId,
                    UserId = input.UserId,
                    IsSubscibe = true
                };
                await _coursestudentRepository.InsertAsync(courseStudent);   
            }
        }


        public async Task<PagedResultDto<CourseLookupDto>> GetListCoursesToAssginToStudentAsync(string? search,int pageNumber,int pageSize,Guid userId)
        {
            // كل الكورسات
            var queryable = await _courseRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(search))
                queryable = queryable.Where(c => c.Name.Contains(search));

            var totalCount = await queryable.CountAsync();

            // IDs الكورسات اللي الطالب مشترك فيها
            var subscribedCourseIds = await(await _coursestudentRepository.GetQueryableAsync())
                .Where(cs => cs.UserId == userId && cs.IsSubscibe)
                .Select(cs => cs.CourseId)
                .ToListAsync();

            // نجيب الصفحة المطلوبة
            var courses = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // نعمل الماب لـ DTO
            var courseLookupDtos = courses.Select(c => new CourseLookupDto
            {
                CourseId = c.Id,
                Name = c.Name,
                IsSelect = subscribedCourseIds.Contains(c.Id)
            }).ToList();

            return new PagedResultDto<CourseLookupDto>(totalCount, courseLookupDtos);
        }


        public async Task<PagedResultDto<CourseStudentDto>> GetListAsync(int pageNumber, int pageSize, bool isSubscribe, Guid courseId, string? search)
        {
            var currentUserId = _currentUser.GetId();
            var roles = await _userRepository.GetRolesAsync(currentUserId);
            var isAdmin = roles.Any(x => x.Name.ToUpper() == RoleConsts.Admin.ToUpper());

            // 1. جلب الـ Queryable مع عمل Include للمستخدم والكورس لتجنب استعلامات إضافية
            var queryable = await _coursestudentRepository.GetQueryableAsync();
            queryable = queryable.Include(x => x.User).Include(x => x.Course);

            // 2. الفلترة الأساسية
            var filteredQuery = queryable.Where(x => x.IsSubscibe == isSubscribe && x.CourseId == courseId);

            // التحقق من الصلاحيات (Admin يرى الكل، المحاضر يرى طلاب كورساته فقط)
            if (!isAdmin)
            {
                filteredQuery = filteredQuery.Where(x => x.Course.UserId == currentUserId);
            }

            // فلتر البحث النصي
            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredQuery = filteredQuery.Where(c => c.Course.Name.Contains(search) || c.User.Name.Contains(search));
            }

            // 3. حساب العدد الإجمالي قبل الـ Pagination
            var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

            // 4. جلب البيانات (Pagination)
            var list = await AsyncExecuter.ToListAsync(
                filteredQuery
                    .OrderByDescending(c => c.CreationTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
            );

            // 5. حل مشكلة الميديا دفعة واحدة باستخدام الـ Dictionary (Bulk Fetch)
            var userIds = list.Select(x => x.UserId).Distinct().ToList();
            // جلب الروابط في Dictionary: Key هو الـ UserId والقيمة هي الـ Url
            var mediaItemsDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(userIds);

            // 6. التحويل إلى DTO ودمج البيانات
            var coursestudentDtos = list.Select(cs => {
                var dto = _mapper.Map<CourseStudentDto>(cs);

                // تعبئة بيانات المستخدم (موجودة فعلياً في الذاكرة بسبب الـ Include)
                if (cs.User != null)
                {
                    dto.Name = cs.User.Name;
                    dto.Email = cs.User.Email;
                    dto.PhoneNumber = cs.User.GetProperty<string>(SetPropConsts.PhoneNumber);
                }
                // جلب الصورة من الـ Dictionary (أداء عالي جداً O(1))
                mediaItemsDic.TryGetValue(cs.UserId, out var url);
                dto.LogoUrl = url ?? "";
                return dto;
            }).ToList();

            return new PagedResultDto<CourseStudentDto>(totalCount, coursestudentDtos);
        }

        public async Task<PagedResultDto<CourseStudentDto>> GetListStudentAsync(int pageNumber, int pageSize, bool isSubscribe, string? search)
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());
            var queryable = await _coursestudentRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Include(x => x.Course).Include(x => x.User).Where(c => c.Course.Name.Contains(search) || c.User.Name.Contains(search));
            var totalCount = await AsyncExecuter.CountAsync(queryable.Where(x => x.IsSubscibe == isSubscribe ));
            var coursestudents = new List<CourseStudent>();
            if (roles.Any(x => x.Name.ToUpper() == RoleConsts.Admin.ToUpper())) coursestudents = await AsyncExecuter.ToListAsync(queryable.Include(x => x.Course).Include(x => x.User).Where(x => x.IsSubscibe == isSubscribe ).OrderByDescending(c => c.CreationTime).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            else coursestudents = await AsyncExecuter.ToListAsync(queryable.Include(x => x.Course).Include(x => x.User).Where(c => c.Course.UserId == _currentUser.GetId() && c.IsSubscibe == isSubscribe ).OrderByDescending(c => c.CreationTime).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var coursestudentDtos = _mapper.Map<List<CourseStudentDto>>(coursestudents);
            foreach (var dto in coursestudentDtos)
            {
                var user = await _userRepository.GetAsync(dto.UserId);
                dto.Name = user.Name;
                var medaiItem = await _mediaItemManager.GetAsync(user.Id);
                dto.LogoUrl = medaiItem?.Url ?? UserConsts.DefaultImg;
                dto.Email = user.Email;
                dto.PhoneNumber = user.GetProperty<string?>(SetPropConsts.PhoneNumber)?? string.Empty;
            }
            return new PagedResultDto<CourseStudentDto>(totalCount, coursestudentDtos);
        }


        public async Task<ResponseApi<CourseStudentDto>> CreateAsync(CreateUpdateCourseStudentDto input)
        {
            var coursestudent = _mapper.Map<CourseStudent>(input);
            coursestudent.IsSubscibe = false; 
            var result = await _coursestudentRepository.InsertAsync(coursestudent);
            var dto = _mapper.Map<CourseStudentDto>(result);
            return new ResponseApi<CourseStudentDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<CourseStudentDto>> UpdateAsync(Guid id, CreateUpdateCourseStudentDto input)
        {
            var coursestudentDB = await _coursestudentRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (coursestudentDB == null) return new ResponseApi<CourseStudentDto> { Data = null, Success = false, Message = "Not found coursestudent" };
            var coursestudent = _mapper.Map(input, coursestudentDB);
            if (coursestudent.IsSubscibe) 
            { 
                var result = await _coursestudentRepository.UpdateAsync(coursestudent);
                var dto = _mapper.Map<CourseStudentDto>(result);
                return new ResponseApi<CourseStudentDto> { Data = dto, Success = true, Message = "update succeess" };
            }
            else 
            {
                await _coursestudentRepository.DeleteAsync(coursestudent);
                return new ResponseApi<CourseStudentDto> { Data = null, Success = true, Message = "delete succeess" };
            }
        }
        

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var coursestudent = await _coursestudentRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (coursestudent == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found coursestudent" };
            await _coursestudentRepository.DeleteAsync(coursestudent);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task DeleteAllStudentInCourse(Guid courseId)
        {
            var queryable = await _coursestudentRepository.GetQueryableAsync();
            var courseStudents = await AsyncExecuter.ToListAsync(queryable.Where(x => x.CourseId == courseId));
            foreach (var courseStudent in courseStudents)
            {
                await _coursestudentRepository.DeleteAsync(courseStudent);
            }
        }

        public async Task DeleteAllStudentInAllCourses()
        {
            var queryable = await _coursestudentRepository.GetQueryableAsync();
            var courseStudents = await queryable.ToListAsync();
            foreach (var courseStudent in courseStudents)
            {
                await _coursestudentRepository.DeleteAsync(courseStudent);
            }
        }
        public async Task<PagedResultDto<StudentDegreeByCourseDto>> GetStudentDegreByCourseAsync(int pageNumber, int pageSize, Guid courseId, Guid userId)
        {
            // 🟢 1. الاستعلام الأساسي
            var query = await _quizStudentRepository.GetQueryableAsync();

            var studentQuizzesQuery = query
                .Include(x => x.Quiz)
                    .ThenInclude(x => x.Lecture)
                        .ThenInclude(x => x.Chapter)
                            .ThenInclude(x => x.Course)
                .Where(x => x.UserId == userId && x.Quiz.Lecture.Chapter.CourseId == courseId);

            // 🟢 2. إجمالي السجلات
            var totalCount = await studentQuizzesQuery.CountAsync();

            // 🟢 3. تطبيق Pagination
            var studentQuizzes = await studentQuizzesQuery
                .OrderByDescending(x => x.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🟢 4. تجهيز البيانات
            var user = await _userRepository.GetAsync(userId);
            var mediaItem = await _mediaItemManager.GetAsync(userId);

            var result = new StudentDegreeByCourseDto
            {
                UserId = user.Id,
                Name = user.Name,
                LogoUrl = mediaItem?.Url ?? ""
            };

            foreach (var studentQuiz in studentQuizzes)
            {
                var lectureTry = await _lectureTryRepository.FirstOrDefaultAsync(x =>
                    x.LectureId == studentQuiz.Quiz.LectureId && x.UserId == userId);

                result.Quizzes.Add(new StudentQuizDto
                {
                    QuizName = studentQuiz.Quiz?.Title ?? string.Empty,
                    LectureName = studentQuiz.Quiz?.Lecture?.Title ?? string.Empty,
                    QuizScore = studentQuiz.Score,
                    TryCount = lectureTry?.MyTryCount ?? 0
                });
            }

            // 🟢 5. إرجاع النتيجة مع pagination
            return new PagedResultDto<StudentDegreeByCourseDto>(
                totalCount,
                new List<StudentDegreeByCourseDto> { result }
            );
        }



    }
}
