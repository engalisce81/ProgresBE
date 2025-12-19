namespace Dev.Acadmy.Permissions;

public static class AcadmyPermissions
{
    public const string GroupName = "Acadmy";

    public static class Courses
    {
        public const string Default = GroupName + ".Courses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }
    public static class Advertisements
    {
        public const string Default = GroupName + ".Advertisements";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
    
    // أضف هذه المجموعة للتقييمات
    public static class CourseFeedbacks
    {
        public const string Default = GroupName + ".CourseFeedbacks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Accept = Default + ".Accept"; // صلاحية القبول الخاصة بالأدمن
    }
    public static class Supports
    {
        public const string Default = GroupName + ".Supports";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }
    
    public static class Exams
    {
        public const string Default = GroupName + ".Exams";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }
    public static class Teachers
    {
        public const string Default = GroupName + ".Teachers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }

    
    public static class GradeLevels
    {
        public const string Default = GroupName + ".GradeLevels";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }

    public static class Students
    {
        public const string Default = GroupName + ".Students";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }

    public static class Universites
    {
        public const string Default = GroupName + ".Universites";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }
    public static class CourseInfos
    {
        public const string Default = GroupName + ".CourseInfos";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Publish = Default + ".Publish";
    }
    public static class Colleges
    {
        public const string Default = GroupName + ".Colleges";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }

    public static class Subjects
    {
        public const string Default = GroupName + ".Subjects";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
    public static class Chapters
    {
        public const string Default = GroupName + ".Chapters";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }

    public static class CourseStudents
    {
        public const string Default = GroupName + ".CourseStudents";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }

    public static class Lectures
    {
        public const string Default = GroupName + ".Lectures";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
    public static class MediaItems
    {
        public const string Default = GroupName + ".MediaItems";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
    public static class Questions
    {
        public const string Default = GroupName + ".Questions";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
    public static class QuestionAnswers
    {
        public const string Default = GroupName + ".QuestionAnswers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
    public static class QuestionBanks
    {
        public const string Default = GroupName + ".QuestionBanks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }

    public static class QuestionTypes
    {
        public const string Default = GroupName + ".QuestionTypes";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }

    public static class Quizs
    {
        public const string Default = GroupName + ".Quizs";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }

    public static class QuizStudents
    {
        public const string Default = GroupName + ".QuizStudents";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
    }
}
