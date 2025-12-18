using AutoMapper;
using Dev.Acadmy.Chapters;
using Dev.Acadmy.Courses;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Questions;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Supports;
using Dev.Acadmy.Universites;

namespace Dev.Acadmy;

public class AcadmyApplicationAutoMapperProfile : Profile
{
    public AcadmyApplicationAutoMapperProfile()
    {
        CreateMap<College, CollegeDto>();
        CreateMap<CreateUpdateCollegeDto, College>();

        // Course
        CreateMap<Entities.Courses.Entities.Course, CourseDto>();
        CreateMap<CreateUpdateCourseDto, Entities.Courses.Entities.Course>();

        // Chapter
        CreateMap<Chapter, ChapterDto>();
        CreateMap<CreateUpdateChapterDto, Chapter>();

        // CourseStudent
        CreateMap<CourseStudent, CourseStudentDto>();
        CreateMap<CreateUpdateCourseStudentDto, CourseStudent>();

        // Lecture
        CreateMap<Lecture, LectureDto>();
        CreateMap<CreateUpdateLectureDto, Lecture>();

        // MediaItem
        CreateMap<MediaItem, MediaItemDto>();
        CreateMap<CreateUpdateMediaItemDto, MediaItem>();

        // Question
        CreateMap<Question, Dev.Acadmy.Questions.QuestionDto>();

        // QuestionDto -> Question (لو عايز تـ update direct)
        CreateMap<CreateUpdateQuestionDto, Question>();

        // QuestionAnswer
        CreateMap<Questions.QuestionAnswer, QuestionAnswerDto>();
        CreateMap<CreateUpdateQuestionAnswerDto, Questions.QuestionAnswer>();

        // QuestionBank
        CreateMap<QuestionBank, QuestionBankDto>();
        CreateMap<CreateUpdateQuestionBankDto, QuestionBank>();

        // QuestionType
        CreateMap<QuestionType, QuestionTypeDto>();
        CreateMap<CreateUpdateQuestionTypeDto, QuestionType>();

        // Quiz
        CreateMap<Quiz, QuizDto>();
        CreateMap<CreateUpdateQuizDto, Quiz>();

        // QuizStudent
        CreateMap<QuizStudent, QuizStudentDto>();
        CreateMap<CreateUpdateQuizStudentDto, QuizStudent>();

        CreateMap<Subject, SubjectDto>();
        CreateMap<CreateUpdateSubjectDto, Subject>();

        CreateMap<CourseInfo, CourseInfoDto>();
        CreateMap<CreateUpdateCourseInfoDto, CourseInfo>();

        CreateMap<University,UniversityDto>();
        CreateMap<CreateUpdateUniversityDto ,  University>();

        CreateMap<GradeLevel,GradeLevelDto>();  
        CreateMap<CreateUpdateGradeLevelDto, GradeLevel>();

        CreateMap<Support, SupportDto>();
        CreateMap<CreateUpdateSupportDto , Support>();

        CreateMap<Exam, ExamDto>();
        CreateMap<CreateUpdateExamDto, Exam>();

        CreateMap<LookupDto, QuestionBank>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)); ;
        CreateMap<LookupDto, QuestionType>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        CreateMap<LookupDto, Quiz>().ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name));
        CreateMap<LookupDto, Question>().ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name));
        CreateMap<LookupDto, College>();
        CreateMap<LookupDto, Entities.Courses.Entities.Course>();
        CreateMap<LookupDto, Chapter>();
        CreateMap<LookupDto, Subject>();
        CreateMap<LookupDto, University>();
        CreateMap<LookupDto, GradeLevel>();
        CreateMap<LookupDto, Term>();

        CreateMap<QuestionBank, LookupDto>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        CreateMap<QuestionType, LookupDto>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        CreateMap<Quiz, LookupDto>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));
        CreateMap<Question, LookupDto>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));
        CreateMap<College, LookupDto>();
        CreateMap<Entities.Courses.Entities.Course, LookupDto>();
        CreateMap<Chapter, LookupDto>();
        CreateMap<Subject ,LookupDto>();
        CreateMap<University,LookupDto>();
        CreateMap<GradeLevel, LookupDto>();
        CreateMap<Term, LookupDto>();

    }
}
