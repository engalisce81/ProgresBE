using Dev.Acadmy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Dev.Acadmy.Permissions;

public class AcadmyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
       // context.RemoveGroup(AcadmyPermissions.GroupName);
        var group = context.AddGroup(AcadmyPermissions.GroupName);

        var colleges = group.AddPermission(AcadmyPermissions.Colleges.Default, L("Permission:Colleges"));
        colleges.AddChild(AcadmyPermissions.Colleges.Create, L("Permission:Colleges.Create"));
        colleges.AddChild(AcadmyPermissions.Colleges.Edit, L("Permission:Colleges.Edit"));
        colleges.AddChild(AcadmyPermissions.Colleges.View, L("Permission:Colleges.View"));
        colleges.AddChild(AcadmyPermissions.Colleges.Delete, L("Permission:Colleges.Delete"));

        var gradelevels = group.AddPermission(AcadmyPermissions.GradeLevels.Default, L("Permission:GradeLevels"));
        gradelevels.AddChild(AcadmyPermissions.GradeLevels.Create, L("Permission:GradeLevels.Create"));
        gradelevels.AddChild(AcadmyPermissions.GradeLevels.Edit, L("Permission:GradeLevels.Edit"));
        gradelevels.AddChild(AcadmyPermissions.GradeLevels.View, L("Permission:GradeLevels.View"));
        gradelevels.AddChild(AcadmyPermissions.GradeLevels.Delete, L("Permission:GradeLevels.Delete"));

        var exams = group.AddPermission(AcadmyPermissions.Exams.Default, L("Permission:Exams"));
        exams.AddChild(AcadmyPermissions.Exams.Create, L("Permission:Exams.Create"));
        exams.AddChild(AcadmyPermissions.Exams.Edit, L("Permission:Exams.Edit"));
        exams.AddChild(AcadmyPermissions.Exams.View, L("Permission:Exams.View"));
        exams.AddChild(AcadmyPermissions.Exams.Delete, L("Permission:Exams.Delete"));

        var supporsts = group.AddPermission(AcadmyPermissions.Supports.Default, L("Permission:Supports"));
        supporsts.AddChild(AcadmyPermissions.Supports.Create, L("Permission:Supports.Create"));
        supporsts.AddChild(AcadmyPermissions.Supports.Edit, L("Permission:Supports.Edit"));
        supporsts.AddChild(AcadmyPermissions.Supports.View, L("Permission:Supports.View"));
        supporsts.AddChild(AcadmyPermissions.Supports.Delete, L("Permission:Supports.Delete"));

        var advertisements = group.AddPermission(AcadmyPermissions.Advertisements.Default, L("Permission:Advertisements"));
        advertisements.AddChild(AcadmyPermissions.Advertisements.Create, L("Permission:Advertisements.Create"));
        advertisements.AddChild(AcadmyPermissions.Advertisements.Edit, L("Permission:Advertisements.Edit"));
        advertisements.AddChild(AcadmyPermissions.Advertisements.View, L("Permission:Advertisements.View"));
        advertisements.AddChild(AcadmyPermissions.Advertisements.Delete, L("Permission:Advertisements.Delete"));

        var students = group.AddPermission(AcadmyPermissions.Students.Default, L("Permission:Students"));
        students.AddChild(AcadmyPermissions.Students.Create, L("Permission:Students.Create"));
        students.AddChild(AcadmyPermissions.Students.Edit, L("Permission:Students.Edit"));
        students.AddChild(AcadmyPermissions.Students.View, L("Permission:Students.View"));
        students.AddChild(AcadmyPermissions.Students.Delete, L("Permission:Students.Delete"));

        var teachers = group.AddPermission(AcadmyPermissions.Teachers.Default, L("Permission:Teachers"));
        teachers.AddChild(AcadmyPermissions.Teachers.Create, L("Permission:Teachers.Create"));
        teachers.AddChild(AcadmyPermissions.Teachers.Edit, L("Permission:Teachers.Edit"));
        teachers.AddChild(AcadmyPermissions.Teachers.View, L("Permission:Teachers.View"));
        teachers.AddChild(AcadmyPermissions.Teachers.Delete, L("Permission:Teachers.Delete"));

        // Courses
        var courses = group.AddPermission(AcadmyPermissions.Courses.Default, L("Permission:Courses"));
        courses.AddChild(AcadmyPermissions.Courses.Create, L("Permission:Courses.Create"));
        courses.AddChild(AcadmyPermissions.Courses.Edit, L("Permission:Courses.Edit"));
        courses.AddChild(AcadmyPermissions.Courses.Delete, L("Permission:Courses.Delete"));
        courses.AddChild(AcadmyPermissions.Courses.View, L("Permission:Courses.View"));
        courses.AddChild(AcadmyPermissions.Courses.Publish, L("Permission:Courses.Publish"));

        var courseFeedback = group.AddPermission(AcadmyPermissions.CourseFeedbacks.Default, L("Permission:CourseFeedbacks"));
        courseFeedback.AddChild(AcadmyPermissions.CourseFeedbacks.Create, L("Permission:CourseFeedbacks.Create"));
        courseFeedback.AddChild(AcadmyPermissions.CourseFeedbacks.Edit, L("Permission:CourseFeedbacks.Edit"));
        courseFeedback.AddChild(AcadmyPermissions.CourseFeedbacks.Delete, L("Permission:CourseFeedbacks.Delete"));
        courseFeedback.AddChild(AcadmyPermissions.CourseFeedbacks.View, L("Permission:CourseFeedbacks.View"));
        courseFeedback.AddChild(AcadmyPermissions.CourseFeedbacks.Accept, L("Permission:CourseFeedbacks.Accept"));

        // Chapters
        var chapters = group.AddPermission(AcadmyPermissions.Chapters.Default, L("Permission:Chapters"));
        chapters.AddChild(AcadmyPermissions.Chapters.Create, L("Permission:Chapters.Create"));
        chapters.AddChild(AcadmyPermissions.Chapters.Edit, L("Permission:Chapters.Edit"));
        chapters.AddChild(AcadmyPermissions.Chapters.Delete, L("Permission:Chapters.Delete"));
        chapters.AddChild(AcadmyPermissions.Chapters.View, L("Permission:Chapters.View"));

        // CourseStudents
        var courseStudents = group.AddPermission(AcadmyPermissions.CourseStudents.Default, L("Permission:CourseStudents"));
        courseStudents.AddChild(AcadmyPermissions.CourseStudents.Create, L("Permission:CourseStudents.Create"));
        courseStudents.AddChild(AcadmyPermissions.CourseStudents.Edit, L("Permission:CourseStudents.Edit"));
        courseStudents.AddChild(AcadmyPermissions.CourseStudents.Delete, L("Permission:CourseStudents.Delete"));
        courseStudents.AddChild(AcadmyPermissions.CourseStudents.View, L("Permission:CourseStudents.View"));

        // Lectures
        var lectures = group.AddPermission(AcadmyPermissions.Lectures.Default, L("Permission:Lectures"));
        lectures.AddChild(AcadmyPermissions.Lectures.Create, L("Permission:Lectures.Create"));
        lectures.AddChild(AcadmyPermissions.Lectures.Edit, L("Permission:Lectures.Edit"));
        lectures.AddChild(AcadmyPermissions.Lectures.Delete, L("Permission:Lectures.Delete"));
        lectures.AddChild(AcadmyPermissions.Lectures.View, L("Permission:Lectures.View"));

        // MediaItems
        var mediaItems = group.AddPermission(AcadmyPermissions.MediaItems.Default, L("Permission:MediaItems"));
        mediaItems.AddChild(AcadmyPermissions.MediaItems.Create, L("Permission:MediaItems.Create"));
        mediaItems.AddChild(AcadmyPermissions.MediaItems.Edit, L("Permission:MediaItems.Edit"));
        mediaItems.AddChild(AcadmyPermissions.MediaItems.Delete, L("Permission:MediaItems.Delete"));
        mediaItems.AddChild(AcadmyPermissions.MediaItems.View, L("Permission:MediaItems.View"));

        // Questions
        var questions = group.AddPermission(AcadmyPermissions.Questions.Default, L("Permission:Questions"));
        questions.AddChild(AcadmyPermissions.Questions.Create, L("Permission:Questions.Create"));
        questions.AddChild(AcadmyPermissions.Questions.Edit, L("Permission:Questions.Edit"));
        questions.AddChild(AcadmyPermissions.Questions.Delete, L("Permission:Questions.Delete"));
        questions.AddChild(AcadmyPermissions.Questions.View, L("Permission:Questions.View"));

        // QuestionAnswers
        var questionAnswers = group.AddPermission(AcadmyPermissions.QuestionAnswers.Default, L("Permission:QuestionAnswers"));
        questionAnswers.AddChild(AcadmyPermissions.QuestionAnswers.Create, L("Permission:QuestionAnswers.Create"));
        questionAnswers.AddChild(AcadmyPermissions.QuestionAnswers.Edit, L("Permission:QuestionAnswers.Edit"));
        questionAnswers.AddChild(AcadmyPermissions.QuestionAnswers.Delete, L("Permission:QuestionAnswers.Delete"));
        questionAnswers.AddChild(AcadmyPermissions.QuestionAnswers.View, L("Permission:QuestionAnswers.View"));

        // QuestionBanks
        var questionBanks = group.AddPermission(AcadmyPermissions.QuestionBanks.Default, L("Permission:QuestionBanks"));
        questionBanks.AddChild(AcadmyPermissions.QuestionBanks.Create, L("Permission:QuestionBanks.Create"));
        questionBanks.AddChild(AcadmyPermissions.QuestionBanks.Edit, L("Permission:QuestionBanks.Edit"));
        questionBanks.AddChild(AcadmyPermissions.QuestionBanks.Delete, L("Permission:QuestionBanks.Delete"));
        questionBanks.AddChild(AcadmyPermissions.QuestionBanks.View, L("Permission:QuestionBanks.View"));

        // QuestionTypes
        var questionTypes = group.AddPermission(AcadmyPermissions.QuestionTypes.Default, L("Permission:QuestionTypes"));
        questionTypes.AddChild(AcadmyPermissions.QuestionTypes.Create, L("Permission:QuestionTypes.Create"));
        questionTypes.AddChild(AcadmyPermissions.QuestionTypes.Edit, L("Permission:QuestionTypes.Edit"));
        questionTypes.AddChild(AcadmyPermissions.QuestionTypes.Delete, L("Permission:QuestionTypes.Delete"));
        questionTypes.AddChild(AcadmyPermissions.QuestionTypes.View, L("Permission:QuestionTypes.View"));

        // Quizs
        var quizs = group.AddPermission(AcadmyPermissions.Quizs.Default, L("Permission:Quizs"));
        quizs.AddChild(AcadmyPermissions.Quizs.Create, L("Permission:Quizs.Create"));
        quizs.AddChild(AcadmyPermissions.Quizs.Edit, L("Permission:Quizs.Edit"));
        quizs.AddChild(AcadmyPermissions.Quizs.Delete, L("Permission:Quizs.Delete"));
        quizs.AddChild(AcadmyPermissions.Quizs.View, L("Permission:Quizs.View"));

        // QuizStudents
        var quizStudents = group.AddPermission(AcadmyPermissions.QuizStudents.Default, L("Permission:QuizStudents"));
        quizStudents.AddChild(AcadmyPermissions.QuizStudents.Create, L("Permission:QuizStudents.Create"));
        quizStudents.AddChild(AcadmyPermissions.QuizStudents.Edit, L("Permission:QuizStudents.Edit"));
        quizStudents.AddChild(AcadmyPermissions.QuizStudents.Delete, L("Permission:QuizStudents.Delete"));
        quizStudents.AddChild(AcadmyPermissions.QuizStudents.View, L("Permission:QuizStudents.View"));

        // Subjects
        var subjects = group.AddPermission(AcadmyPermissions.Subjects.Default, L("Permission:Subjects"));
        subjects.AddChild(AcadmyPermissions.Subjects.Create, L("Permission:Subjects.Create"));
        subjects.AddChild(AcadmyPermissions.Subjects.Edit, L("Permission:Subjects.Edit"));
        subjects.AddChild(AcadmyPermissions.Subjects.Delete, L("Permission:Subjects.Delete"));
        subjects.AddChild(AcadmyPermissions.Subjects.View, L("Permission:Subjects.View"));

        // CourseInfos
        var courseInfos = group.AddPermission(AcadmyPermissions.CourseInfos.Default, L("Permission:CourseInfos"));
        courseInfos.AddChild(AcadmyPermissions.CourseInfos.Create, L("Permission:CourseInfos.Create"));
        courseInfos.AddChild(AcadmyPermissions.CourseInfos.Edit, L("Permission:CourseInfos.Edit"));
        courseInfos.AddChild(AcadmyPermissions.CourseInfos.Delete, L("Permission:CourseInfos.Delete"));
        courseInfos.AddChild(AcadmyPermissions.CourseInfos.View, L("Permission:CourseInfos.View"));

        // Universities
        var universities = group.AddPermission(AcadmyPermissions.Universites.Default, L("Permission:Universites"));
        universities.AddChild(AcadmyPermissions.Universites.Create, L("Permission:Universites.Create"));
        universities.AddChild(AcadmyPermissions.Universites.Edit, L("Permission:Universites.Edit"));
        universities.AddChild(AcadmyPermissions.Universites.Delete, L("Permission:Universites.Delete"));
        universities.AddChild(AcadmyPermissions.Universites.View, L("Permission:Universites.View"));


    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AcadmyResource>(name);
    }
}
