using Dev.Acadmy.AccountTypes;
using Dev.Acadmy.Chapters;
using Dev.Acadmy.Courses;
using Dev.Acadmy.Emails;
using Dev.Acadmy.Entities.Advertisementes.Entities;
using Dev.Acadmy.Entities.Chats.Entites;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Entities.Posts.Entities;
using Dev.Acadmy.Entities.Reports.Entities;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Questions;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Supports;
using Dev.Acadmy.Universites;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Dev.Acadmy.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AcadmyDbContext :
    AbpDbContext<AcadmyDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DbSet<Entities.Courses.Entities.Course> Courses { get; set; }
    public DbSet<CourseStudent> CourseStudents { get; set; }
    public DbSet<College> Colleges { get; set; }
    public DbSet<Lecture> Lectures { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizStudent> QuizStudents { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<QuestionBank> QuestionBanks { get; set; }
    public DbSet<QuestionType> QuestionTypes { get; set; }
    public DbSet<MediaItem> MediaItems { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<AccountType> AccountTypes { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<CourseInfo> CourseInfos { get; set; }
    public DbSet<Term> Terms { get; set; }  
    public DbSet<University> Universities { get; set; } 
    public DbSet<GradeLevel> GradeLevels { get; set; }
    public DbSet<LectureStudent> LectureStudents { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<LectureTry> LectureTrys { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamQuestionBank> ExamQuestionBanks { get; set; }
    public DbSet<QuizStudentAnswer> QuizStudentAnswer { get; set; }
    public DbSet<Support> Supports { get; set; }   
    public DbSet<ExamQuestion> ExamQuestions { get; set; }
    public DbSet<CourseFeedback> CourseFeedbacks { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<UserReport> UserReports { get; set; }
    public DbSet<CourseCertificate> CourseCertificates { get; set; }
    public AcadmyDbContext(DbContextOptions<AcadmyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        builder.ApplyConfigurationsFromAssembly(typeof(AcadmyDbContext).Assembly);
    }
}
