namespace Everwell.API.Constants
{
    public class ApiEndpointConstants
    {
        static ApiEndpointConstants() {  }

        public const string RootEndpoint = "/api";
        public const string ApiVersion = "/v2.5";
        public const string ApiEndpoint = RootEndpoint + ApiVersion;

        public static class Auth
        {
            public const string AuthEndpoint = ApiEndpoint + "/auth";
            public const string LoginEndpoint = ApiEndpoint + "/login";
            public const string RegisterEndpoint = ApiEndpoint + "/register"; // to do
            public const string ChangePasswordEndpoint = ApiEndpoint + "/changepassword"; // to do
            public const string RefreshTokenEndpoint = ApiEndpoint + "/refreshtoken"; // to do
            public const string LogoutEndpoint = ApiEndpoint + "/logout"; // to do
        }

        public static class User
        {
            public const string UserEndpoint = ApiEndpoint + "/user";
            public const string GetUserEndpoint = UserEndpoint + "/{id}";
            public const string GetAllUsersEndpoint = UserEndpoint + "/getall";
            public const string GetUsersByRoleEndpoint = GetAllUsersEndpoint + "/{role}";
            public const string CreateUserEndpoint = UserEndpoint + "/create";
            public const string UpdateUserEndpoint = UserEndpoint + "/update/{id}";
            public const string DeleteUserEndpoint = UserEndpoint + "/delete/{id}";
            
            // Profile endpoints
            public const string GetMyProfileEndpoint = UserEndpoint + "/profile/me";
            public const string SetRoleEndpoint = UserEndpoint + "/set-role/{id}";
            public const string UpdateProfileEndpoint = UserEndpoint + "/profile/{id}";
            public const string UpdateMyProfileEndpoint = UserEndpoint + "/profile/me";
            public const string UpdateAvatarEndpoint = UserEndpoint + "/avatar/{id}";
            public const string UpdateMyAvatarEndpoint = UserEndpoint + "/avatar/me";
        }

        public static class Service
        {
            public const string ServiceEndpoint = ApiEndpoint + "/service";
            public const string GetAllServicesEndpoint = ServiceEndpoint + "/getall";
            public const string GetServiceEndpoint = ServiceEndpoint + "/{id}";
            public const string CreateServiceEndpoint = ServiceEndpoint + "/create";
            public const string UpdateServiceEndpoint = ServiceEndpoint + "/update/{id}";
            public const string DeleteServiceEndpoint = ServiceEndpoint + "/delete/{id}";
        }

        public static class Appointment
        {
            public const string AppointmentEndpoint = ApiEndpoint + "/appointment";
            public const string GetAllAppointmentsEndpoint = AppointmentEndpoint + "/getall";
            public const string GetAppointmentsByConsultantEndpoint = AppointmentEndpoint + "/consultant/{id}";
            public const string GetAppointmentEndpoint = AppointmentEndpoint + "/{id}";
            public const string CreateAppointmentEndpoint = AppointmentEndpoint + "/create";
            public const string UpdateAppointmentEndpoint = AppointmentEndpoint + "/update/{id}";
            public const string CancelAppointmentEndpoint = AppointmentEndpoint + "/cancel/{id}";
            public const string DeleteAppointmentEndpoint = AppointmentEndpoint + "/delete/{id}";
            public const string GetConsultantSchedulesEndpoint = AppointmentEndpoint + "/consultant/schedules";
            public const string GetConsultantSchedulesByIdEndpoint = AppointmentEndpoint + "/consultant/schedules/{id}";
            public const string CreateConsultantScheduleEndpoint = AppointmentEndpoint + "/consultant/schedule/create";
        }

        public static class Feedback
        {
            public const string FeedbackEndpoint = ApiEndpoint + "/feedback";
            public const string GetAllFeedbacksEndpoint = FeedbackEndpoint + "/getall";
            public const string GetFeedbackEndpoint = FeedbackEndpoint + "/{id}";
            public const string CreateFeedbackEndpoint = FeedbackEndpoint + "/create";
            public const string UpdateFeedbackEndpoint = FeedbackEndpoint + "/update/{id}";
            public const string DeleteFeedbackEndpoint = FeedbackEndpoint + "/delete/{id}";
            public const string GetPublicConsultantReviewsEndpoint = FeedbackEndpoint + "/consultant/{consultantId}/public";
        }

        public static class Post
        {
            public const string PostEndpoint = ApiEndpoint + "/post";
            public const string GetAllPostsEndpoint = PostEndpoint + "/getall";
            public const string GetFilteredPostsEndpoint = PostEndpoint + "/filter";
            public const string GetPostEndpoint = PostEndpoint + "/{id}";
            public const string CreatePostEndpoint = PostEndpoint + "/create";
            public const string UpdatePostEndpoint = PostEndpoint + "/update/{id}";
            public const string ApprovePostEndpoint = PostEndpoint + "/approve/{id}";
            public const string DeletePostEndpoint = PostEndpoint + "/delete/{id}";
        }

        public static class Question
        {
            public const string QuestionEndpoint = ApiEndpoint + "/question";
            public const string GetAllQuestionsEndpoint = QuestionEndpoint + "/getall";
            public const string GetQuestionEndpoint = QuestionEndpoint + "/{id}";
            public const string CreateQuestionEndpoint = QuestionEndpoint + "/create";
            public const string UpdateQuestionEndpoint = QuestionEndpoint + "/update/{id}";
            public const string DeleteQuestionEndpoint = QuestionEndpoint + "/delete/{id}";
        }

        public static class STITesting
        {
            public const string STITestingEndpoint = ApiEndpoint + "/stitesting";
            public const string GetAllSTITestingsEndpoint = STITestingEndpoint + "/getall";
            public const string GetSTITestingEndpoint = STITestingEndpoint + "/{id}";
            public const string GetSTITestingsByCurrentUserEndpoint = STITestingEndpoint + "/currentuser";
            public const string GetSTITestingsByCustomerEndpoint = STITestingEndpoint + "/customer/{customerId}";
            public const string CreateSTITestingEndpoint = STITestingEndpoint + "/create";
            public const string UpdateSTITestingEndpoint = STITestingEndpoint + "/update/{id}";
            public const string DeleteSTITestingEndpoint = STITestingEndpoint + "/delete/{id}";
        }

        public static class TestResult
        {
            public const string TestResultEndpoint = ApiEndpoint + "/testresult";
            public const string GetAllTestResultsEndpoint = TestResultEndpoint + "/getall";
            public const string GetTestResultEndpoint = TestResultEndpoint + "/{id}";
            public const string GetTestResultsBySTITestsEndpoint = TestResultEndpoint + "/testing/{stiTestingId}";
            public const string GetTestResultByCustomerEndpoint = TestResultEndpoint + "/customer/{id}";
            public const string CreateTestResultEndpoint = TestResultEndpoint + "/create";
            public const string UpdateTestResultEndpoint = TestResultEndpoint + "/update/{id}";
            public const string DeleteTestResultEndpoint = TestResultEndpoint + "/delete/{id}";
        }

        public static class MenstrualCycleTracking
        {
            public const string GetAllMenstrualCycleTrackingsEndpoint = "/api/menstrual-cycle-trackings";
            public const string GetMenstrualCycleTrackingEndpoint = "/api/menstrual-cycle-trackings/{id}";
            public const string CreateMenstrualCycleTrackingEndpoint = "/api/menstrual-cycle-trackings";
            public const string UpdateMenstrualCycleTrackingEndpoint = "/api/menstrual-cycle-trackings/{id}";
            public const string DeleteMenstrualCycleTrackingEndpoint = "/api/menstrual-cycle-trackings/{id}";
            public const string GetCycleHistoryEndpoint = "/api/menstrual-cycle-trackings/history";
            public const string PredictNextCycleEndpoint = "/api/menstrual-cycle-trackings/predict-next";
            public const string GetFertilityWindowEndpoint = "/api/menstrual-cycle-trackings/fertility-window";
            public const string GetCycleAnalyticsEndpoint = "/api/menstrual-cycle-trackings/analytics";
            public const string GetCycleInsightsEndpoint = "/api/menstrual-cycle-trackings/insights";
            public const string GetNotificationsEndpoint = "/api/menstrual-cycle-trackings/notifications";
            public const string UpdateNotificationPreferencesEndpoint = "/api/menstrual-cycle-trackings/notification-preferences";
            public const string GetCycleTrendsEndpoint = "/api/menstrual-cycle-trackings/trends";
        }

        public static class Dashboard
        {
            public const string DashboardEndpoint = ApiEndpoint + "/dashboard";
            public const string GetDashboardDataEndpoint = DashboardEndpoint + "/data";
            public const string GetDashboardStatsEndpoint = DashboardEndpoint + "/stats";
            public const string GetUsersByRoleEndpoint = DashboardEndpoint + "/users-by-role";
            public const string GetAppointmentsByStatusEndpoint = DashboardEndpoint + "/appointments-by-status";
        }
        
        public static class Notification
        {
            public const string NotificationEndpoint = ApiEndpoint + "/notification";
            public const string GetUserNotifications = NotificationEndpoint + "/user/{userId}";
            public const string MarkAsRead = NotificationEndpoint + "/mark/{id}";
            public const string DeleteNotificationEndpoint = NotificationEndpoint + "/delete/{id}";
        }
    }
}
