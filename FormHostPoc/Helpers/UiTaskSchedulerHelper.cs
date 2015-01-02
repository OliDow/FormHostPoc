using System.Threading.Tasks;

namespace FormHostPoc.Helpers{

    public class UiTaskSchedulerHelper
    {
        private static UiTaskSchedulerHelper _instance;
        public static UiTaskSchedulerHelper Instance
        {
            get { return _instance ?? (_instance = new UiTaskSchedulerHelper()); }
        }

        private readonly TaskScheduler _uiTaskScheduler;
        public TaskScheduler UiTaskScheduler
        {
            get { return _uiTaskScheduler; }
        }

        private UiTaskSchedulerHelper()
        {
            _uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }
    }
}