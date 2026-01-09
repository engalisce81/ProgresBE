using AutoMapper;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Questions;
using Dev.Acadmy.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Quizzes
{
    public class QuizManager :DomainService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<QuizStudent ,Guid> _quizStudentRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<LectureStudent, Guid> _lectureStudentRepository;
        private readonly IRepository<Lecture, Guid> _lectureRepository;
        private readonly IRepository<QuizStudentAnswer, Guid> _quizStudentAnswerRepository;
        private readonly IRepository<LectureTry, Guid> _lectureTryRepository;
        private readonly MediaItemManager _mediaitemManager;
        private readonly IRepository<Question, Guid> _questionRepository;
        private readonly IRepository<Exam, Guid> _examRepository;
        private readonly IExamQuestionRepository _examQuestionRepository;
        private readonly IRepository<ExamStudent, Guid> _examStudentRepository;
        private readonly IRepository<ExamStudentAnswer, Guid> _examStudentAnswerRepository;
        public QuizManager(IRepository<ExamStudentAnswer, Guid> examStudentAnswerRepository, IRepository<ExamStudent, Guid> examStudentRepository, IExamQuestionRepository examQuestionRepository, IRepository<Exam, Guid> examRepository, IRepository<Question, Guid> questionRepository, MediaItemManager mediaitemManager, IRepository<LectureTry, Guid> lectureTryRepository, IRepository<QuizStudentAnswer, Guid> quizStudentAnswerRepository, IRepository<Lecture, Guid> lectureRepository, IRepository<LectureStudent, Guid> lectureStudentRepository, IIdentityUserRepository userRepository, ICurrentUser currentUser, IRepository<QuizStudent , Guid> quizStudentRepository, IMapper mapper, IQuizRepository quizRepository)
        {
            _examStudentAnswerRepository = examStudentAnswerRepository;
            _examStudentRepository = examStudentRepository;
            _examQuestionRepository = examQuestionRepository;
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _mediaitemManager = mediaitemManager;   
            _lectureTryRepository = lectureTryRepository;
            _quizStudentAnswerRepository = quizStudentAnswerRepository;
            _lectureRepository = lectureRepository;
            _lectureStudentRepository = lectureStudentRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _quizRepository = quizRepository;
            _mapper = mapper;
            _quizStudentRepository = quizStudentRepository;
        }

        public async Task<ResponseApi<QuizDto>> GetAsync(Guid id)
        {
            var quiz = await _quizRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (quiz == null) return new ResponseApi<QuizDto> { Data = null, Success = false, Message = "Not found quiz" };
            var dto = _mapper.Map<QuizDto>(quiz);
            return new ResponseApi<QuizDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<List<Quiz>> GetQuizesByLectureId(Guid lectureId) => await ( await _quizRepository.GetQueryableAsync()).Where(x=>x.LectureId == lectureId).ToListAsync();
        
        public async Task DeletQuizesByLectureId(Guid lectureId)
        {
            var quizes = await (await _quizRepository.GetQueryableAsync()).Where(x => x.LectureId == lectureId).ToListAsync();
            foreach(var quiz in quizes)
            {
                var studentQuiz = await (await _quizStudentRepository.GetQueryableAsync()).Where(x => x.QuizId == quiz.Id).ToListAsync();
                await _quizStudentRepository.DeleteManyAsync(studentQuiz);
                await _quizRepository.DeleteAsync(quiz);
            }
        }

        public async Task<PagedResultDto<QuizDto>> GetListAsync(int pageNumber, int pageSize, string? search)
        {
            var queryable = await _quizRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Where(c => c.Description.Contains(search));
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var quizs = await AsyncExecuter.ToListAsync(queryable.OrderBy(c => c.CreationTime).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var quizDtos = _mapper.Map<List<QuizDto>>(quizs);
            return new PagedResultDto<QuizDto>(totalCount, quizDtos);
        }

        public async Task<ResponseApi<QuizDto>> CreateAsync(CreateUpdateQuizDto input)
        {
            var quiz = _mapper.Map<Quiz>(input);
            var result = await _quizRepository.InsertAsync(quiz ,autoSave:true);
            var dto = _mapper.Map<QuizDto>(result);
            return new ResponseApi<QuizDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<QuizDto>> UpdateAsync(Guid id, CreateUpdateQuizDto input)
        {
            var quizDB = await _quizRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (quizDB == null) return new ResponseApi<QuizDto> { Data = null, Success = false, Message = "Not found quiz" };
            var quiz = _mapper.Map(input, quizDB);
            var result = await _quizRepository.UpdateAsync(quiz,autoSave:true);
            var dto = _mapper.Map<QuizDto>(result);
            return new ResponseApi<QuizDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var quiz = await _quizRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (quiz == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found quiz" };
            await _quizRepository.DeleteAsync(quiz ,autoSave:true);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

       


        public async Task<ResponseApi<LectureQuizResultDto>> GetLectureQuizResultsAsync(Guid lectureId)
        {
            var userId = _currentUser.GetId();

            var lecture = await (await _lectureRepository.GetQueryableAsync())
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.QuestionAnswers)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
                throw new UserFriendlyException("Lecture not found");

            var quizStudents = await (await _quizStudentRepository.GetQueryableAsync())
                .Include(qs => qs.Answers)
                .Where(qs => qs.UserId == userId && qs.LectureId == lectureId)
                .ToListAsync();

            var lectureResult = new LectureQuizResultDto
            {
                LectureId = lecture.Id,
                LectureTitle = lecture.Title
            };

            foreach (var quiz in lecture.Quizzes)
            {
                var quizStudent = quizStudents.FirstOrDefault(x => x.QuizId == quiz.Id);
                if (quizStudent == null) continue;

                double totalScore = quiz.Questions.Sum(q => q.Score);

                var quizResult = new QuizResultDetailDto
                {
                    QuizId = quiz.Id,
                    QuizTitle = quiz.Title,
                    StudentScore = quizStudent.Score,
                    QuizSuccesRate = lecture.SuccessQuizRate,
                    TotalScore = totalScore,
                };

                foreach (var question in quiz.Questions)
                {
                    var studentAnswer = quizStudent.Answers.FirstOrDefault(a => a.QuestionId == question.Id);

                    var questionResult = new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Title,
                        ScoreObtained = studentAnswer?.ScoreObtained ?? 0,
                        ScoreTotal = question.Score,
                        LogoUrl = _mediaitemManager.GetAsync(question.Id).Result?.Url?? string.Empty
                    };
                    var selectAnswer = quizStudents.Where(x => x.QuizId == quiz.Id).Select(x=>x.Answers).FirstOrDefault();
                    // نضيف كل الإجابات الخاصة بالسؤال
                    foreach (var answer in question.QuestionAnswers)
                    {
                        questionResult.Answers.Add(new AnswerResultDto
                        {
                            AnswerId = answer.Id,
                            AnswerText = answer.Answer,
                            SelectText = selectAnswer?.Where(x => x.QuestionId ==question.Id)?.Select(x=>x.TextAnswer)?.FirstOrDefault()?? string.Empty,
                            IsCorrect = answer.IsCorrect,
                            IsSelected = studentAnswer?.SelectedAnswerId == answer.Id
                        });
                    }

                    quizResult.Questions.Add(questionResult);
                }

                lectureResult.Quizzes.Add(quizResult);
            }

            return new ResponseApi<LectureQuizResultDto>
            {
                Data = lectureResult,
                Success = true,
                Message = "Lecture quiz results retrieved successfully"
            };
        }


        public async Task<ResponseApi<QuizStudentDto>> MarkQuizAsync(Guid quizId, int score)
        {
            var userId = _currentUser.GetId();
            var quiz = await _quizRepository.GetAsync(quizId);

            if (quiz == null)
            {
                return new ResponseApi<QuizStudentDto>
                {
                    Success = false,
                    Message = "Quiz not found",
                    Data = null
                };
            }

            // هات المحاضرة
            var lectureId = quiz.LectureId;
            if (lectureId == null)
            {
                return new ResponseApi<QuizStudentDto>
                {
                    Success = false,
                    Message = "Quiz is not linked to a lecture",
                    Data = null
                };
            }
            var lecture = await _lectureRepository.FirstOrDefaultAsync(x => x.Id == lectureId);
            // هات LectureStudent
            var lectureStudent = await _lectureStudentRepository.FirstOrDefaultAsync(x =>
                x.LectureId == lectureId && x.UserId == userId);

            if (lectureStudent == null)
            {
                lectureStudent = new LectureStudent
                {
                    LectureId = lectureId.Value,
                    UserId = userId,
                    AttemptsUsed = 0,
                    MaxAttempts =  lecture?.QuizTryCount??0
                };
                await _lectureStudentRepository.InsertAsync(lectureStudent);
            }

            if (lectureStudent.IsCompleted)
            {
                return new ResponseApi<QuizStudentDto>
                {
                    Success = false,
                    Message = "لقد استهلكت كل المحاولات المتاحة لهذه المحاضرة",
                    Data = null
                };
            }

            // عدل المحاولات
            lectureStudent.AttemptsUsed++;
            await _lectureStudentRepository.UpdateAsync(lectureStudent);

            // سجل QuizStudent
            var quizStudent = new QuizStudent
            {
                LectureId = lectureId,
                UserId = userId,
                QuizId = quizId,
                Score = score
            };
            await _quizStudentRepository.InsertAsync(quizStudent);

            // رجع الـ DTO
            var dto = new QuizStudentDto
            {
                LectureId = lectureId.Value,
                QuizId = quizId,
                UserId = userId,
                Score = score
            };

            return new ResponseApi<QuizStudentDto>
            {
                Success = true,
                Message = "تم تسجيل نتيجتك بنجاح",
                Data = dto
            };
        }





        //public async Task<ResponseApi<QuizResultDto>> SubmitQuizAsync(QuizAnswerDto input, bool isExam)
        //{
        //    var userId = _currentUser.GetId();

        //    var quiz = await (await _quizRepository.GetQueryableAsync())
        //        .Include(q => q.Questions)
        //            .ThenInclude(q => q.QuestionAnswers)
        //        .Include(q => q.Questions)
        //            .ThenInclude(q => q.QuestionType)
        //        .Include(x => x.Lecture)
        //        .FirstOrDefaultAsync(q => q.Id == input.QuizId);

        //    if (quiz == null)
        //        throw new UserFriendlyException("Quiz not found");

        //    var lecture = quiz.Lecture;
        //    if (lecture == null)
        //        throw new UserFriendlyException("Lecture not found for this quiz");

        //    // ✅ تحقق هل الطالب جاوب الكويز قبل كده
        //    var quizStudent = await (await _quizStudentRepository.GetQueryableAsync())
        //        .Include(x => x.Answers)
        //        .FirstOrDefaultAsync(x => x.UserId == userId && x.QuizId == input.QuizId);

        //    // ✅ لو جاوب قبل كده نحذف إجاباته القديمة
        //    if (quizStudent != null)
        //    {
        //        var oldAnswers = await _quizStudentAnswerRepository
        //            .GetListAsync(x => x.QuizStudentId == quizStudent.Id);

        //        foreach (var old in oldAnswers)
        //        {
        //            await _quizStudentAnswerRepository.DeleteAsync(old, autoSave: true);
        //        }
        //    }
        //    else
        //    {
        //        // ✅ لو أول مرة يجاوب الكويز
        //        quizStudent = new QuizStudent
        //        {
        //            LectureId = lecture.Id,
        //            UserId = userId,
        //            QuizId = quiz.Id,
        //            Score = 0
        //        };

        //        await _quizStudentRepository.InsertAsync(quizStudent, autoSave: true);
        //    }

        //    double totalScore = 0;
        //    double studentScore = 0;

        //    // ✅ لف على كل الأسئلة في الكويز
        //    foreach (var question in quiz.Questions)
        //    {
        //        var studentAnswers = input.Answers
        //            .Where(a => a.QuestionId == question.Id)
        //            .ToList();

        //        if (studentAnswers == null || !studentAnswers.Any())
        //            continue;

        //        bool isCorrect = false;
        //        double obtained = 0;

        //        switch ((QuestionTypeEnum)question.QuestionType.Key)
        //        {
        //            // ✅ اختيار من متعدد / صح أو خطأ
        //            case QuestionTypeEnum.MCQ:
        //            case QuestionTypeEnum.TrueOrFalse:
        //                var selected = studentAnswers.FirstOrDefault(a => a.SelectedAnswerId != null);
        //                if (selected?.SelectedAnswerId != null)
        //                {
        //                    var correctAnswer = question.QuestionAnswers.FirstOrDefault(a => a.IsCorrect);
        //                    if (correctAnswer != null && correctAnswer.Id == selected.SelectedAnswerId)
        //                    {
        //                        isCorrect = true;
        //                        obtained = question.Score;
        //                        studentScore += question.Score;
        //                    }
        //                }
        //                break;

        //            // ✅ إجابة قصيرة
        //            case QuestionTypeEnum.ShortAnswer:
        //                {
        //                    var studentTexts = studentAnswers
        //                        .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
        //                        .Select(a => a.TextAnswer!.ToLower().Trim())
        //                        .ToList();

        //                    if (studentTexts.Any())
        //                    {
        //                        // ✅ استخراج جميع الكلمات من الإجابات الصحيحة
        //                        var keywords = question.QuestionAnswers
        //                            .SelectMany(a => a.Answer
        //                                .ToLower()
        //                                .Split(new[] { ' ', ',', '.', ';', ':', '!', '?', '-', '_', '/' },
        //                                    StringSplitOptions.RemoveEmptyEntries))
        //                            .Distinct()
        //                            .ToList();

        //                        // ✅ استخراج الكلمات من إجابات الطالب
        //                        var studentWords = studentTexts
        //                            .SelectMany(t => t
        //                                .Split(new[] { ' ', ',', '.', ';', ':', '!', '?', '-', '_', '/' },
        //                                    StringSplitOptions.RemoveEmptyEntries))
        //                            .Distinct()
        //                            .ToList();

        //                        // ✅ لو لقى أي كلمة صحيحة → الدرجة الكاملة
        //                        bool hasAnyMatch = studentWords.Any(w => keywords.Contains(w));

        //                        if (hasAnyMatch)
        //                        {
        //                            obtained = question.Score;
        //                            studentScore += obtained;
        //                            isCorrect = true;
        //                        }
        //                    }
        //                }
        //                break;

        //            // ✅ إجابة تكميلية
        //            case QuestionTypeEnum.CompleteAnswer:
        //                {
        //                    var studentTexts = studentAnswers
        //                        .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
        //                        .Select(a => a.TextAnswer!.Trim().ToLower())
        //                        .ToList();

        //                    var correctAnswers = question.QuestionAnswers
        //                        .Where(a => a.IsCorrect)
        //                        .Select(a => a.Answer.Trim().ToLower())
        //                        .ToList();

        //                    if (correctAnswers.Any() && studentTexts.Any())
        //                    {
        //                        // ✅ لو أي إجابة من الطالب تطابقت مع أي إجابة صحيحة → الدرجة الكاملة
        //                        bool hasAnyMatch = studentTexts.Any(s => correctAnswers.Contains(s));

        //                        if (hasAnyMatch)
        //                        {
        //                            obtained = question.Score;
        //                            studentScore += obtained;
        //                            isCorrect = true;
        //                        }
        //                        else
        //                        {
        //                            obtained = 0;
        //                            isCorrect = false;
        //                        }
        //                    }
        //                }
        //                break;
        //        }

        //        totalScore += question.Score;

        //        // ✅ حفظ إجابات الطالب كلها في 
        //        var combinedAnswers = studentAnswers
        //            .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
        //            .Select(a => a.TextAnswer)
        //            .ToList();

        //        var answerEntity = new QuizStudentAnswer
        //        {
        //            QuizStudentId = quizStudent.Id,
        //            QuestionId = question.Id,
        //            SelectedAnswerId = studentAnswers.FirstOrDefault()?.SelectedAnswerId,
        //            TextAnswer = (combinedAnswers != null && combinedAnswers.Any()) ? string.Join(" | ", combinedAnswers) : null,
        //            IsCorrect = isCorrect,
        //            ScoreObtained = obtained
        //        };

        //        await _quizStudentAnswerRepository.InsertAsync(answerEntity, autoSave: true);
        //    }

        //    // ✅ تحديث عدد المحاولات والدرجة
        //    quizStudent.TryCount += 1;
        //    quizStudent.Score = (int)Math.Round(studentScore);
        //    await _quizStudentRepository.UpdateAsync(quizStudent, autoSave: true);

        //    // ✅ إدارة LectureTry
        //    var lectureTry = await _lectureTryRepository
        //        .FirstOrDefaultAsync(x => x.LectureId == lecture.Id && x.UserId == userId);

        //    if (lectureTry == null)
        //    {
        //        lectureTry = new LectureTry
        //        {
        //            LectureId = lecture.Id,
        //            UserId = userId,
        //            MyTryCount = 1,
        //            IsSucces = false
        //        };

        //        await _lectureTryRepository.InsertAsync(lectureTry, autoSave: true);
        //    }
        //    else
        //    {
        //        lectureTry.MyTryCount += 1;
        //    }

        //    double requiredRate = lecture.SuccessQuizRate / 100.0;
        //    lectureTry.IsSucces = totalScore > 0 && (studentScore / totalScore) >= requiredRate;

        //    await _lectureTryRepository.UpdateAsync(lectureTry, autoSave: true);

        //    var userTryCount = await UserTryCount(userId, lecture.Id, input.QuizId);

        //    return new ResponseApi<QuizResultDto>
        //    {
        //        Data = new QuizResultDto
        //        {
        //            QuizId = quiz.Id,
        //            TotalScore = totalScore,
        //            StudentScore = studentScore,
        //            MyTryCount = userTryCount?.Data?.MyTryCount ?? 0,
        //            LectureTryCount = userTryCount?.Data?.LectureTryCount ?? 0,
        //            IsSuccesful = userTryCount?.Data?.IsSucces ?? false,
        //        },
        //        Success = true,
        //        Message = "Quiz submitted and updated successfully"
        //    };
        //}



        public async Task<ResponseApi<QuizResultDto>> SubmitQuizAsync(QuizAnswerDto input, bool isExam)
        {
            var userId = _currentUser.GetId();
            List<Question> targetQuestions;
            double successRate = 0;
            Guid? lectureId = null;

            // 1. جلب البيانات وتحديد المصدر (Quiz vs Exam)
            if (isExam)
            {
                var exam = await _examRepository.GetAsync(input.QuizId);
                if (exam == null) throw new UserFriendlyException("Exam not found");

                var examData = await _examQuestionRepository.GetQuestionsByExamIdAsync(input.QuizId);
                targetQuestions = examData.Select(x => x.Question).ToList();
                successRate = exam.PassScore; // أو اجلبها من جدول الامتحان إذا كانت موجودة
            }
            else
            {
                var quiz = await (await _quizRepository.GetQueryableAsync())
                    .Include(q => q.Questions).ThenInclude(q => q.QuestionAnswers)
                    .Include(q => q.Questions).ThenInclude(q => q.QuestionType)
                    .Include(x => x.Lecture)
                    .FirstOrDefaultAsync(q => q.Id == input.QuizId);

                if (quiz == null) throw new UserFriendlyException("Quiz not found");
                if (quiz.Lecture == null) throw new UserFriendlyException("Lecture not found");

                targetQuestions = quiz.Questions.ToList();
                lectureId = quiz.LectureId;
                successRate = quiz.Lecture.SuccessQuizRate;
            }

            // 2. إدارة سجل محاولة الطالب (ExamStudent vs QuizStudent)
            Guid studentRecordId;
            if (isExam)
            {
                var examStudent = await _examStudentRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.ExamId == input.QuizId);
                if (examStudent != null)
                {
                    await _examStudentAnswerRepository.DeleteDirectAsync(x => x.ExamStudentId == examStudent.Id);
                    examStudent.TryCount += 1;
                }
                else
                {
                    examStudent = new ExamStudent { ExamId = input.QuizId, UserId = userId, TryCount = 1 };
                    await _examStudentRepository.InsertAsync(examStudent, autoSave: true);
                }
                studentRecordId = examStudent.Id;
            }
            else
            {
                var quizStudent = await _quizStudentRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.QuizId == input.QuizId);
                if (quizStudent != null)
                {
                    await _quizStudentAnswerRepository.DeleteDirectAsync(x => x.QuizStudentId == quizStudent.Id);
                    quizStudent.TryCount += 1;
                }
                else
                {
                    quizStudent = new QuizStudent { QuizId = input.QuizId, UserId = userId, TryCount = 1, LectureId = lectureId.Value };
                    await _quizStudentRepository.InsertAsync(quizStudent, autoSave: true);
                }
                studentRecordId = quizStudent.Id;
            }

            double totalScore = 0;
            double studentScore = 0;

            // 3. حلقة التصحيح وحفظ الإجابات
            foreach (var question in targetQuestions)
            {
                var studentAnswers = input.Answers.Where(a => a.QuestionId == question.Id).ToList();
                if (!studentAnswers.Any()) continue;

                bool isCorrect = false;
                double obtained = 0;

                // --- منطق التصحيح الخاص بك (نفس المنطق السابق) ---
                switch ((QuestionTypeEnum)question.QuestionType.Key)
                {
                    case QuestionTypeEnum.MCQ:
                    case QuestionTypeEnum.TrueOrFalse:
                        var selected = studentAnswers.FirstOrDefault(a => a.SelectedAnswerId != null);
                        var correct = question.QuestionAnswers.FirstOrDefault(a => a.IsCorrect);
                        if (correct != null && correct.Id == selected?.SelectedAnswerId)
                        {
                            isCorrect = true;
                            obtained = question.Score;
                            studentScore += question.Score;
                        }
                        break;

                    case QuestionTypeEnum.ShortAnswer:
                    case QuestionTypeEnum.CompleteAnswer:
                        var studentTexts = studentAnswers.Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
                                                         .Select(a => a.TextAnswer!.ToLower().Trim()).ToList();
                        var correctTexts = question.QuestionAnswers.Where(a => a.IsCorrect)
                                                         .Select(a => a.Answer.ToLower().Trim()).ToList();
                        if (studentTexts.Any(s => correctTexts.Contains(s)))
                        {
                            isCorrect = true;
                            obtained = question.Score;
                            studentScore += obtained;
                        }
                        break;
                }

                totalScore += question.Score;
                var textAns = string.Join(" | ", studentAnswers.Select(a => a.TextAnswer));

                // 4. الحفظ في الجدول الصحيح (ExamStudentAnswer vs QuizStudentAnswer)
                if (isExam)
                {
                    await _examStudentAnswerRepository.InsertAsync(new ExamStudentAnswer
                    {
                        ExamStudentId = studentRecordId,
                        QuestionId = question.Id,
                        SelectedAnswerId = studentAnswers.FirstOrDefault()?.SelectedAnswerId,
                        TextAnswer = textAns,
                        IsCorrect = isCorrect,
                        ScoreObtained = obtained
                    });
                }
                else
                {
                    await _quizStudentAnswerRepository.InsertAsync(new QuizStudentAnswer
                    {
                        QuizStudentId = studentRecordId,
                        QuestionId = question.Id,
                        SelectedAnswerId = studentAnswers.FirstOrDefault()?.SelectedAnswerId,
                        TextAnswer = textAns,
                        IsCorrect = isCorrect,
                        ScoreObtained = obtained
                    });
                }
            }

            // 5. تحديث الحالات النهائية (Success status & Scores)
            bool isPassed = totalScore > 0 && (studentScore / totalScore) >= (successRate / 100.0);

            if (isExam)
            {
                // 1. جلب البيانات مع الـ Include المناسب
                var examStudent = await (await _examStudentRepository.GetQueryableAsync())
                    .Include(x => x.Exam)
                    .FirstOrDefaultAsync(x => x.Id == studentRecordId);

                // 2. التحقق من وجود السجل ومن وجود بيانات الامتحان المرتبطة به
                if (examStudent != null && examStudent.Exam != null)
                {
                    // حساب الدرجة بناءً على النسبة المئوية
                    // ملحوظة: تأكد أن Score من نوع double أو decimal لتجنب مشاكل قسمة الأعداد الصحيحة
                    examStudent.Score = studentScore * (examStudent.Exam.Score / 100.0);

                    examStudent.IsPassed = isPassed;

                    // 3. تحديث البيانات
                    await _examStudentRepository.UpdateAsync(examStudent);
                }
                else
                {
                    // يمكنك هنا إضافة Log أو التعامل مع حالة عدم وجود البيانات
                     throw new EntityNotFoundException("Exam or Student record not found.");
                }
            }
            else
            {
                var quizStudent = await _quizStudentRepository.GetAsync(studentRecordId);
                quizStudent.Score = (int)Math.Round(studentScore);
                await _quizStudentRepository.UpdateAsync(quizStudent);

                // إدارة LectureTry (للكويز فقط)
                var lectureTry = await _lectureTryRepository.FirstOrDefaultAsync(x => x.LectureId == lectureId && x.UserId == userId);
                if (lectureTry == null)
                {
                    await _lectureTryRepository.InsertAsync(new LectureTry { LectureId = lectureId.Value, UserId = userId, MyTryCount = 1, IsSucces = isPassed });
                }
                else
                {
                    lectureTry.MyTryCount += 1;
                    lectureTry.IsSucces = isPassed;
                    await _lectureTryRepository.UpdateAsync(lectureTry);
                }
            }

            return new ResponseApi<QuizResultDto>
            {
                Data = new QuizResultDto
                {
                    QuizId = input.QuizId,
                    TotalScore = totalScore,
                    StudentScore = studentScore,
                    IsSuccesful = isPassed
                },
                Success = true,
                Message = "Submitted successfully"
            };
        }


        

        public async Task<ResponseApi<LectureTryDto>> UserTryCount(Guid userId, Guid lecId, Guid quizId)
        {
            var trys = await _lectureTryRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.LectureId == lecId);
            var lecture = await (await _lectureRepository.GetQueryableAsync()).Include(x => x.Quizzes).FirstOrDefaultAsync(x => x.Id == lecId);
            var isSucces = await _lectureTryRepository.AnyAsync(x => x.UserId == userId && x.LectureId == lecId && x.IsSucces == true);
            var quizStudent = await (await _quizStudentRepository.GetQueryableAsync()).FirstOrDefaultAsync(x => x.UserId == userId && x.LectureId == lecId && x.QuizId == quizId);
            var totalScore = await (await _questionRepository.GetQueryableAsync())
                .Where(x => x.QuizId == quizId)
                .SumAsync(x => (double?)x.Score) ?? 0;
            var myScoreRate = lecture?.SuccessQuizRate > 0 && quizStudent != null ? Math.Round(((double)quizStudent.Score / (double)totalScore) * 100, 2) : 0;
            var lecturetry = new LectureTryDto { MyTryCount = trys?.MyTryCount ?? 0, LectureTryCount = lecture?.QuizTryCount ?? 0 * lecture?.Quizzes?.Count ?? 0, IsSucces = isSucces, SuccessQuizRate = lecture?.SuccessQuizRate ?? 0, MyScoreRate = myScoreRate };
            return new ResponseApi<LectureTryDto> { Data = lecturetry, Message = "get count", Success = true };
        }

        public async Task<QuizFullDetailModel> GetFullDetailsAsync(Guid refId, bool isExam)
        {
            if (isExam)
            {
                var exam = await _examRepository.GetAsync(refId);
                if (exam == null) throw new UserFriendlyException("Exam not found");

                var examQuestions = await _examQuestionRepository.GetQuestionsByExamIdAsync(refId);

                return new QuizFullDetailModel
                {
                    Id = exam.Id,
                    Title = exam.Name,
                    QuizTime = exam.TimeExam,
                    TryCount = 0, // أو القيمة الموجودة في جدول الامتحان
                    Questions = examQuestions.Select(x => x.Question).ToList()
                };
            }
            else
            {
                var quizFull = await _quizRepository.GetQuizWithQuestionsAsync(refId);
                if (quizFull == null) throw new UserFriendlyException("Quiz not found");

                return new QuizFullDetailModel
                {
                    Id = quizFull.Id,
                    Title = quizFull.Title,
                    QuizTime = quizFull.QuizTime,
                    TryCount = quizFull.Lecture?.QuizTryCount ?? 0,
                    Questions = quizFull.Questions?.ToList() ?? new List<Question>()
                };
            }
        }
    }
}
